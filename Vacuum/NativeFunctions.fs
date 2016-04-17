module Vacuum.NativeFunctions

open System.Runtime.InteropServices

[<DllImport("Kernel32")>]
extern int GetCompressedFileSize(string lpFileName, int& lpFileSizeHigh)

let getCompressedFileSize (path : string) : int64 =
    let mutable high = 0
    let low = GetCompressedFileSize(path, &high)
    int64 low + ((int64 high) <<< 32)
