module Vacuum.Commands

open CommandLine

[<Verb("clean", HelpText = "Clean the temporary directory.")>]
type Clean =
    { [<Option('d', "directory", HelpText = "Temporary directory path. Will be calculated if not defined.")>]
      Directory : string option }
