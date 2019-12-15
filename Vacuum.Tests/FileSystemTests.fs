module Vacuum.Tests.FileSystemTests

open System

open Xunit

open Vacuum
open Vacuum.FileSystem
open Vacuum.Tests.Utils

[<Fact>]
let ``AbsolutePath.create removes UNC prefix``(): unit =
    let path = AbsolutePath.create @"\\?\C:\Temp\path \file."
    Assert.Equal(@"C:\Temp\path \file.", path.RawPathString)

[<Fact>]
let ``AbsolutePath.create normalizes the path separators``(): unit =
    let path = AbsolutePath.create @"C:\Temp/path/to\file.txt"
    Assert.Equal(@"C:\Temp\path\to\file.txt", path.RawPathString)

[<Fact>]
let ``AbsolutePath.create removes duplicated separators``(): unit =
    let path = AbsolutePath.create @"C:\Temp//path\//to\\\file.txt"
    Assert.Equal(@"C:\Temp\path\to\file.txt", path.RawPathString)

[<Fact>]
let ``AbsolutePath.create removes trailing separators``(): unit =
    let path = AbsolutePath.create @"C:\Temp//path\//"
    Assert.Equal(@"C:\Temp\path", path.RawPathString)

[<Fact>]
let ``AbsolutePath.create throws for relative path``(): unit =
    ignore <| Assert.ThrowsAny<Exception>(fun () -> ignore <| AbsolutePath.create @"relative path")
    ignore <| Assert.ThrowsAny<Exception>(fun () -> ignore <| AbsolutePath.create @"./relative path")
    ignore <| Assert.ThrowsAny<Exception>(fun () -> ignore <| AbsolutePath.create @"C:relative path")
    ignore <| Assert.ThrowsAny<Exception>(fun () -> ignore <| AbsolutePath.create @"/disk relative path")
    ignore <| Assert.ThrowsAny<Exception>(fun () -> ignore <| AbsolutePath.create @"\disk relative path")

[<Fact>]
let ``AbsolutePath.EscapedPathString uses UNC for space-ending path``(): unit =
    let path = AbsolutePath.create @"C:\Temp\path "
    Assert.Equal(@"\\?\C:\Temp\path ", path.EscapedPathString)

[<Fact>]
let ``AbsolutePath.EscapedPathString uses UNC for dot-ending path``(): unit =
    let path = AbsolutePath.create @"C:\Temp\path."
    Assert.Equal(@"\\?\C:\Temp\path.", path.EscapedPathString)

[<Fact>]
let ``AbsolutePath.GetParent() should return a proper parent for a file in a space-ending directory``() =
    let path = AbsolutePath.create @"C:\Temp\ \name.txt"
    let parent = path.GetParent()
    Assert.Equal(AbsolutePath.create @"C:\Temp\ \", parent)

[<Fact>]
let ``AbsolutePath.GetParent() should return a proper parent for multiple space-containing directories``() =
    let path = AbsolutePath.create @"C:\Temp\ \ \"
    let parent = path.GetParent()
    Assert.Equal(AbsolutePath.create @"C:\Temp\ \", parent)

[<Fact>]
let ``AbsolutePath.FileName should return a space for a directory consisting of space``() =
    let path = AbsolutePath.create @"C:\Temp\ \"
    Assert.Equal(" ", path.FileName)

[<Fact>]
let ``AbsolutePath resolve should preserve the trailing spaces``(): unit =
    let temp = AbsolutePath.create @"C:\Temp"
    let derivedPath = temp / "spaces "
    Assert.Equal(@"C:\Temp\spaces ", derivedPath.RawPathString)

[<Fact>]
let ``enumerateFileSystemEntriesRecursively deals with spaces``(): unit =
    use dir = prepareEnvironment [|
        { Path = "path with space /file.txt"; Date = DateTime(2010, 1, 1); Size = 1024L }
    |]
    let directory = dir.Path / "path with space "
    let items = FileSystem.Directory.enumerateFileSystemEntriesRecursively directory
    Assert.Equal<AbsolutePath>(Seq.singleton(directory / "file.txt"), items)
