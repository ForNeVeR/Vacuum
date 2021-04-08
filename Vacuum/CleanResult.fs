namespace Vacuum

open System

type ProcessingStatus = Recycled | ForceDeleted | Error | ScanError

type CleanResult = {
    Directory: AbsolutePath
    CleanedDate: DateTime
    ItemsBefore: int
    ItemsAfter: int
    States: Map<ProcessingStatus, int>
    TimeTaken: TimeSpan
}
