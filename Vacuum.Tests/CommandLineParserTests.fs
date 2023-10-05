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
    let G = M * 1024L
    test "10" 10L
    test "10k" (10L * k)
    test "10m" (10L * M)
    test "10M" (10L * M)
    test "10g" (10L * G)
    test "10G" (10L * G)

[<Fact>]
let ``--free parameter should be parsed with postfix`` () =
    let test text result =
        let args = [| "--free"; text |]
        let options = parseCommandLine args
        Assert.Equal (result, Option.get options.FreeUntil)

    let k = 1024L
    let M = k * 1024L
    test "10" 10L
    test "10k" (10L * k)
    test "10m" (10L * M)
    test "10M" (10L * M)

[<Fact>]
let ``--force parameter should be parsed``(): unit =
    Assert.False((parseCommandLine Array.empty).Force)
    Assert.True((parseCommandLine [|"--force"|]).Force)
