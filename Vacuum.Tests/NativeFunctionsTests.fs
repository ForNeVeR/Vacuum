module Vacuum.Tests.NativeFunctionsTests

open System

open Xunit

open Vacuum
open Vacuum.Tests.Utils

[<Fact>]
let ``GetCompressedFileSize should return meaningful value for small file`` () =
    let fileName = "file.dat"
    use directory = prepareEnvironment [ { Path = fileName; Date = DateTime.UtcNow; Size = 10240L } ]
    let size = NativeFunctions.getCompressedFileSize(directory.Path / fileName)
    Assert.True (size > 0L && size < int64 Int32.MaxValue)
