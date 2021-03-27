module Vacuum.Tests.NativeFunctionsTests

open System

open Xunit

open Vacuum
open Vacuum.Tests.Framework
open Vacuum.Tests.Framework.FileSystemUtils

[<Fact>]
let ``GetCompressedFileSize should return meaningful value for small file`` () =
    let fileName = "file.dat"
    use directory = prepareEnvironment [|
        Temp.CreateFile(fileName, DateTime.UtcNow, 10240L)
    |]
    let size = NativeFunctions.getCompressedFileSize(directory.Path / fileName)
    Assert.True (size > 0L && size < int64 Int32.MaxValue)
