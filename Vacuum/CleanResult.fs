namespace Vacuum

open System

open Vacuum.FileSystem

type RemoveStatus = Ok | Error

type CleanResult = {
    Directory: AbsolutePath
    CleanedDate: DateTime
    ItemsBefore: int
    ItemsAfter: int
    States: Map<RemoveStatus, int>
    TimeTaken: TimeSpan
}
