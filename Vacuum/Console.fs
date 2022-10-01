module internal Vacuum.Console

open System
open System.Text

let initializeConsole(): unit =
    Console.OutputEncoding <- Encoding.UTF8

let printColor color (message: string): unit =
    let oldColor = Console.ForegroundColor
    Console.ForegroundColor <- color
    Console.WriteLine message
    Console.ForegroundColor <- oldColor

let info: string -> unit = printColor ConsoleColor.White
let ok(): unit = printColor ConsoleColor.Green "ok"
let error: string -> unit = printColor ConsoleColor.Red

let reportError (verbose: bool) (title: string option) (ex: Exception): unit =
    let prefix = title |> Option.map(fun t -> $"{t}: ")
    error $"{prefix}{ex.Message}"
    if verbose then
        Console.Error.WriteLine(ex.ToString())
