<!--
SPDX-FileCopyrightText: 2024 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>

SPDX-License-Identifier: MIT
-->

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

"Entry" is either a file or a directory. A directory counts as "touched" if any of
its children was touched in the last month. Vacuum considers the following
dates when examining the files (as they're reported by the filesystem):

- creation date
- last write date

Main command-line arguments:

- `(-d|--directory) <path>`: path to the temporary directory which should be
  cleaned up. Falls back to [`Path.GetTempPath`][path.get-temp-path] by default
  (which uses certain environment variables to determine the path).
- `(-p|--period) <number>`: number of days for an entry to be untouched before
  being deleted by Vacuum. 30 by default.
- `(-s|--space) (<number>|<number>k|<number>m|<number>g)`: amount of space to clean up (`k`
  = kibibytes, `m` = mebibytes, `g` = gibibytes). In space-cleaning mode, Vacuum will still clean
  up the oldest items first.
- `(-F|--free) (<number>|<number>k|<number>m|<number>g)`: amount of space to be free after the
  clean (`k` = kibibytes, `m` = mebibytes, `g` = gibibytes). The oldest items will still be cleaned up first.

  > **Pro Tip:**
  >
  > If you use PowerShell, then it's possible to [easily pass arbitrary sizes in bytes][docs.pwsh-numeric-literals] without any need for calculation. Try the following in shell:
  >
  > ```console
  > $ Vacuum --space $(10gb)
  > ```
  >
  > This will call `Vacuum --space 10737418240` (i.e. 10 gibibytes).

- `(-f|--force)`: forces Vacuum to delete the entries it wasn't able to recycle.
- `(-w|--what-if)`: only prints the files that would be deleted instead of actually deleting them.
- `(-v|--verbose)`: show exception call stacks.

Consult the embedded help system for the detailed parameter manual:

```console
$ Vacuum.exe --help
```

Documentation
-------------

- [Changelog][changelog]
- [Contributor Guide][docs.contributing]
- [Maintainership][maintainership]
- [Third-party software][third-party]

Download
--------
To download Vacuum binary distribution, please visit [GitHub releases
section][releases].

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.0][reuse.spec].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier
[changelog]: ./CHANGELOG.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: ./LICENSE.md
[docs.pwsh-numeric-literals]: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_numeric_literals
[dotnet]: https://dot.net/
[maintainership]: ./MAINTAINERSHIP.md
[path.get-temp-path]: https://docs.microsoft.com/en-us/dotnet/api/system.io.path.gettemppath?view=net-5.0&tabs=windows
[releases]: https://github.com/ForNeVeR/Vacuum/releases
[reuse.spec]: https://reuse.software/spec/
[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
[third-party]: THIRD-PARTY-NOTICES.md
