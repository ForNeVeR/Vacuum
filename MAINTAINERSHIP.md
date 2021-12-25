Vacuum Maintainership
=====================

Release
-------

To release a new version:
1. Update the copyright year in [the license file][license], if required.
2. Choose the new version according to [Semantic Versioning][semver]. It should consist of three numbers (i.e. `1.0.0`).
3. Change the version number in `Vacuum/Vacuum.fsproj`.
4. Make sure there's a properly formed version entry in [the
   changelog][changelog].
5. Ensure that the third-party licenses are up-to-date.
6. Push a tag named `v<VERSION>` to GitHub.

[changelog]: ./CHANGELOG.md
[license]: ./LICENSE.md
[semver]: https://semver.org/spec/v2.0.0.html
