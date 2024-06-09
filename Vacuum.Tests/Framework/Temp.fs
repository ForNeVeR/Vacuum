// SPDX-FileCopyrightText: 2024 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>
//
// SPDX-License-Identifier: MIT

namespace Vacuum.Tests.Framework

open System
open Vacuum.Tests.Framework.FileSystemUtils

type Temp =
    static member DefaultDateTime = DateTime(2010, 1, 1)

    static member CreateFile(path: string, ?date: DateTime, ?size: int64): FileSystemEntryInfo = {
        Entry = File {| Path = path; Size = defaultArg size 0L |}
        Date = defaultArg date Temp.DefaultDateTime
    }

    static member CreateDirectory(path: string, ?date: DateTime): FileSystemEntryInfo = {
        Entry = Directory {| Path = path |}
        Date = defaultArg date Temp.DefaultDateTime
    }

    static member CreateJunction(path: string, target: string, ?date: DateTime): FileSystemEntryInfo = {
        Entry = Junction {| Path = path; TargetPath = target |}
        Date = defaultArg date Temp.DefaultDateTime
    }
