module Vacuum.Tests.CommandLineParserTests

open Xunit

open Vacuum

let parseCommandLine = CommandLineParser.parse >> Option.get

[<Fact>]
let ``--directory parameter should be parsed`` () =
    let directory = "C:\\Nonexistent"
    let args = [| "--directory"; directory |]
    let options = parseCommandLine args
    Assert.Equal (Some directory, options.Directory)

[<Fact>]
let ``--space parameter should be parsed with postfix`` () =
    let test text result =
        let args = [| "--space"; text |]
        let options = parseCommandLine args
        Assert.Equal (result, Option.get options.BytesToFree)

    let k = 1024L
    let M = k * 1024L
    test "10" 10L
    test "10k" (10L * k)
    test "10m" (10L * M)
    test "10M" (10L * M)
