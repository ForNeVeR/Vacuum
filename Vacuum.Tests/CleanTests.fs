module Vacuum.Tests.CleanTests

open System

open Xunit

open Vacuum
open Vacuum.FileSystem
open Vacuum.Tests.Framework
open Vacuum.Tests.Framework.FileSystemUtils

[<Fact>]
let ``Cleaner should always clean the bytes it told to`` () =
    let M = 1024L * 1024L
    use directory = prepareEnvironment [|
        Temp.CreateFile("file0.txt", DateTime(2010, 1, 1), 10L * M)
        Temp.CreateFile("file1.txt", DateTime(2011, 1, 1), 10L * M)
        Temp.CreateFile("file2.txt", DateTime(2011, 1, 1), 10L * M)
    |]
    ignore <| Program.clean directory.Path (DateTime (2012, 1, 1)) (Some <| 15L * M)
    Assert.Equal<string> ([| "file2.txt" |], directory.GetFiles())

[<Fact>]
let ``Cleaner should not fail on long paths`` () =
    let fileName = String ('x', 251) + ".txt"
    use directory = prepareEnvironment [| Temp.CreateFile(fileName, DateTime(2010, 1, 1)) |]
    ignore <| Program.clean directory.Path (DateTime (2011, 1, 1)) None
    Assert.Equal<string> ([| fileName |], directory.GetFiles()) // TODO: It should delete the file actually

[<Fact(Skip = "System call level is not correct")>]
let ``Cleaner should properly clean the directory trees with paths consisting of spaces``(): unit =
    use directory = prepareEnvironment [
        Temp.CreateFile(" / / /name.txt", DateTime(2010, 1, 1))
    ]
    ignore <| Program.clean directory.Path (DateTime(2011, 1, 1)) None
    Assert.Empty(Array.ofSeq <| directory.GetFiles())

[<Fact>]
let ``Cleaner should not fail on invalid junctions``(): unit =
    use testDataRoot = prepareEnvironment [|
        Temp.CreateDirectory("directory")
        Temp.CreateJunction("junction", "directory")
    |]

    let directory = testDataRoot.Path / "directory"
    FileSystem.Directory.deleteRecursive directory

    let result = Program.clean testDataRoot.Path (DateTime(2011, 1, 1)) None
    Assert.Equal(0, result.States.[ScanError])

[<Fact>]
let ``Cleaner should not visit junctions' internals``(): unit =
    use testDataRoot = prepareEnvironment [|
        Temp.CreateDirectory("directory", DateTime(2011, 1, 1))
        Temp.CreateFile("directory/file.txt", DateTime(2010, 1, 1))
        Temp.CreateJunction("junction", "directory")
    |]

    ignore <| Program.clean testDataRoot.Path (DateTime(2011, 1, 1)) None

    Assert.NotEmpty(Directory.enumerateFileSystemEntries(testDataRoot.Path / "directory"))

[<Fact>]
let ``Cleaner should recycle junctions``(): unit =
    use testDataRoot = prepareEnvironment [|
        Temp.CreateFile("directory/file.txt", DateTime(2011, 1, 1))
        Temp.CreateJunction("junction", "directory", DateTime(2010, 1, 1))
    |]

    ignore <| Program.clean testDataRoot.Path (DateTime(2011, 1, 1)) None

    Assert.NotEmpty(Directory.enumerateFileSystemEntries(testDataRoot.Path / "directory"))
    Assert.False(File.exists(testDataRoot.Path / "junction"))

[<Fact>]
let ``Cleaner should clean empty directories``(): unit =
    use testDataRoot = prepareEnvironment [|
        Temp.CreateDirectory("mydir", DateTime(2010, 1, 1))
    |]
    let stats = Program.clean testDataRoot.Path (DateTime(2011, 1, 1)) None
    Assert.Equal(1, stats.States.[Ok])
