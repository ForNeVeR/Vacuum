namespace Vacuum

open Vacuum.Clean

type ICleaner =
    abstract member Clean: Commands.Clean -> CleanResult

type DefaultCleaner =
    interface ICleaner with
        member this.Clean (cmd: Commands.Clean) =
