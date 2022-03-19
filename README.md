Vacuum [![Status Aquana][status-aquana]][andivionian-status-classifier]
======

Windows temporary directory cleanup tool.

Requirements
------------

To develop the program, [.NET][dotnet] SDK 6.0 or later is required.

Usage
-----

Simply execute `Vacuum.exe`. It will remove any entries in your temp directory
that weren't touched in the last month.

"Entry" is either a file or a directory. Directory counts as "touched" if any of
its children was touched in the month period. Vacuum considers the following
dates when examining the files (as they're reported by the filesystem):

- creation date
- last write date

Main command-line arguments:

- `(-d|--directory) <path>`: path to the temporary directory which should be
  cleaned up. Falls back to [`Path.GetTempPath`][path.get-temp-path] by default
  (which uses certain environment variables to determine the path).
- `(-p|--period) <number>`: number of days for entry to be untouched before
  being deleted by Vacuum. 30 by default.
- `(-s|--space) (<number>|<number>k|<number>m)` amount of space to clean up (`k`
  = kibibytes, m = mebibytes). In space-cleaning mode, Vacuum will still clean
  up the oldest items first.
- `(-f|--force)`: forces Vacuum to delete the entries it wasn't able to recycle.
- `(-w|--what-if)`: only prints the files that will be deleted instead of actually deleting them.

Consult the embedded help system for the detailed parameter manual:

```console
$ Vacuum.exe --help
```

Documentation
-------------

- [Changelog][changelog]
- [Maintainership][maintainership]
- [License (MIT)][license]
- [Third-party software][third-party]

Download
--------

To download Vacuum binary distribution, please visit [GitHub releases
section][releases].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier
[changelog]: ./CHANGELOG.md
[dotnet]: https://dot.net/
[license]: ./LICENSE.md
[maintainership]: ./MAINTAINERSHIP.md
[path.get-temp-path]: https://docs.microsoft.com/en-us/dotnet/api/system.io.path.gettemppath?view=net-5.0&tabs=windows
[releases]: https://github.com/ForNeVeR/Vacuum/releases
[third-party]: THIRD-PARTY-NOTICES.md

[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
