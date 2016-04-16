module Vacuum.Tests.CleanTests

open System

open Xunit

open Vacuum
open Vacuum.Tests.Utils

[<Fact>]
let ``Cleaner should always clean the bytes it told to`` () =
    let M = 1024L * 1024L
    use directory = prepareEnvironment [ { Name = "file0.txt"; Date = DateTime (2010, 1, 1); Size = 10L * M }
                                         { Name = "file1.txt"; Date = DateTime (2011, 1, 1); Size = 10L * M }
                                         { Name = "file2.txt"; Date = DateTime (2011, 1, 1); Size = 10L * M } ]
    let result = Program.clean directory.Path (DateTime (2010, 1, 1)) (Some <| 15L * M)
    Assert.Equal ([ "file2.txt" ], directory.GetFiles())
