Vacuum [![Status Aquana][status-aquana]][andivionian-status-classifier] [![Build status][build-status-appveyor]][appveyor]
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

Download
--------

To download Vacuum binary distribution, please visit [GitHub releases
section][releases].

Licensing
---------

Copyright © 2016 Friedrich von Never <friedrich@fornever.me>

This software may be used under the terms of the MIT license, see `License.md`
for details.

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier
[appveyor]: https://ci.appveyor.com/project/ForNeVeR/Vacuum/branch/develop

[build-status-appveyor]: https://ci.appveyor.com/api/projects/status/9mogckfsaxpvmfef/branch/develop?svg=true
[releases]: https://github.com/ForNeVeR/Vacuum/releases
[status-aquana]: https://img.shields.io/badge/status-aquana-yellowgreen.svg
