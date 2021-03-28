Vacuum Maintainership
=====================

Release
-------

To release a new version:
1. Update the copyright year in [the license file][license], if required.
2. Make sure the version number consists of three numbers (`1.0.0` is ok, `1.0`
   isn't).
3. Change the version number in `Vacuum/Vacuum.fsproj`.
4. Make sure there's a properly formed version entry in [the
   changelog][changelog].
5. Push a tag named `v<VERSION>` to GitHub.

[changelog]: ./CHANGELOG.md
[license]: ./LICENSE.md
