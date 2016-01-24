module Vacuum.Program

open System
open System.Diagnostics
open System.IO

open Microsoft.VisualBasic.FileIO

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

let lastTouchedEarlierThan date path =
    File.GetCreationTimeUtc path < date
    && File.GetLastAccessTimeUtc path < date
    && File.GetLastWriteTimeUtc path < date

let private needToRemoveTopLevel date path =
    try
        if File.Exists path then
            lastTouchedEarlierThan date path
        else
            Directory.EnumerateFileSystemEntries (path, "*.*", SearchOption.AllDirectories)
            |> Seq.forall (lastTouchedEarlierThan date)
    with
    | :? UnauthorizedAccessException -> false

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

        Error

let private clean directory =
    let stopwatch = Stopwatch ()
    stopwatch.Start ()

    info (sprintf "Cleaning directory %s" directory)

    let itemsBefore = itemCount directory
    let date = DateTime.UtcNow.AddMonths -1

    let states =
        Directory.EnumerateFileSystemEntries directory
        |> Seq.filter (needToRemoveTopLevel date)
        |> Seq.map remove
        |> Seq.groupBy id
        |> Seq.map (fun (key, s) -> (key, Seq.length s))
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
let main _ =
    let directory = Path.GetTempPath ()
    let result = clean directory

    info (sprintf "\nDirectory %s cleaned up" result.Directory)
    info (sprintf "  Cleaned up files older than %s" (result.CleanedDate.ToString "s"))

    info (sprintf "\n  Items before cleanup: %d" result.ItemsBefore)
    info (sprintf "  Items after cleanup: %d" result.ItemsAfter)

    let getResult r = defaultArg (Map.tryFind r result.States) 0
    let successes = getResult Ok
    let errors = getResult Error

    printColor ConsoleColor.Green (sprintf "\n  Finished ok: %d" successes)
    printColor ConsoleColor.Red (sprintf "  Finished with error: %d" errors)

    info (sprintf "\n  Total time taken: %A" result.TimeTaken)

    0
