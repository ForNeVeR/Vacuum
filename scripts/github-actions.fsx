// SPDX-FileCopyrightText: 2024-2026 Vacuum contributors <https://github.com/ForNeVeR/Vacuum>
//
// SPDX-License-Identifier: MIT

#r "nuget: Generaptor, 1.9.1"

open System
open Generaptor
open Generaptor.GitHubActions
open type Generaptor.GitHubActions.Commands

let mainBranch = "master"

let windowsImage = "windows-2019"
let linuxImage = "ubuntu-22.04"

let workflows = [
    let checkOut = step(
        name = "Check out the sources",
        usesSpec = Auto "actions/checkout"
    )

    workflow "main" [
        name "Main"
        onPushTo mainBranch
        onPullRequestTo mainBranch
        onSchedule(day = DayOfWeek.Saturday)
        onWorkflowDispatch

        let dotNetJob id steps = job id [
            setEnv "DOTNET_CLI_TELEMETRY_OPTOUT" "1"
            setEnv "DOTNET_NOLOGO" "1"
            setEnv "NUGET_PACKAGES" "${{ github.workspace }}/.github/nuget-packages"

            step(
                name = "Check out the sources",
                uses = "actions/checkout@v1"
            )
            step(
                name = "Set up .NET SDK",
                uses = "actions/setup-dotnet@v1"
            )
            step(
                name = "Cache NuGet packages",
                uses = "actions/cache@v4",
                options = Map.ofList [
                    "key", "${{ runner.os }}.nuget.${{ hashFiles('**/*.*proj', '**/*.props') }}"
                    "path", "${{ env.NUGET_PACKAGES }}"
                ]
            )

            yield! steps
        ]

        dotNetJob "verify-workflows" [
            runsOn "ubuntu-24.04"
            step(run = "dotnet fsi ./scripts/github-actions.fsx verify")
        ]

        dotNetJob "main" [
            runsOn windowsImage
            step(
                name = "Build",
                run = "dotnet build"
            )
            step(
                name = "Test",
                run = "dotnet test",
                timeoutMin = 10
            )
        ]

        job "encoding" [
            runsOn linuxImage
            checkOut

            step(name = "Verify encoding", shell = "pwsh", run = "scripts/Test-Encoding.ps1")
        ]

        job "licenses" [
            runsOn linuxImage
            checkOut

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

            checkOut

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

exit <| EntryPoint.Process fsi.CommandLineArgs workflows
