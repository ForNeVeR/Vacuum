module Vacuum.Clean

open System
open System.Diagnostics

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
    Date: DateTime
    BytesToFree: int64 option
    CleanMode: CleanMode
}
    with
        static member Normal(directory: AbsolutePath, date: DateTime, ?bytesToFree: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            CleanMode = Normal
        }

        static member ForceDelete(directory: AbsolutePath, date: DateTime, ?bytesToFree: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            CleanMode = ForceDelete
        }

        static member WhatIf(directory: AbsolutePath, date: DateTime, ?bytesToFree: int64): CleanParameters = {
            Directory = directory
            Date = date
            BytesToFree = bytesToFree
            CleanMode = WhatIf
        }

type ProcessingStatus = Recycled | ForceDeleted | Error | ScanError | WillBeRemoved

type CleanResult = {
    Directory: AbsolutePath
    CleanedDate: DateTime
    ItemsBefore: int
    ItemsAfter: int
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

let private needToRemoveTopLevel date path: {| NeedToRemove: bool; HasScanError: bool |} =
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
        error $"Error when scanning %s{path.RawPathString}: %s{ex.Message}"
        {| NeedToRemove = false; HasScanError = true |}

let private forceRemove(item: IFileSystemItem) =
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
        reportError ex
        Error

let private remove mode (item: IFileSystemItem) =
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
        reportError ex

        if mode = ForceDelete then
            forceRemove item
        else Error

let rec private getEntrySize path =
    if File.exists path then
        NativeFunctions.getCompressedFileSize path
    else
        Directory.enumerateFileSystemEntriesRecursively path
        |> Seq.map getEntrySize
        |> Seq.sum

let private takeBytes bytes (files: AbsolutePath seq): IFileSystemItem seq =
    seq {
        let enumerator = files.GetEnumerator()
        try
            let mutable currentSize = 0L
            while currentSize < bytes && enumerator.MoveNext() do
                let entry = enumerator.Current
                let entrySize = getEntrySize entry

                currentSize <- currentSize + entrySize
                yield SizedFileSystemItem(entry, entrySize)
        finally
            enumerator.Dispose()
    }

let clean ({ Directory = directory
             Date = date
             BytesToFree = bytesToFree
             CleanMode = cleanMode }: CleanParameters): CleanResult =
    let stopwatch = Stopwatch()
    stopwatch.Start()

    info $"Cleaning directory %s{directory.RawPathString}"
    if bytesToFree.IsSome then
        info $"Cleaning %d{bytesToFree.Value} bytes"

    let itemsBefore = itemCount directory

    let allEntries = Directory.enumerateFileSystemEntries directory
    let filesToDelete, scanErrorCount =
        match bytesToFree with
        | Some bytes ->
            allEntries
            |> Seq.sortBy getLastFileAccessDate
            |> takeBytes bytes, 0
        | None ->
            let files = ResizeArray<IFileSystemItem>()
            let mutable errorCount = 0
            for entry in allEntries do
                let scanResult = needToRemoveTopLevel date entry
                if scanResult.NeedToRemove then files.Add(FileSystemItem(entry))
                if scanResult.HasScanError then errorCount <- errorCount + 1
            upcast files, errorCount

    let states =
        filesToDelete
        |> Seq.map(remove cleanMode)
        |> Seq.groupBy id
        |> Seq.map (fun (key, s) -> key, Seq.length s)
        |> Map.ofSeq
        |> Map.add ScanError scanErrorCount

    let itemsAfter = itemCount directory
    let time = stopwatch.Elapsed

    { Directory = directory
      CleanedDate = date
      ItemsBefore = itemsBefore
      ItemsAfter = itemsAfter
      States = states
      TimeTaken = time }
