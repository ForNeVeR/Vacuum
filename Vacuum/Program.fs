module Vacuum.Program

open System

open Vacuum.Clean
open Vacuum.Commands
open Vacuum.FileSystem
open Vacuum.Console

let defaultPeriod = 30

[<EntryPoint>]
let main args =
    initializeConsole()

    match CommandLineParser.parse args with
    | Some options ->
        let directory = defaultArg options.Directory (Directory.getTempPath().RawPathString)
        let period = defaultArg options.Period defaultPeriod
        let date =
            if options.BytesToFree.IsNone
            then Some <| DateTime.UtcNow.AddDays(-(double period))
            else None
        let cleanMode =
            match options.Force, options.WhatIf with
            | false, false -> Normal
            | true, false -> ForceDelete
            | false, true -> WhatIf
            | true, true -> failwith "Invalid flags: both force and what-if modes are enabled."

        let parameters = {
            Directory = AbsolutePath.create directory
            Date = date
            BytesToFree = options.BytesToFree
            CleanMode = cleanMode
            Verbose = options.Verbose
        }
        let result = clean parameters

        info $"\nDirectory %s{result.Directory.RawPathString} cleaned up"
        match result.CleanedDate with
        | Some cleanedDate -> info $"  Report of cleaning up items older than {cleanedDate:s}"
        | None -> info "  Report of cleaning up items"

        info $"\n  Items before cleanup: %d{result.ItemsBefore}"
        info $"  Free disk space before cleanup: {presentSpace result.FreeDiskSpaceBefore}"
        if cleanMode <> WhatIf then
            info $"  Items after cleanup: %d{result.ItemsAfter}"
            let space = result.FreeDiskSpaceAfter |> Option.map presentSpace |> Option.get
            info $"  Free disk space after cleanup: {space}"

        let getResult r = defaultArg (Map.tryFind r result.States) 0
        let recycled = getResult Recycled
        let forceDeleted = getResult ForceDeleted
        let errors = getResult Error
        let scanErrors = getResult ScanError
        let skipped = getResult WillBeRemoved

        printf "\n"
        let printStatEntry color label number =
            printColor color $"  %s{label}: %d{number}"
        let printStatEntryOpt color label number =
            if number > 0 then printStatEntry color label number

        let printRecycledEntry = if cleanMode = WhatIf then printStatEntryOpt else printStatEntry
        let printWillBeRemovedEntry = if cleanMode = WhatIf then printStatEntry else printStatEntryOpt

        printRecycledEntry ConsoleColor.Green "Recycled" recycled
        printWillBeRemovedEntry ConsoleColor.Green "Will be removed" skipped
        printStatEntryOpt ConsoleColor.Green "Force deleted" forceDeleted
        printStatEntryOpt ConsoleColor.Red "Cannot delete" errors
        printStatEntryOpt ConsoleColor.Red "Scan errors" scanErrors

        info $"\n  Total time taken: %A{result.TimeTaken}"

        0
    | None -> 1
