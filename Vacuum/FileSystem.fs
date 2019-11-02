module Vacuum.FileSystem

open System
open System.IO

open Microsoft.VisualBasic.FileIO

[<Struct>]
type Path = private Path of string with
    /// Creates a path to any file system item. For directories whose names ends with spaces, additional escaping will
    /// be applied.
    static member create(path: string, isDirectory: bool) =
        // When passing a directory path to external APIs, we need to be extra careful about directories whose names are
        // consisting only of spaces. To work with these directories, we'll need to add a terminating path separator,
        // otherwise most APIs will just trim the trailing spaces and work with the parent directories.
        let escaped =
            match path.EndsWith " ", isDirectory with
            | true, false -> raise <| Exception(sprintf "Unsupported file with trailing spaces: \"%s\"" path)
            | true, true -> sprintf "%s%c" path Path.DirectorySeparatorChar
            | false, _ -> path
        Path escaped

    /// Creates a path to an existing file system item. It will check if the item is file or directory. For directories
    /// whose names ends with spaces, additional escaping will be applied.
    static member existing(path: string): Path =
        let isDirectory = Directory.Exists(path + Path.DirectorySeparatorChar.ToString())
        Path.create(path, isDirectory)

    static member (/) (p1: Path, p2: string): Path =
        let (Path p1) = p1
        let resultPath = Path.Combine(p1, p2)
        Path resultPath

    static member (/) (p1: Path, p2: Path): Path =
        let (Path p2) = p2
        p1 / p2

    override this.ToString(): string =
        let (Path p) = this
        p

    static member private removeTrailingSlashes(p: string) =
        p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)

    member this.FileName: string =
        let (Path p) = this
        // Path.GetFileName doesn't work with the trailing slashes, but, on the other hand, it works with the trailing
        // spaces. Thus, remove the trailing slash from the path:
        Path.GetFileName(Path.removeTrailingSlashes p)

    member this.GetParent(): Path =
        let (Path p) = this
        // Directory.GetParent doesn't work with the trailing slashes, but, on the other hand, it works with the
        // trailing spaces. Thus, remove the trailing slash from the path:
        let currentPath = Path.removeTrailingSlashes p
        let parentPath = Directory.GetParent(currentPath).ToString() // called instead of FullPath, because FullPath
                                                                     // eats trailing spaces from the names
        Path.create(parentPath, true)

    member this.GetFullPath(): Path =
        let (Path p) = this
        Path(System.IO.Path.GetFullPath p)

/// Allows to extract the string path value from the path object.
let (|Path|) (p: Path) =
    let (Path x) = p
    x

module File =
    let exists(Path p): bool =
        File.Exists p

    let getCreationTimeUtc(Path p): DateTime =
        File.GetCreationTimeUtc p

    let getLastAccessTimeUtc(Path p): DateTime =
        File.GetLastAccessTimeUtc p

    let getLastWriteTimeUtc(Path p): DateTime =
        File.GetLastWriteTimeUtc p

    let setCreationTimeUtc (Path p) (d: DateTime): unit =
        File.SetCreationTimeUtc(p, d)

    let setLastAccessTimeUtc(Path p) (d: DateTime): unit =
        File.SetLastAccessTimeUtc(p, d)

    let setLastWriteTimeUtc(Path p) (d: DateTime): unit =
        File.SetLastWriteTimeUtc(p, d)

    let create(Path p): FileStream =
        File.Create p

    let recycle(Path p) =
        FileSystem.DeleteFile(p, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

module Directory =
    let enumerateFileSystemEntries(Path p): Path seq =
        Directory.EnumerateFileSystemEntries p
        |> Seq.map Path.existing

    let enumerateFileSystemEntriesRecursively(Path p): Path seq =
        Directory.EnumerateFileSystemEntries(p, "*", SearchOption.AllDirectories)
        |> Seq.map Path.existing

    let setCreationTimeUtc (Path p) (d: DateTime): unit =
        Directory.SetCreationTimeUtc(p, d)

    let setLastAccessTimeUtc(Path p) (d: DateTime): unit =
        Directory.SetLastAccessTimeUtc(p, d)

    let setLastWriteTimeUtc(Path p) (d: DateTime): unit =
        Directory.SetLastWriteTimeUtc(p, d)

    let getTempPath() =
        Path.GetTempPath()
        |> Path.existing

    let create(Path p): unit =
        Directory.CreateDirectory p |> ignore

    let recycle(Path p) =
        FileSystem.DeleteDirectory(p, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

    let deleteRecursive(Path p) =
        Directory.Delete(p, true)
