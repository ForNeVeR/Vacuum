namespace Vacuum

open System
open System.IO
open System.Text.RegularExpressions

[<Struct>]
type AbsolutePath = private AbsolutePath of string with
    static member UncPathPrefix = @"\\?\"

    static member private normalizePathWithoutUncPrefix(p: string) =
        let withNormalizedSeparators = p.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        let withTrimmedSeparators = withNormalizedSeparators.TrimEnd Path.DirectorySeparatorChar
        let withDeduplicatedSeparators = Regex.Replace(withTrimmedSeparators,
                                                       $"{Regex.Escape(string Path.DirectorySeparatorChar)}+",
                                                       string Path.DirectorySeparatorChar,
                                                       RegexOptions.CultureInvariant)
        withDeduplicatedSeparators

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

        let normalizedPath = AbsolutePath.normalizePathWithoutUncPrefix withoutUncPrefix
        let result = AbsolutePath normalizedPath
        AbsolutePath.assertAbsolute result
        result

    static member (/) (p1: AbsolutePath, p2: string): AbsolutePath =
        let (AbsolutePath p1) = p1
        let resultPath = Path.Combine(p1, AbsolutePath.normalizePathWithoutUncPrefix p2)
        AbsolutePath resultPath

    /// Returns the path string optionally prefixed by the UNC path prefix (\\?\) if necessary.
    ///
    /// It may be necessary if the path ends with a space or a dot.
    member this.EscapedPathString: string =
        let (AbsolutePath p) = this
        let isPrefixRequired =
            p.Split Path.DirectorySeparatorChar
            |> Array.exists (fun p -> p.EndsWith ' ' || p.EndsWith '.')
        if isPrefixRequired then
            AbsolutePath.UncPathPrefix + p
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
        let lastSeparatorIndex = p.LastIndexOf Path.DirectorySeparatorChar
        if lastSeparatorIndex = -1 then
            failwithf $"No parent path could be determined for path \"{this.RawPathString}\"."

        let parentPath = p.Substring(0, lastSeparatorIndex)
        AbsolutePath parentPath
