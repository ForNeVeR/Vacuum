namespace Vacuum

open System.Globalization
open Vacuum

type IFileSystemItem =
    abstract member Path: AbsolutePath
    abstract member Present: unit -> string

type FileSystemItem(path: AbsolutePath) =
    interface IFileSystemItem with
        member _.Path = path
        member _.Present() = path.RawPathString

type SizedFileSystemItem(path: AbsolutePath, byteSize: int64) =
    interface IFileSystemItem with
        member _.Path = path
        member _.Present() =
            let float, size, measure =
                match double byteSize with
                | x when x >= 2.0 ** 30.0 -> true, x / 2.0 ** 30.0, "GiB"
                | x when x >= 2.0 ** 20.0 -> true, x / 2.0 ** 20.0, "MiB"
                | x when x >= 2.0 ** 10.0 -> true, x / 2.0 ** 10.0, "kiB"
                | x -> false, x, "B"
            let formattedSize =
                if float
                then size.ToString("F2", CultureInfo.InvariantCulture)
                else size.ToString("0", CultureInfo.InvariantCulture)
            $"{path.RawPathString} ({formattedSize} {measure})"
