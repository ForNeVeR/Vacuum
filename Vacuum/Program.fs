module Vacuum.Program

open System
open System.Diagnostics
open System.Text

open Vacuum.Commands
open Vacuum.FileSystem

let defaultPeriod = 30

let printColor color (message : string) =
    let oldColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    Console.WriteLine message
    Console.ForegroundColor <- oldColor

let private info = printColor ConsoleColor.White
let private ok () = printColor ConsoleColor.Green "ok"
let private error = printColor ConsoleColor.Red

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

let private reportError(ex: Exception) =
    error ex.Message
    Console.Error.WriteLine (ex.ToString ())

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

let clean (directory: AbsolutePath) (date: DateTime) (bytesToFree: int64 option) (forceDelete: bool): CleanResult =
    let stopwatch = Stopwatch ()
    stopwatch.Start ()

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

let initializeConsole() =
    Console.OutputEncoding <- Encoding.UTF8

[<EntryPoint>]
let main args =
    initializeConsole()

    match CommandLineParser.parse args with
    | Some options ->
        let directory = defaultArg options.Directory (Directory.getTempPath().RawPathString)
        let period = defaultArg options.Period defaultPeriod
        let date = DateTime.UtcNow.AddDays (-(double period))
        let result = clean (AbsolutePath.create directory) date options.BytesToFree options.Force

        info $"\nDirectory %s{result.Directory.RawPathString} cleaned up"
        info (sprintf "  Cleaned up files older than %s" (result.CleanedDate.ToString "s"))

        info $"\n  Items before cleanup: %d{result.ItemsBefore}"
        info $"  Items after cleanup: %d{result.ItemsAfter}"

        let getResult r = defaultArg (Map.tryFind r result.States) 0
        let recycled = getResult Recycled
        let forceDeleted = getResult ForceDeleted
        let errors = getResult Error
        let scanErrors = getResult ScanError

        printf "\n"
        let printStatEntry color label number =
            printColor color $"  %s{label}: %d{number}"
        let printStatEntryOpt color label number =
            if number > 0 then printStatEntry color label number

        printStatEntry ConsoleColor.Green "Recycled" recycled
        printStatEntryOpt ConsoleColor.Green "Force deleted" forceDeleted
        printStatEntryOpt ConsoleColor.Red "Cannot delete" errors
        printStatEntryOpt ConsoleColor.Red "Scan errors" scanErrors

        info $"\n  Total time taken: %A{result.TimeTaken}"

        0
    | None -> 1
