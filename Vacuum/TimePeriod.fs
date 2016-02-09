module Vacuum.TimePeriod

open System

type Period =
    | Days of int
    | Months of int

let parse (s : string) : Period =
    match s.ToLowerInvariant() with
    | x when x.EndsWith "m" ->
        let count = int (x.Substring (0, x.Length - 1))
        Months count
    | x -> Days (int x)

let subtract (dateTime : DateTime) = function
    | Days d -> dateTime.AddDays (- float d)
    | Months m -> dateTime.AddMonths -m
