// SPDX-FileCopyrightText: 2024 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>
//
// SPDX-License-Identifier: MIT

module Vacuum.CommandLineParser

open CommandLine

open Vacuum.Commands

let parse (args : string[]) : Clean option =
    match Parser.Default.ParseArguments<Clean> args with
    | :? Parsed<Clean> as command -> Some command.Value
    | _ -> None

