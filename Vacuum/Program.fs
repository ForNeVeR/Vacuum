open System
open System.IO

open Microsoft.VisualBasic.FileIO

type RemoveResult = Ok | Error

let printColor color (message : string) =
    let oldColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    Console.WriteLine message
    Console.ForegroundColor <- oldColor

let info = printColor ConsoleColor.White
let ok () = printColor ConsoleColor.Green "ok"
let error = printColor ConsoleColor.Red

let lastTouchedEarlierThan date path =
    File.GetCreationTimeUtc path < date
    && File.GetLastAccessTimeUtc path < date
    && File.GetLastWriteTimeUtc path < date

let needToRemoveTopLevel date path =
    try
        if File.Exists path then
            lastTouchedEarlierThan date path
        else
            Directory.EnumerateFileSystemEntries (path, "*.*", SearchOption.AllDirectories)
            |> Seq.forall (lastTouchedEarlierThan date)
    with
    | :? UnauthorizedAccessException -> false        

let remove item =
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

let clean directory =
    info (sprintf "Cleaning directory %s" directory)

    let date = DateTime.UtcNow.AddMonths -1

    Directory.EnumerateFileSystemEntries directory
    |> Seq.filter (needToRemoveTopLevel date)
    |> Seq.map remove
    |> Seq.groupBy id
    |> Seq.map (fun (key, s) -> (key, Seq.length s))
    |> Map.ofSeq

[<EntryPoint>]
let main _ = 
    let directory = Path.GetTempPath ()
    let results = clean directory
    let getResult r = defaultArg (Map.tryFind r results) 0
    let successes = getResult Ok
    let errors = getResult Error

    printColor ConsoleColor.Green (sprintf "\nFinished ok: %d" successes)
    printColor ConsoleColor.Red (sprintf "Finished with error: %d" errors)

    0
