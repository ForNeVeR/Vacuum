namespace Vacuum

open System

type RemoveStatus = Ok | Error

type CleanResult =
    { Directory : string
      CleanedDate : DateTime
      ItemsBefore : int
      ItemsAfter : int
      States : Map<RemoveStatus, int>
      TimeTaken : TimeSpan }

