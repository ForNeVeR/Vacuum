module Vacuum.Tests.Framework.FileSystemUtils

open System

open NCode.ReparsePoints

open Vacuum
open Vacuum.FileSystem

type FileSystemEntry =
    | File of {| Path: string; Size: int64 |}
    | Directory of {| Path: string |}
    | Junction of {| Path: string; TargetPath: string |}

type FileSystemEntryInfo = {
    Entry: FileSystemEntry
    Date: DateTime
}

type DisposableDirectory = {
    Path: AbsolutePath
}
    with
    member this.GetFiles () : string seq =
        Directory.enumerateFileSystemEntries this.Path
        |> Seq.map (fun p -> p.FileName)
    interface IDisposable with
        member this.Dispose () =
            Directory.deleteRecursive this.Path

let private setDate setters path date =
    setters
    |> Seq.iter(fun setter -> setter path date)

let private setFileDate =
    setDate [| File.setCreationTimeUtc; File.setLastWriteTimeUtc |]

let private setDirectoryDate =
    setDate [| Directory.setCreationTimeUtc; Directory.setLastWriteTimeUtc |]

let private setJunctionDate =
    setDate [| ReparsePoint.setCreationTimeUtc; ReparsePoint.setLastWriteTimeUtc |]

let private createEntry (rootLocation: AbsolutePath) (file: FileSystemEntryInfo) =
    match file.Entry with
    | File entry ->
        let path = rootLocation / entry.Path

        let location = path.GetParent()
        Directory.create location

        do
            use stream = File.create path
            stream.SetLength entry.Size
        setFileDate path file.Date
    | Directory entry ->
        let path = rootLocation / entry.Path
        Directory.create path
        setDirectoryDate path file.Date
    | Junction entry ->
        let junctionPath = rootLocation / entry.Path
        let targetPath = rootLocation / entry.TargetPath

        ReparsePointFactory.Provider.CreateLink(
            junctionPath.EscapedPathString,
            targetPath.EscapedPathString,
            LinkType.Junction
        )

        setJunctionDate junctionPath file.Date

let prepareEnvironment (infos : FileSystemEntryInfo seq) : DisposableDirectory =
    let infos' = Seq.cache infos

    let name = string <| Guid.NewGuid ()
    let path = Directory.getTempPath() / name

    Directory.create path
    if (not (Seq.isEmpty infos')) then
        let minDate =
            infos'
            |> Seq.map (fun s -> s.Date)
            |> Seq.min
        setDirectoryDate path minDate
        infos' |> Seq.iter(createEntry path)

    { Path = path }
