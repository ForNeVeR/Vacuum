module internal Vacuum.Console

open System
open System.Globalization
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

let presentSpace(bytes: int64): string =
    let float, size, measure =
        match double bytes with
        | x when x >= 2.0 ** 30.0 -> true, x / 2.0 ** 30.0, "GiB"
        | x when x >= 2.0 ** 20.0 -> true, x / 2.0 ** 20.0, "MiB"
        | x when x >= 2.0 ** 10.0 -> true, x / 2.0 ** 10.0, "kiB"
        | x -> false, x, "B"
    let formattedSize =
        if float
        then size.ToString("F2", CultureInfo.InvariantCulture)
        else size.ToString("0", CultureInfo.InvariantCulture)
    $"{formattedSize} {measure}"
