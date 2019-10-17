module Vacuum.Program

open System
open System.Diagnostics
open System.IO

open Microsoft.VisualBasic.FileIO

open Vacuum.Commands

let defaultPeriod = 30

let printColor color (message : string) =
    let oldColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    Console.WriteLine message
    Console.ForegroundColor <- oldColor

let private info = printColor ConsoleColor.White
let private ok () = printColor ConsoleColor.Green "ok"
let private error = printColor ConsoleColor.Red

let private itemCount path =
    (Directory.GetFileSystemEntries path).Length

let private getLastFileAccessDate path =
    [| File.GetCreationTimeUtc; File.GetLastAccessTimeUtc; File.GetLastWriteTimeUtc |]
    |> Array.map (fun x -> x path)
    |> Array.max

let private lastTouchedEarlierThan date path =
    getLastFileAccessDate path < date

let private needToRemoveTopLevel date path =
    try
        if File.Exists path then
            lastTouchedEarlierThan date path
        else
            [| Seq.singleton path
               Directory.EnumerateFileSystemEntries (path, "*.*", IO.SearchOption.AllDirectories) |]
            |> Seq.concat
            |> Seq.forall (lastTouchedEarlierThan date)
    with
    | :? UnauthorizedAccessException -> false
    | ex ->
        let message = sprintf "Error when scanning %s" path
        raise <| Exception(message, ex)

let private remove item =
    try
        if File.Exists item then
            printf "Removing file %s... " item
            FileSystem.DeleteFile (item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)
        else
            printf "Removing directory %s... " item
            FileSystem.DeleteDirectory (item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

        ok ()

        Ok
    with
    | ex ->
        error (ex.Message)
        Console.Error.WriteLine (ex.ToString ())

        Vacuum.Error

let rec private getEntrySize path =
    if File.Exists path then
        NativeFunctions.getCompressedFileSize path
    else
        Directory.EnumerateFileSystemEntries (path, "*.*", IO.SearchOption.AllDirectories)
        |> Seq.map getEntrySize
        |> Seq.sum

let private takeBytes bytes (files : string seq) =
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

let clean (directory : string) (date : DateTime) (bytesToFree : int64 option) : CleanResult =
    let stopwatch = Stopwatch ()
    stopwatch.Start ()

    info (sprintf "Cleaning directory %s" directory)
    if bytesToFree.IsSome then
        info (sprintf "Cleaning %d bytes" bytesToFree.Value)

    let itemsBefore = itemCount directory

    let allEntries = Directory.EnumerateFileSystemEntries directory
    let filesToDelete =
        match bytesToFree with
        | Some bytes ->
            allEntries
            |> Seq.sortBy getLastFileAccessDate
            |> takeBytes bytes
        | None ->
            allEntries
            |> Seq.filter (needToRemoveTopLevel date)

    let states =
        filesToDelete
        |> Seq.map remove
        |> Seq.groupBy id
        |> Seq.map (fun (key, s) -> key, Seq.length s)
        |> Map.ofSeq

    let itemsAfter = itemCount directory
    let time = stopwatch.Elapsed

    { Directory = directory
      CleanedDate = date
      ItemsBefore = itemsBefore
      ItemsAfter = itemsAfter
      States = states
      TimeTaken = time }

[<EntryPoint>]
let main args =
    match CommandLineParser.parse args with
    | Some options ->
        let directory = defaultArg options.Directory (Path.GetTempPath ())
        let period = defaultArg options.Period defaultPeriod
        let date = DateTime.UtcNow.AddDays (-(double period))
        let result = clean directory date options.BytesToFree

        info (sprintf "\nDirectory %s cleaned up" result.Directory)
        info (sprintf "  Cleaned up files older than %s" (result.CleanedDate.ToString "s"))

        info (sprintf "\n  Items before cleanup: %d" result.ItemsBefore)
        info (sprintf "  Items after cleanup: %d" result.ItemsAfter)

        let getResult r = defaultArg (Map.tryFind r result.States) 0
        let successes = getResult Ok
        let errors = getResult Vacuum.Error

        printColor ConsoleColor.Green (sprintf "\n  Finished ok: %d" successes)
        printColor ConsoleColor.Red (sprintf "  Finished with error: %d" errors)

        info (sprintf "\n  Total time taken: %A" result.TimeTaken)

        0
    | None -> 1
