module Vacuum.Tests.Utils

open System

open Vacuum.FileSystem

type FileInfo = {
    Path: string
    Date: DateTime
    Size: int64
}

type DisposableDirectory = {
    Path: Path
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
    setDate [| File.setCreationTimeUtc; File.setLastAccessTimeUtc; File.setLastWriteTimeUtc |]

let private setDirectoryDate =
    setDate [| Directory.setCreationTimeUtc; Directory.setLastAccessTimeUtc; Directory.setLastWriteTimeUtc |]

let private setTreeDates (root: Path) (child: Path) minDate =
    let mutable current = child.GetFullPath()
    while current <> root.GetFullPath() do
        if File.exists current
        then setFileDate current minDate
        else setDirectoryDate current minDate

        current <- current.GetParent()

let private createFile (rootLocation: Path) minDate (file: FileInfo) =
    let path = rootLocation / file.Path

    let location = path.GetParent()
    Directory.create location

    do
        use stream = File.create path
        stream.SetLength file.Size
    setFileDate path file.Date
    setTreeDates rootLocation location minDate

let prepareEnvironment (infos : FileInfo seq) : DisposableDirectory =
    let infos' = Seq.cache infos
    let minDate =
        infos
        |> Seq.map (fun s -> s.Date)
        |> Seq.min

    let name = string <| Guid.NewGuid ()
    let path = Directory.getTempPath() / name

    Directory.create path
    setDirectoryDate path minDate

    infos' |> Seq.iter (createFile path minDate)

    { Path = path }
