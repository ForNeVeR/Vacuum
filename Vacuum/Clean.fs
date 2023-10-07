module Vacuum.Clean

open System
open System.Diagnostics
open System.IO

open Vacuum
open Vacuum.FileSystem
open Vacuum.Console

type CleanMode =
    | Normal
    /// Force to delete items when they are impossible to move to the recycle bin.
    | ForceDelete
    /// Only print the names of the files that will be recycled; do not delete anything actually.
    | WhatIf

type CleanParameters = {
    Directory: AbsolutePath
    Date: DateTime option
    BytesToFree: int64 option
    FreeUntil: int64 option
    CleanMode: CleanMode
    Verbose: bool
}
    with
        static member Normal(directory: AbsolutePath, ?date: DateTime, ?bytesToFree: int64, ?freeUntil: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            FreeUntil = freeUntil
            CleanMode = Normal
            Verbose = false
        }

        static member ForceDelete(directory: AbsolutePath, ?date: DateTime, ?bytesToFree: int64, ?freeUntil: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            FreeUntil = freeUntil
            CleanMode = ForceDelete
            Verbose = false
        }

        static member WhatIf(directory: AbsolutePath, ?date: DateTime, ?bytesToFree: int64, ?freeUntil: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            FreeUntil = freeUntil
            CleanMode = WhatIf
            Verbose = false
        }

type ProcessingStatus = Recycled | ForceDeleted | Error | ScanError | WillBeRemoved

type CleanResult = {
    Directory: AbsolutePath
    CleanedDate: DateTime option
    ItemsBefore: int
    ItemsAfter: int
    FreeDiskSpaceBefore: int64
    FreeDiskSpaceAfter: int64 option
    States: Map<ProcessingStatus, int>
    TimeTaken: TimeSpan
}

let private itemCount =
    Directory.enumerateFileSystemEntries >> Seq.length

let private getLastFileAccessDate path =
    [| File.getCreationTimeUtc; File.getLastWriteTimeUtc |]
    |> Array.map (fun x -> x path)
    |> Array.max

let private lastTouchedEarlierThan date path =
    getLastFileAccessDate path < date

let private needToRemoveTopLevel verbose date path: {| NeedToRemove: bool; HasScanError: bool |} =
    try
        let needToRemove =
            if File.exists path then
                lastTouchedEarlierThan date path
            else if ReparsePoint.exists path then
                lastTouchedEarlierThan date path
            else
                [| Seq.singleton path
                   Directory.enumerateFileSystemEntriesRecursively path |]
                |> Seq.concat
                |> Seq.forall (lastTouchedEarlierThan date)
        {| NeedToRemove = needToRemove; HasScanError = false |}
    with
    | :? UnauthorizedAccessException -> {| NeedToRemove = false; HasScanError = false |}
    | ex ->
        reportError verbose (Some $"Error when scanning {path.RawPathString}") ex
        {| NeedToRemove = false; HasScanError = true |}

let private forceRemove verbose (item: IFileSystemItem) =
    try
        if ReparsePoint.exists item.Path then
            printf $"Forced deletion of a reparse point %s{item.Present()}… "
            ReparsePoint.delete item.Path
        else if File.exists item.Path then
            printf $"Forced deletion of a file %s{item.Present()}… "
            File.delete item.Path
        else
            printf $"Forced deletion of a directory %s{item.Present()}… "
            Directory.deleteRecursive item.Path

        ok()
        ForceDeleted
    with
    | ex ->
        reportError verbose None ex
        Error

let private remove verbose mode (item: IFileSystemItem) =
    let conditionalRemove removeAction itemType =
        match mode with
        | Normal | ForceDelete ->
            printf $"Removing {itemType} {item.Present()}… "
            removeAction item.Path
            ok()
            Recycled
        | WhatIf ->
            printfn $"Will be removed: {itemType} {item.Present()}."
            WillBeRemoved

    try
        let removalResult =
            if ReparsePoint.exists item.Path then
                conditionalRemove ReparsePoint.recycle "reparse point"
            else if File.exists item.Path then
                conditionalRemove File.recycle "file"
            else
                conditionalRemove Directory.recycle "directory"

        removalResult
    with
    | ex ->
        reportError verbose None ex

        if mode = ForceDelete then
            forceRemove verbose item
        else Error

let rec private getEntrySize path =
    if File.exists path then
        NativeFunctions.getCompressedFileSize path
    else
        Directory.enumerateFileSystemEntriesRecursively path
        |> Seq.filter File.exists
        |> Seq.map getEntrySize
        |> Seq.sum

let private takeBytes bytes (files: AbsolutePath seq): IFileSystemItem seq =
    seq {
        use enumerator = files.GetEnumerator()
        let mutable currentSize = 0L
        while currentSize < bytes && enumerator.MoveNext() do
            let entry = enumerator.Current
            let entrySize = getEntrySize entry

            currentSize <- currentSize + entrySize
            yield SizedFileSystemItem(entry, entrySize)
    }

let clean({
    Directory = directory
    Date = date
    BytesToFree = bytesToFree
    FreeUntil = freeUntil
    CleanMode = cleanMode
    Verbose = verbose
}: CleanParameters): CleanResult =
    let stopwatch = Stopwatch()
    stopwatch.Start()

    let itemsBefore = itemCount directory
    let diskRoot = Path.GetPathRoot directory.RawPathString
    let freeDiskSpaceBefore = NativeFunctions.getFreeDiskSpace diskRoot

    let bytesToFree' =
        match bytesToFree, freeUntil with
        | Some _, None -> bytesToFree
        | None, Some bytes ->
            bytes - freeDiskSpaceBefore |> max 0 |> Some
        | None, None -> None
        | Some _, Some _ -> failwith "Invalid parameters: both bytesToFree and freeUntil are passed."
    
    info $"Cleaning directory %s{directory.RawPathString}"
    bytesToFree' |> Option.iter (fun bytes -> info $"Cleaning %d{bytes} bytes")

    let allEntries = Directory.enumerateFileSystemEntries directory
    let filesToDelete, scanErrorCount =
        match date, bytesToFree' with
        | Some date, None ->
            let files = ResizeArray<IFileSystemItem>()
            let mutable errorCount = 0
            for entry in allEntries do
                let scanResult = needToRemoveTopLevel verbose date entry
                if scanResult.NeedToRemove then files.Add(FileSystemItem(entry))
                if scanResult.HasScanError then errorCount <- errorCount + 1
            files :> _ seq, errorCount
        | None, Some bytes ->
            allEntries
            |> Seq.sortBy getLastFileAccessDate
            |> takeBytes bytes, 0
        | _, _ -> failwith "Invalid parameters: exactly one of date, bytesToFree and freeUntil should be passed."


    let states =
        filesToDelete
        |> Seq.map(remove verbose cleanMode)
        |> Seq.groupBy id
        |> Seq.map (fun (key, s) -> key, Seq.length s)
        |> Map.ofSeq
        |> Map.add ScanError scanErrorCount

    let itemsAfter = itemCount directory
    let freeDiskSpaceAfter = if cleanMode = WhatIf then None else Some <| NativeFunctions.getFreeDiskSpace diskRoot
    let time = stopwatch.Elapsed

    { Directory = directory
      CleanedDate = date
      ItemsBefore = itemsBefore
      ItemsAfter = itemsAfter
      FreeDiskSpaceBefore = freeDiskSpaceBefore
      FreeDiskSpaceAfter = freeDiskSpaceAfter
      States = states
      TimeTaken = time }
