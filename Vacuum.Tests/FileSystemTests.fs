module Vacuum.Tests.FileSystemTests

open Xunit

open Vacuum.FileSystem

[<Fact>]
let ``Path.GetParent() should return a proper parent for a file in a space-ending directory``() =
    let path = Path.create(@"C:\Temp\ \name.txt", false)
    let parent = path.GetParent()
    Assert.Equal(Path.create(@"C:\Temp\ \", true), parent)

[<Fact>]
let ``Path.GetParent() should return a proper parent for multiple space-containing directories``() =
    let path = Path.create(@"C:\Temp\ \ \", true)
    let parent = path.GetParent()
    Assert.Equal(Path.create(@"C:\Temp\ \", true), parent)

[<Fact>]
let ``Path.FileName should return a space for a directory consisting of space``() =
    let path = Path.create(@"C:\Temp\ \", true)
    Assert.Equal(" ", path.FileName)
