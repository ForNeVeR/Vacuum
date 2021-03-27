# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic
Versioning v2.0.0](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Scan error count in the output
- Reparse point support

## [1.1.1] - 2021-03-24
### Changed
- Project migrated to .NET 5.0

### Fixed
- [#44: Vacuum fails completely after failing to enumerate certain
  path](https://github.com/ForNeVeR/Vacuum/issues/44)

## [1.1.0] - 2021-01-30
### Added
- Documentation files are now included into the program distribution

### Fixed
- Format for path logging
- Broken Unicode characters in the output

## [1.0.1] - 2019-12-15
### Fixed
- [#26: Infinite looping](https://github.com/ForNeVeR/Vacuum/issues/26) when
  encountered a path with spaces
- [#28: Paths with dot aren't scanned
  properly](https://github.com/ForNeVeR/Vacuum/issues/28)

## [1.0.0] - 2019-10-17
### Changed
- [#24: Port to .NET Core](https://github.com/ForNeVeR/Vacuum/issues/24)
- Improved logging for file system errors

## [0.5.1] - 2016-05-27
### Fixed
- [#17: `PathTooLongException`](https://github.com/ForNeVeR/Vacuum/issues/17)

## [0.5.0] - 2016-04-27
### Added
- Flag to free a specified amount of space

## [0.4.0] - 2016-04-03
### Removed
- Possibility to set clean period in months

## [0.3.0] - 2016-02-09
### Added
- Embedded help
- Flag to override cleaned directory
- Flag to override period after which items are considered for cleanup

## [0.2.0] - 2016-01-29
### Changed
- Consider date of a directory itself, in addition to the contained files' dates

## [0.1.0] - 2016-01-24
### Added
- Statistics logging

## [0.0.1] - 2016-01-23
This is the initial version of the program to delete old entries from the system
temporary directory.

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
[Unreleased]: https://github.com/ForNeVeR/Vacuum/compare/v1.1.1...HEAD
