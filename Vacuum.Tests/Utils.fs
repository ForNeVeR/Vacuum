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
    interface IDisposable with
        member this.Dispose () =
            Directory.Delete (this.Path, true)

let private setLastChange path date =
    [ File.SetCreationTimeUtc; File.SetLastAccessTimeUtc; File.SetLastWriteTimeUtc ]
    |> Seq.iter (fun f -> f (path, date))

let private createFile location file =
    let path = Path.Combine (location, file.Name)
    do
        use stream = File.Create path
        stream.SetLength file.Size
    setLastChange path file.Date

let prepareEnvironment (infos : FileInfo seq) : DisposableDirectory =
    let infos' = Seq.cache infos
    let minDate =
        infos
        |> Seq.map (fun s -> s.Date)
        |> Seq.min

    let name = string <| Guid.NewGuid ()
    let path = Path.Combine (Path.GetTempPath (), name)

    ignore <| Directory.CreateDirectory path
    setLastChange path minDate

    infos' |> Seq.iter (createFile path)

    { Path = path }

