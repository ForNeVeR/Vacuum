module Vacuum.NativeFunctions

open System.Runtime.InteropServices

open Vacuum.FileSystem

[<DllImport("Kernel32")>]
extern int GetCompressedFileSize(string lpFileName, int& lpFileSizeHigh)

let getCompressedFileSize(path: AbsolutePath): int64 =
    let mutable high = 0
    let low = GetCompressedFileSize(path.EscapedPathString, &high)
    int64 low + ((int64 high) <<< 32)
