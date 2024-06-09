// SPDX-FileCopyrightText: 2024 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>
//
// SPDX-License-Identifier: MIT

module Vacuum.FileSystem

open System
open System.IO

open Microsoft.VisualBasic.FileIO

module File =
    let exists(p: AbsolutePath): bool =
        File.Exists p.EscapedPathString

    let getCreationTimeUtc(p: AbsolutePath): DateTime =
        File.GetCreationTimeUtc p.EscapedPathString

    let getLastWriteTimeUtc(p: AbsolutePath): DateTime =
        File.GetLastWriteTimeUtc p.EscapedPathString

    let setCreationTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        File.SetCreationTimeUtc(p.EscapedPathString, d)

    let setLastWriteTimeUtc(p: AbsolutePath) (d: DateTime): unit =
        File.SetLastWriteTimeUtc(p.EscapedPathString, d)

    let create(p: AbsolutePath): FileStream =
        File.Create p.EscapedPathString

    let delete(p: AbsolutePath): unit =
        File.Delete p.EscapedPathString

    let recycle(p: AbsolutePath) =
        FileSystem.DeleteFile(p.EscapedPathString, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

module ReparsePoint =
    let exists(path: AbsolutePath): bool =
        Directory.Exists(path.EscapedPathString)
        && File.GetAttributes(path.EscapedPathString).HasFlag(FileAttributes.ReparsePoint)

    let delete(path: AbsolutePath): unit =
        Directory.Delete(path.EscapedPathString)

    let setCreationTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        NativeFunctions.setFileCreationTimeUtc p d

    let setLastWriteTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        NativeFunctions.setLastWriteTimeUtc p d

    let recycle(p: AbsolutePath): unit =
        FileSystem.DeleteDirectory(p.EscapedPathString, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

module Directory =
    let enumerateFileSystemEntries(p: AbsolutePath): AbsolutePath seq =
        if ReparsePoint.exists p then failwith $"Should not enumerate reparse point contents: \"%s{p.RawPathString}\""
        Directory.EnumerateFileSystemEntries p.EscapedPathString
        |> Seq.map AbsolutePath.create

    let rec enumerateFileSystemEntriesRecursively(rootPath: AbsolutePath): AbsolutePath seq =
        if ReparsePoint.exists rootPath then
            failwith $"Should not enumerate reparse point contents: \"%s{rootPath.RawPathString}\""

        seq {
            let mutable currentPath = rootPath
            for entry in enumerateFileSystemEntries currentPath do
                yield entry
                if not(ReparsePoint.exists entry) && Directory.Exists(entry.EscapedPathString) then
                    yield! enumerateFileSystemEntriesRecursively entry
        }

    let setCreationTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        Directory.SetCreationTimeUtc(p.EscapedPathString, d)

    let setLastWriteTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        Directory.SetLastWriteTimeUtc(p.EscapedPathString, d)

    let getTempPath(): AbsolutePath =
        Path.GetTempPath()
        |> AbsolutePath.create

    let create(p: AbsolutePath): unit =
        Directory.CreateDirectory p.EscapedPathString |> ignore

    let recycle(p: AbsolutePath): unit =
        FileSystem.DeleteDirectory(p.EscapedPathString, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

    let rec deleteRecursive(rootToDelete: AbsolutePath): unit =
        for entry in enumerateFileSystemEntries rootToDelete do
            if ReparsePoint.exists entry then ReparsePoint.delete entry
            else if Directory.Exists(entry.EscapedPathString) then deleteRecursive entry
            else if File.exists entry then File.Delete entry.EscapedPathString
            else failwith $"File not found: \"%s{entry.RawPathString}\""
        Directory.Delete(rootToDelete.EscapedPathString)

