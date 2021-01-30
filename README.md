Vacuum [![Status Aquana][status-aquana]][andivionian-status-classifier]
======

Windows temporary directory cleanup tool.

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

See [the changelog][changelog].

Download
--------

To download Vacuum binary distribution, please visit [GitHub releases
section][releases].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier
[changelog]: ./CHANGELOG.md
[releases]: https://github.com/ForNeVeR/Vacuum/releases

[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
