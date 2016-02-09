module Vacuum.Commands

open CommandLine

[<Verb("clean", HelpText = "Clean the temporary directory.")>]
type Clean =
    { [<Option('d', "directory", HelpText = "Temporary directory path. Will be calculated if not defined.")>]
      Directory : string option

      [<Option('p', "period", HelpText = "Time period for file to stay untouched before being cleaned up. In days by default; add M postfix to define period in months. For example, --period 2M will clean up the files that weren't touched in last 2 months. Default period is 1 month.")>]
      Period : string option }
