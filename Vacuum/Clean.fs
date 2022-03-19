module Vacuum.Clean

open System
open System.Diagnostics

open Vacuum.FileSystem
open Vacuum.Console

type CleanMode =
    | Normal
    /// Force to delete items when they are impossible to move to the recycle bin.
    | ForceDelete

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

type ProcessingStatus = Recycled | ForceDeleted | Error | ScanError

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

let private forceRemove item =
    try
        if ReparsePoint.exists item then
            printf $"Forced deletion of a reparse point %s{item.RawPathString}… "
            ReparsePoint.delete item
        else if File.exists item then
            printf $"Forced deletion of a file %s{item.RawPathString}… "
            File.delete item
        else
            printf $"Forced deletion of a directory %s{item.RawPathString}… "
            Directory.deleteRecursive item

        ok()
        ForceDeleted
    with
    | ex ->
        reportError ex
        Error

let private remove force item =
    try
        if ReparsePoint.exists item then
            printf $"Removing reparse point %s{item.RawPathString}… "
            ReparsePoint.recycle item
        else if File.exists item then
            printf $"Removing file %s{item.RawPathString}… "
            File.recycle item
        else
            printf $"Removing directory %s{item.RawPathString}… "
            Directory.recycle item

        ok ()

        Recycled
    with
    | ex ->
        reportError ex

        if force then
            forceRemove item
        else Error

let rec private getEntrySize path =
    if File.exists path then
        NativeFunctions.getCompressedFileSize path
    else
        Directory.enumerateFileSystemEntriesRecursively path
        |> Seq.map getEntrySize
        |> Seq.sum

let private takeBytes bytes (files: AbsolutePath seq) =
    seq {
        let enumerator = files.GetEnumerator()
        try
            let mutable currentSize = 0L
            while currentSize < bytes && enumerator.MoveNext() do
                let entry = enumerator.Current
                currentSize <- currentSize + getEntrySize entry
                yield entry
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
            let files = ResizeArray()
            let mutable errorCount = 0
            for entry in allEntries do
                let scanResult = needToRemoveTopLevel date entry
                if scanResult.NeedToRemove then files.Add entry
                if scanResult.HasScanError then errorCount <- errorCount + 1
            upcast files, errorCount

    let forceDelete = cleanMode = ForceDelete
    let states =
        filesToDelete
        |> Seq.map(remove forceDelete)
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
