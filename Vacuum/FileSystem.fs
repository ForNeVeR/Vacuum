module Vacuum.FileSystem

open System
open System.IO
open System.Text.RegularExpressions

open Microsoft.VisualBasic.FileIO

[<Struct>]
type AbsolutePath = private AbsolutePath of string with
    static member UncPathPrefix = @"\\?\"

    /// Checks that the path represents an absolute path. For local Windows paths, it checks that the path is a path
    /// with disk letter and that it is rooted on that disk. Path.IsPathRooted won't work because it doesn't work for
    /// disk-relative paths such as "C:Temp".
    static member private assertAbsolute(AbsolutePath path) =
        if not(path.Length > 2 && path.[1] = ':' && path.[2] = Path.DirectorySeparatorChar) then
            failwithf "Path \"%s\" is not absolute" path

    /// Creates a path from a string. All the characters in the string passed will be preserved (and it differs from the
    /// way it works in the standard path-handling functions). Supports alternate path separators and UNC path prefix
    /// (\\?\), but will normalize them (and add a prefix when necessary). Trims any trailing path separators.
    static member create(path: string) =
        let withoutUncPrefix =
            if path.StartsWith(AbsolutePath.UncPathPrefix, StringComparison.Ordinal)
            then path.Substring AbsolutePath.UncPathPrefix.Length
            else path
        let withNormalizedSeparators = withoutUncPrefix.Replace(Path.AltDirectorySeparatorChar,
                                                    Path.DirectorySeparatorChar)
        let withTrimmedSeparators = withNormalizedSeparators.TrimEnd Path.DirectorySeparatorChar
        let withDeduplicatedSeparators = Regex.Replace(withTrimmedSeparators,
                                                       sprintf "%s+" (Regex.Escape (string Path.DirectorySeparatorChar)),
                                                       string Path.DirectorySeparatorChar,
                                                       RegexOptions.CultureInvariant)

        let result = AbsolutePath withDeduplicatedSeparators
        AbsolutePath.assertAbsolute result
        result

    static member (/) (p1: AbsolutePath, p2: string): AbsolutePath =
        let (AbsolutePath p1) = p1
        let resultPath = Path.Combine(p1, p2)
        AbsolutePath resultPath

    /// Returns the path string optionally prefixed by the UNC path prefix (\\?\) if necessary.
    ///
    /// It may be necessary if the path ends with a space or a dot.
    member this.EscapedPathString: string =
        let (AbsolutePath p) = this
        if p.EndsWith ' ' || p.EndsWith '.' then
            sprintf @"\\?\%s" p
        else
            p

    /// Allows to extract the raw string value from the path object. It represents the path with normalized separators
    /// and trimmed UNC path prefix (\\?\).
    member this.RawPathString: string =
        let (AbsolutePath p) = this
        p

    member this.FileName: string =
        let (AbsolutePath p) = this
        Path.GetFileName p

    member this.GetParent(): AbsolutePath =
        let (AbsolutePath p) = this
        let parentPath = Directory.GetParent(p).ToString() // called instead of FullPath, because FullPath eats trailing
                                                           // spaces from the names
        AbsolutePath parentPath

module File =
    let exists(p: AbsolutePath): bool =
        File.Exists p.EscapedPathString

    let getCreationTimeUtc(p: AbsolutePath): DateTime =
        File.GetCreationTimeUtc p.EscapedPathString

    let getLastAccessTimeUtc(p: AbsolutePath): DateTime =
        File.GetLastAccessTimeUtc p.EscapedPathString

    let getLastWriteTimeUtc(p: AbsolutePath): DateTime =
        File.GetLastWriteTimeUtc p.EscapedPathString

    let setCreationTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        File.SetCreationTimeUtc(p.EscapedPathString, d)

    let setLastAccessTimeUtc(p: AbsolutePath) (d: DateTime): unit =
        File.SetLastAccessTimeUtc(p.EscapedPathString, d)

    let setLastWriteTimeUtc(p: AbsolutePath) (d: DateTime): unit =
        File.SetLastWriteTimeUtc(p.EscapedPathString, d)

    let create(p: AbsolutePath): FileStream =
        File.Create p.EscapedPathString

    let recycle(p: AbsolutePath) =
        FileSystem.DeleteFile(p.EscapedPathString, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

module Directory =
    let enumerateFileSystemEntries(p: AbsolutePath): AbsolutePath seq =
        Directory.EnumerateFileSystemEntries p.EscapedPathString
        |> Seq.map AbsolutePath.create

    let enumerateFileSystemEntriesRecursively(p: AbsolutePath): AbsolutePath seq =
        Directory.EnumerateFileSystemEntries(p.EscapedPathString, "*", SearchOption.AllDirectories)
        |> Seq.map AbsolutePath.create

    let setCreationTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        Directory.SetCreationTimeUtc(p.EscapedPathString, d)

    let setLastAccessTimeUtc(p: AbsolutePath) (d: DateTime): unit =
        Directory.SetLastAccessTimeUtc(p.EscapedPathString, d)

    let setLastWriteTimeUtc (p: AbsolutePath) (d: DateTime): unit =
        Directory.SetLastWriteTimeUtc(p.EscapedPathString, d)

    let getTempPath(): AbsolutePath =
        Path.GetTempPath()
        |> AbsolutePath.create

    let create(p: AbsolutePath): unit =
        Directory.CreateDirectory p.EscapedPathString |> ignore

    let recycle(p: AbsolutePath): unit =
        FileSystem.DeleteDirectory(p.EscapedPathString, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin)

    let deleteRecursive(p: AbsolutePath): unit =
        Directory.Delete(p.EscapedPathString, true)
