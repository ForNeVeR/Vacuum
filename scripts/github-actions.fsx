// SPDX-FileCopyrightText: 2024 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>
//
// SPDX-License-Identifier: MIT

#r "nuget: Generaptor.Library, 1.5.0"

open System
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands
open type Generaptor.Library.Actions
open type Generaptor.Library.Patterns

let mainBranch = "master"

let windowsImage = "windows-2019"
let linuxImage = "ubuntu-22.04"

let workflows = [
    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)
        onWorkflowDispatch

        job "main" [
            runsOn windowsImage
            checkout
            yield! dotNetBuildAndTest(sdkVersion = "6.0.x")
        ]

        job "encoding" [
            runsOn linuxImage
            checkout

            step(name = "Verify encoding", shell = "pwsh", run = "scripts/Test-Encoding.ps1")
        ]

        job "licenses" [
            runsOn linuxImage
            checkout

            step(name = "REUSE license check", uses = "fsfe/reuse-action@v3")
        ]
    ]
    workflow "release" [
        name "Release"
        onPushTags "v*"
        onWorkflowDispatch

        job "release" [
            runsOn windowsImage
            setEnv "DOTNET_NOLOGO" "1"
            setEnv "DOTNET_CLI_TELEMETRY_OPTOUT" "1"

            step(
                name = "Read version from ref",
                id = "version",
                shell = "pwsh",
                run = "Write-Output \"::set-output name=version::$($env:GITHUB_REF -replace '^refs/tags/v', '')\""
            )

            checkout

            step(
                name = "Set up .NET SDK",
                uses = "actions/setup-dotnet@v4",
                options = Map.ofList ["dotnet-version", "6.0.x"]
            )

            step(
                name = "Publish",
                run = "dotnet publish Vacuum --runtime win-x64 --self-contained --configuration Release --output ./publish -p:PublishTrimmed=true -p:PublishSingleFile=true"
            )
            step(
                name = "Package distribution",
                shell = "pwsh",
                run = "Compress-Archive ./publish/* ./vacuum-v${{ steps.version.outputs.version }}.zip"
            )

            step(
                name = "Read changelog",
                id = "changelog",
                uses = "ForNeVeR/ChangelogAutomation.action@v1",
                options = Map.ofList ["output", "./release-notes.md"]
            )

            step(
                name = "Create release",
                id = "release",
                uses = "actions/create-release@v1",
                env = Map.ofList ["GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"],
                options = Map.ofList [
                    "tag_name", "${{ github.ref }}"
                    "release_name", "Vacuum v${{ steps.version.outputs.version }}"
                    "body_path", "./release-notes.md"
                ]
            )
            step(
                name = "Upload distribution",
                uses = "actions/upload-release-asset@v1",
                env = Map.ofList ["GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"],
                options = Map.ofList [
                    "upload_url", "${{ steps.release.outputs.upload_url }}"
                    "asset_path", "./vacuum-v${{ steps.version.outputs.version }}.zip"
                    "asset_name", "vacuum-v${{ steps.version.outputs.version }}.zip"
                    "asset_content_type", "application/zip"
                ]
            )
        ]
    ]
]

EntryPoint.Process fsi.CommandLineArgs workflows
