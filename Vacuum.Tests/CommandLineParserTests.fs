module Vacuum.Tests.CommandLineParserTests

open Xunit

open Vacuum

[<Fact>]
let ``--directory parameter should be parsed`` () =
    let directory = "C:\\Nonexistent"
    let args = [| "--directory"; directory |]
    let options = CommandLineParser.parse args |> Option.get
    Assert.Equal (Some directory, options.Directory)
