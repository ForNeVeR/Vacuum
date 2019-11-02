namespace Vacuum

open System

open Vacuum.FileSystem

type RemoveStatus = Ok | Error

type CleanResult = {
    Directory: Path
    CleanedDate: DateTime
    ItemsBefore: int
    ItemsAfter: int
    States: Map<RemoveStatus, int>
    TimeTaken: TimeSpan
}
