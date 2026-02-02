<!--
SPDX-FileCopyrightText: 2024-2026 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>

SPDX-License-Identifier: MIT
-->

# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic
Versioning v2.0.0](https://semver.org/spec/v2.0.0.html).

## [Unreleased] (1.8.0)
### Changed
- Update to .NET 10.

### Added
- Free disk space report is printed after the cleanup.

### Fixed
- Decimal point is no longer printed for file size in bytes (e.g. `0.00 B` is now just `0 B`).

## [1.7.0] - 2022-10-01
### Changed
- Exception call stacks are hidden by default.

### Added
- `(-v|--verbose)` parameter to show the call stacks.

## [1.6.0] - 2022-03-20
### Added
- [#5: `--what-if` option to preview the deleted files instead of recycling them](https://github.com/ForNeVeR/Vacuum/issues/5).
- Show space occupied by removed entries in a mode when this space is calculated.

### Fixed
- [#4: `--space` parameter wasn't properly calculating amount of space occupied by files inside of nested directories](https://github.com/ForNeVeR/Vacuum/issues/4).

## [1.5.1] - 2021-12-25
### Fixed
- [#67: Unable to delete a file from a directory ending with dot](https://github.com/ForNeVeR/Vacuum/issues/67).

## [1.5.0] - 2021-12-12
### Changed
- Slightly updated statistics output (less redundant information, less empty lines).
- Project is now published as a self-contained single-file binary (no .NET runtime dependency).
- Project has migrated to .NET 6.0.

## [1.4.0] - 2021-04-08
### Added
- `--force` option to delete the entries that we weren't able to recycle.

## [1.3.0] - 2021-04-04
### Changed
- Ignore last access dates when determining whether to delete an entry.

### Added
- Usage documentation.

## [1.2.0] - 2021-03-27
### Added
- Scan error count in the output.
- Reparse point support.

## [1.1.1] - 2021-03-24
### Changed
- Project migrated to .NET 5.0.

### Fixed
- [#44: Vacuum fails completely after failing to enumerate certain path](https://github.com/ForNeVeR/Vacuum/issues/44).

## [1.1.0] - 2021-01-30
### Added
- Documentation files are now included into the program distribution.

### Fixed
- Format for path logging.
- Broken Unicode characters in the output.

## [1.0.1] - 2019-12-15
### Fixed
- [#26: Infinite looping](https://github.com/ForNeVeR/Vacuum/issues/26) when encountered a path with spaces.
- [#28: Paths with dot aren't scanned properly](https://github.com/ForNeVeR/Vacuum/issues/28).

## [1.0.0] - 2019-10-17
### Changed
- [#24: Port to .NET Core](https://github.com/ForNeVeR/Vacuum/issues/24).
- Improved logging for file system errors.

## [0.5.1] - 2016-05-27
### Fixed
- [#17: `PathTooLongException`](https://github.com/ForNeVeR/Vacuum/issues/17).

## [0.5.0] - 2016-04-27
### Added
- Flag to free a specified amount of space.

## [0.4.0] - 2016-04-03
### Removed
- Possibility to set clean period in months.

## [0.3.0] - 2016-02-09
### Added
- `clean` command (not mandatory to pass, is deduced by default).
- `--help`: embedded help system.
- `--version` option to print the program version.
- `--directory` flag to override cleaned directory.
- `--period` flag to override period after which items are considered for cleanup.

## [0.2.0] - 2016-01-29
### Changed
- Consider date of a directory itself, in addition to the contained files' dates.

## [0.1.0] - 2016-01-24
### Added
- Log additional information (cleanup date, items before and after the cleanup, total time taken).

## [0.0.1] - 2016-01-23
This is the initial version of the program to delete old entries from the system temporary directory.

[0.0.1]: https://github.com/ForNeVeR/Vacuum/releases/tag/0.0.1
[0.1.0]: https://github.com/ForNeVeR/Vacuum/compare/0.0.1...0.1
[0.2.0]: https://github.com/ForNeVeR/Vacuum/compare/0.1...0.2
[0.3.0]: https://github.com/ForNeVeR/Vacuum/compare/0.2...0.3
[0.4.0]: https://github.com/ForNeVeR/Vacuum/compare/0.3...0.4
[0.5.0]: https://github.com/ForNeVeR/Vacuum/compare/0.4...0.5
[0.5.1]: https://github.com/ForNeVeR/Vacuum/compare/0.5...0.5.1
[1.0.0]: https://github.com/ForNeVeR/Vacuum/compare/0.5.1...1.0.0
[1.0.1]: https://github.com/ForNeVeR/Vacuum/compare/1.0.0...1.0.1
[1.1.0]: https://github.com/ForNeVeR/Vacuum/compare/1.0.1...v1.1.0
[1.1.1]: https://github.com/ForNeVeR/Vacuum/compare/v1.1.0...v1.1.1
[1.2.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.1.1...v1.2.0
[1.3.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.2.0...v1.3.0
[1.4.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.3.0...v1.4.0
[1.5.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.4.0...v1.5.0
[1.5.1]: https://github.com/ForNeVeR/Vacuum/compare/v1.5.0...v1.5.1
[1.6.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.5.1...v1.6.0
[1.7.0]: https://github.com/ForNeVeR/Vacuum/compare/v1.6.0...v1.7.0
[Unreleased]: https://github.com/ForNeVeR/Vacuum/compare/v1.7.0...HEAD
