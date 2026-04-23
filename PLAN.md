# Milestones 14 and 15 Active Plan

## Status

- Drafted: 2026-04-23
- Execution started: 2026-04-23
- Scope: finish Milestone 14 performance hardening and Milestone 15 packaging/docs/v1 release work.
- Current state: Milestone 14 is closed; remaining work is Milestone 15 release closure.

## Baseline

As of this pass:

- Milestones 1-14 are complete.
- Milestone 15 is in progress.
- Sequential solution verification is the reliable full test command:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1
```

The latest sequential solution run passed:

- 199 passed
- 17 skipped
- 216 total

Parallel solution tests can still hit a Visual Studio VSIX packaging/file-lock race around `extension.vsixmanifest`.

## Completed In This Pass

- Added `Csxaml.Benchmarks`.
- Added generator, metadata, runtime reconciliation, and tooling benchmark scenarios.
- Added `docs/performance-and-scale.md`.
- Added `docs/native-props-events.md`.
- Added `docs/packaging-and-release.md`.
- Added `docs/getting-started.md`.
- Added starter sample under `samples/Csxaml.Starter`.
- Added preview package metadata for:
  - `Csxaml.ControlMetadata`
  - `Csxaml.Runtime`
  - `Csxaml.Generator`
  - `Csxaml.Testing`
- Added package-resolved `Csxaml.Generator` build integration through `buildTransitive` assets.
- Validated a clean package consumer restored from `artifacts/packages` with no repo-local project references.
- Recorded BenchmarkDotNet `ShortRun` baselines for generator, metadata, runtime, and tooling scenarios.
- Optimized tooling markup completion by avoiding the C# projection/workspace path for plain markup tag and attribute-name contexts.
- Marked demo, fixture, tooling host, and editor projects as non-packable where appropriate.
- Updated `README.md`.
- Updated `ROADMAP.md`.

## Verification Completed

```powershell
dotnet build .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj --no-restore
dotnet run --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -c Release -- --filter *MetadataBenchmarks.BuiltInControlLookup* --job Dry --warmupCount 1 --iterationCount 1
dotnet build .\samples\Csxaml.Starter\Csxaml.Starter.csproj --no-restore -m:1
dotnet build .\Csxaml.Demo\Csxaml.Demo.csproj --no-restore
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj --no-restore
dotnet pack .\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Generator\Csxaml.Generator.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Testing\Csxaml.Testing.csproj -c Release -o .\artifacts\packages
dotnet test .\Csxaml.sln --no-restore -m:1
```

All commands above completed successfully.

Clean package-consumer validation also passed from a temporary `artifacts/package-consumer-*` app:

- restored from `artifacts/packages`
- referenced `Csxaml.Generator` and `Csxaml.Runtime`
- disabled repo-level `Directory.Build.*` imports
- generated `MainPage.g.cs`
- generated `MainPage.map.json`
- built successfully

The latest full sequential solution verification still passed after the tooling optimization:

- 199 passed
- 17 skipped
- 216 total

## Remaining Work

### 1. Starter Template Package

Turn `samples/Csxaml.Starter` into a real template package only after clean package consumption works.

The starter sample is useful now, but it still depends on repo-local project references and root build imports. Clean package consumption now works, so this can be converted into a template package in a later slice.

### 2. Final V1 Release Pass

Before tagging v1:

- update `README.md`
- update `ROADMAP.md`
- update `LANGUAGE-SPEC.md` if behavior changed
- update `docs/supported-feature-matrix.md`
- run full benchmarks and record results
- run package validation from a clean consumer
- run sequential solution tests
- build demo, starter sample, benchmark project, and Visual Studio extension
- inspect packages for accidental repo-local paths
- write release notes

Do not create a git tag unless the user explicitly asks for a release tag.

## Closure Rules

Before marking Milestone 15 complete:

- package boundaries are final
- generator/build package works
- clean package consumption works
- starter sample or template exists
- required docs are current
- release/versioning process is documented
- final verification passes

## Known Risks

- current baselines are recorded from `ShortRun` benchmark jobs rather than CI trend capture
- generator/runtime exact version alignment is documented but not strongly enforced by restore yet
- VS Code packaging remains local-development oriented
- external attached-property owner discovery, named slots, richer event args, virtualization, and `DataContext`-heavy third-party interop remain outside the current v1 slice
