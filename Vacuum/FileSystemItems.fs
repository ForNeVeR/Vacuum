namespace Vacuum

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
            $"{path.RawPathString} ({Console.presentSpace byteSize})"
