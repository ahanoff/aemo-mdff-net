# Contributing Guidelines

## Prerequisites

- Install the supported .NET SDKs for the target frameworks listed in `Directory.Build.props`.
- This repository currently targets `net8.0`, `net9.0`, and `net10.0`.

## Repository Layout

- `AEMO.MDFF.sln` is the root solution.
- Library code lives in `src/AEMO.MDFF`.
- Tests and sample data live in `src/AEMO.MDFF.Tests`.
- Shared MSBuild settings live in `Directory.Build.props`.
- Central NuGet package versions live in `Directory.Packages.props`.

## Build And Test

Run commands from the repository root:

```shell
dotnet restore AEMO.MDFF.sln
dotnet build AEMO.MDFF.sln
dotnet test AEMO.MDFF.sln
```

If your local machine has a newer .NET runtime but not every older targeted runtime, use:

```shell
DOTNET_ROLL_FORWARD=Major dotnet test AEMO.MDFF.sln
```

To validate packaging after a Release build:

```shell
dotnet build AEMO.MDFF.sln --configuration Release
dotnet pack AEMO.MDFF.sln --configuration Release --no-build --output nupkgs
```

## Pull Requests

- Use conventional commits, for example `fix(nem12): ...`, `feat(nem13): ...`, or `chore(build): ...`.
- Keep each PR focused on one concern where practical.
- Include tests for parser behavior changes and malformed record ordering.
- Do not commit generated `bin`, `obj`, or local package output files.

## Parser Guidelines

- Keep MDFF readers streaming: parse one CSV row at a time and yield records incrementally.
- Avoid parser state that survives across calls to `ReadAsync`.
- Keep per-record allocations bounded.
- Model optional MDFF fields as nullable.
- Use explicit `InvalidDataException` messages for invalid record ordering or unsupported record types.
