module Vacuum.Commands

open CommandLine

[<Verb("clean", HelpText = "Clean the temporary directory.")>]
type Clean() = class end
