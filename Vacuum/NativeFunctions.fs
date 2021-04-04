module Vacuum.NativeFunctions

open System
open System.ComponentModel
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

module private Native =
    type DWORD = uint32
    type WORD = uint16

let private throwLastWin32Error(): unit =
    raise <| Win32Exception()

module private Kernel32 =
    [<Literal>]
    let MAX_PATH = 260

    [<StructLayout(LayoutKind.Sequential)>]
    [<Struct>]
    type FILETIME =
        val mutable dwLowDateTime: Native.DWORD
        val mutable dwHighDateTime: Native.DWORD

    [<StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)>]
    [<Struct>]
    type WIN32_FIND_DATAW =
        val mutable dwFileAttributes: Native.DWORD
        val mutable ftCreationTime: FILETIME
        val mutable ftLastAccessTime: FILETIME
        val mutable ftLastWriteTime: FILETIME
        val mutable nFileSizeHigh: Native.DWORD
        val mutable nFileSizeLow: Native.DWORD
        val mutable dwReserved0: Native.DWORD
        val mutable dwReserved1: Native.DWORD
        [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)>]
        val mutable cFileName: string
        [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)>]
        val mutable cAlternateFileName: string
        val mutable dwFileType: Native.DWORD
        val mutable dwCreatorType: Native.DWORD
        val mutable wFinderFlags: Native.WORD

    [<DllImport("Kernel32", CharSet = CharSet.Unicode)>]
    extern int GetCompressedFileSize(string lpFileName, int& lpFileSizeHigh)

    [<DllImport("Kernel32", SetLastError = true)>]
    extern bool FindClose(nativeint hFindFile)

    type SafeFindCloseHandle() =
        inherit SafeHandleZeroOrMinusOneIsInvalid(true)

        override this.ReleaseHandle() =
            FindClose this.handle

    [<DllImport("Kernel32", CharSet = CharSet.Unicode, SetLastError = true)>]
    extern SafeFindCloseHandle FindFirstFileW(string lpFileName, WIN32_FIND_DATAW& lpFindFileData)

    [<DllImport("Kernel32", SetLastError = true)>]
    extern bool SetFileTime(
        SafeFindCloseHandle hFile,
        FILETIME& lpCreationTime,
        FILETIME& lpLastAccessTime,
        FILETIME& lpLastWriteTime
    )

let getCompressedFileSize(path: AbsolutePath): int64 =
    let mutable high = 0
    let low = Kernel32.GetCompressedFileSize(path.EscapedPathString, &high)
    int64 low + ((int64 high) <<< 32)

let private setFileTime(path: AbsolutePath) setter: unit =
    let mutable data = Kernel32.WIN32_FIND_DATAW()
    use handle = Kernel32.FindFirstFileW(path.EscapedPathString, &data)
    if handle.IsInvalid then throwLastWin32Error()

    setter data handle

let setFileCreationTimeUtc(path: AbsolutePath) (time: DateTime): unit =
    setFileTime path (fun data handle ->
        let mutable data = data
        let mutable creationTimeLong = time.ToFileTimeUtc()
        let creationTime = &Unsafe.As<int64, Kernel32.FILETIME>(&creationTimeLong)
        if Kernel32.SetFileTime(handle, &creationTime, &data.ftLastAccessTime, &data.ftLastWriteTime)
        then throwLastWin32Error()
    )

let setLastWriteTimeUtc(path: AbsolutePath) (time: DateTime): unit =
    setFileTime path (fun data handle ->
        let mutable data = data
        let mutable lastWriteTimeLong = time.ToFileTimeUtc()
        let lastWriteTime = &Unsafe.As<int64, Kernel32.FILETIME>(&lastWriteTimeLong)
        if Kernel32.SetFileTime(handle, &data.ftCreationTime, &data.ftLastAccessTime, &lastWriteTime)
        then throwLastWin32Error()
    )
