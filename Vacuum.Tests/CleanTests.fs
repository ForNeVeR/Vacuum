module Vacuum.Tests.CleanTests

open System

open Xunit

open Vacuum
open Vacuum.Tests.Utils

[<Fact>]
let ``Cleaner should always clean the bytes it told to`` () =
    let M = 1024L * 1024L
    use directory = prepareEnvironment [ { Path = "file0.txt"; Date = DateTime (2010, 1, 1); Size = 10L * M }
                                         { Path = "file1.txt"; Date = DateTime (2011, 1, 1); Size = 10L * M }
                                         { Path = "file2.txt"; Date = DateTime (2011, 1, 1); Size = 10L * M } ]
    ignore <| Program.clean directory.Path (DateTime (2012, 1, 1)) (Some <| 15L * M)
    Assert.Equal<string> ([| "file2.txt" |], directory.GetFiles())

[<Fact>]
let ``Cleaner should not fail on long paths`` () =
    let fileName = String ('x', 251) + ".txt"
    use directory = prepareEnvironment [ { Path = fileName; Date = DateTime (2010, 1, 1); Size = 0L } ]
    ignore <| Program.clean directory.Path (DateTime (2011, 1, 1)) None
    Assert.Equal<string> ([| fileName |], directory.GetFiles()) // TODO: It should delete the file actually

[<Fact(Skip = "System call level is not correct")>]
let ``Cleaner should properly clean the directory trees with paths consisting of spaces``(): unit =
    use directory = prepareEnvironment [
        { Path = " / / /name.txt"; Date = DateTime(2010, 1, 1); Size = 0L }
    ]
    ignore <| Program.clean directory.Path (DateTime(2011, 1, 1)) None
    Assert.Empty(Array.ofSeq <| directory.GetFiles())
