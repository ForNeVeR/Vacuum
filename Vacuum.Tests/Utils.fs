module Vacuum.Tests.Utils

open System
open System.IO

type FileInfo =
    { Name : string
      Date : DateTime
      Size : int64 }

type DisposableDirectory =
    { Path : string }
    member this.GetFiles () : string seq =
        Directory.EnumerateFileSystemEntries this.Path
        |> Seq.map Path.GetFileName
    interface IDisposable with
        member this.Dispose () =
            Directory.Delete (this.Path, true)

let private setDate setters args = setters |> Seq.iter ((|>) args)

let private setFileDate =
    setDate [ File.SetCreationTimeUtc; File.SetLastAccessTimeUtc; File.SetLastWriteTimeUtc ]

let private setDirectoryDate =
    setDate [ Directory.SetCreationTimeUtc; Directory.SetLastAccessTimeUtc; Directory.SetLastWriteTimeUtc ]

let private createFile location file =
    let path = Path.Combine (location, file.Name)
    do
        use stream = File.Create path
        stream.SetLength file.Size
    setFileDate (path, file.Date)

let prepareEnvironment (infos : FileInfo seq) : DisposableDirectory =
    let infos' = Seq.cache infos
    let minDate =
        infos
        |> Seq.map (fun s -> s.Date)
        |> Seq.min

    let name = string <| Guid.NewGuid ()
    let path = Path.Combine (Path.GetTempPath (), name)

    ignore <| Directory.CreateDirectory path
    setDirectoryDate (path, minDate)

    infos' |> Seq.iter (createFile path)

    { Path = path }
