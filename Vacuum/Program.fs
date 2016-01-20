open System
open System.IO

open Microsoft.VisualBasic.FileIO

let info (message : string) =
    let color = Console.ForegroundColor
    Console.ForegroundColor <- ConsoleColor.White
    Console.WriteLine message

let lastTouchedEarlierThan date path =
    File.GetCreationTimeUtc path < date
    && File.GetLastAccessTimeUtc path < date
    && File.GetLastWriteTimeUtc path < date

let needToRemoveTopLevel date path =
    if File.Exists path then
        lastTouchedEarlierThan date path
    else
        Directory.EnumerateFileSystemEntries (path, "*.*", SearchOption.AllDirectories)
        |> Seq.forall (lastTouchedEarlierThan date)

let remove item =
    if File.Exists item then
        printf "Removing file %s..." item
        FileSystem.DeleteFile (item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)
    else 
        printf "Removing directory %s..." item
        FileSystem.DeleteDirectory (item, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

    printfn "ok"

let clean directory =
    info <| sprintf "Cleaning directory %s" directory

    let date = DateTime.UtcNow.AddMonths -1

    Directory.EnumerateFileSystemEntries directory
    |> Seq.filter (needToRemoveTopLevel date)
    |> Seq.iter remove

[<EntryPoint>]
let main _ = 
    let directory = Path.GetTempPath ()
    clean directory
    0
