module Vacuum.Program

open System
open System.Diagnostics
open System.IO

open CommandLine
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

let lastTouchedEarlierThan date path =
    File.GetCreationTimeUtc path < date
    && File.GetLastAccessTimeUtc path < date
    && File.GetLastWriteTimeUtc path < date

let private needToRemoveTopLevel date path =
    try
        if File.Exists path then
            lastTouchedEarlierThan date path
        else
            [| Seq.singleton path
               Directory.EnumerateFileSystemEntries (path, "*.*", SearchOption.AllDirectories) |]
            |> Seq.concat
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

        Vacuum.Error

let private clean directory (period : int) =
    let stopwatch = Stopwatch ()
    stopwatch.Start ()

    info (sprintf "Cleaning directory %s" directory)

    let itemsBefore = itemCount directory
    let date = DateTime.UtcNow.AddDays (-(double period))

    let states =
        Directory.EnumerateFileSystemEntries directory
        |> Seq.filter (needToRemoveTopLevel date)
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
    match Parser.Default.ParseArguments<Clean> args with
    | :? Parsed<Clean> as command ->
        let options = command.Value
        let directory = defaultArg options.Directory (Path.GetTempPath ())
        let period = defaultArg options.Period defaultPeriod
        let result = clean directory period

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
    | :? NotParsed<Clean> -> 1
    | other -> failwithf "Internal error: unknown argument parse result %A" other
