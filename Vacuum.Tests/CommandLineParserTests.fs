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

    let k = 1024
    let M = k * 1024
    test "10" 10
    test "10k" (10 * k)
    test "10m" (10 * M)
    test "10M" (10 * M)
