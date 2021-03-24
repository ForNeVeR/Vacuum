Vacuum [![Status Aquana][status-aquana]][andivionian-status-classifier]
======

Windows temporary directory cleanup tool.

Requirements
------------

To run the program, [.NET][dotnet] Runtime 5.0 or later should be available in
the system.

To develop the program, [.NET][dotnet] SDK 5.0 or later is required.

Usage
-----

Simply execute `Vacuum.exe`. It will remove any entries in your temp directory
that weren't touched in the last month.

"Entry" is either a file or a directory. Directory counts as "touched" if any of
its children was touched in the month period. Vacuum considers the following
dates when examining the files (as they're reported by the filesystem):

- creation date
- last write date
- last access date

Consult the embedded help system for the detailed parameter manual:

    Vacuum.exe --help

Documentation
-------------

- [Changelog][changelog]
- [Maintainership][maintainership]
- [License][license]

Download
--------

To download Vacuum binary distribution, please visit [GitHub releases
section][releases].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier
[changelog]: ./CHANGELOG.md
[dotnet]: https://dot.net/
[license]: ./LICENSE.md
[maintainership]: ./MAINTAINERSHIP.md
[releases]: https://github.com/ForNeVeR/Vacuum/releases

[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
