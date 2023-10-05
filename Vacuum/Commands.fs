module Vacuum.Commands

open CommandLine

let private parseSuffix (s: string) =
    let allButLast = s.Substring (0, s.Length - 1)
    match s with
    | size when size.ToLowerInvariant().EndsWith("k") -> 1024L * (int64 <| allButLast)
    | size when size.ToLowerInvariant().EndsWith("m") -> 1024L * 1024L * (int64 <| allButLast)
    | size when size.ToLowerInvariant().EndsWith("g") -> 1024L * 1024L * 1024L * (int64 <| allButLast)
    | size -> int64 size

[<Verb("clean", HelpText = "Clean the temporary directory.")>]
type Clean =
    {
        [<Option(
            'd',
            "directory",
            HelpText = "Temporary directory path. Will be calculated if not defined.")>]
        Directory: string option

        [<Option(
            'p',
            "period",
            HelpText = "Number of days for item to stay untouched before being cleaned up. Default period is 30 days.")>]
        Period: int option

        [<Option(
            's',
            "space",
            HelpText = "Amount of space to be freed disregard the dates. Off by default. Supports k, m and g postfix. For example, 10k = 10 kibibytes, 10m = 10 mebibytes, 10g = 10 gibibytes.")>]
        Space: string option

        [<Option(
            'F',
            "free",
            HelpText = "Free until the disk has this amount of space left. Off by default. Supports k, m and g postfix. For example, 10g = 10 gibibytes." )>]
        Free: string option

        [<Option(
            'f',
            "force",
            HelpText = "Force to delete entries that weren't movable into the recycle bin.")>]
        Force: bool

        [<Option(
            'w',
            "what-if",
            HelpText = "Only collects the files that will be removed, without actually removing anything.")>]
        WhatIf: bool

        [<Option(
            'v',
            "verbose",
            HelpText = "Verbose error messages.")>]
        Verbose: bool
    }

    member this.BytesToFree =
        this.Space |> Option.map parseSuffix
    
    member this.FreeUntil =
        this.Free |> Option.map parseSuffix
