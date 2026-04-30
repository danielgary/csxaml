# Packaging and Release

This document describes the intended v1 packaging and release shape.

Status: preview packages can be packed locally, including the build-transitive `Csxaml.Generator` package. A clean package-consumer validation app has restored from `artifacts/packages`, generated `.csxaml` output, and produced source maps. The repo is still not a tagged v1 release.

## Current Prototype Path

The repo-local development build path is:

- `Directory.Build.props` imports `build/Csxaml.props`.
- `Directory.Build.targets` imports `build/Csxaml.targets`.
- projects opt in with `<EnableCsxaml>true</EnableCsxaml>`.
- `build/Csxaml.targets` globs `**\*.csxaml` into `CsxamlSource` by default.
- `build/Csxaml.targets` runs the generator with `dotnet run --project "$(CsxamlGeneratorProject)"`.
- `CsxamlGeneratorProject` defaults to `..\Csxaml.Generator\Csxaml.Generator.csproj` relative to the build asset.
- generated files are written under `$(IntermediateOutputPath)Csxaml`.

The preview package build path is:

- app projects reference `Csxaml.Generator` with `PrivateAssets="all"`.
- `Csxaml.Generator` carries `buildTransitive/Csxaml.Generator.props` and `buildTransitive/Csxaml.Generator.targets`.
- package consumers run `dotnet exec` against `tools/net10.0/any/Csxaml.Generator.dll`.
- `CsxamlGeneratorProject` remains available as a repo/debug override.
- generated files and source maps are still written under `$(IntermediateOutputPath)Csxaml`.

Repo projects may continue using project references while development is active. Clean validation should use packages from `artifacts/packages` and should disable repo-level `Directory.Build.*` imports if the validation project lives under this checkout.

## Proposed V1 Package Boundaries

| Package | Responsibility | Current repo source |
| --- | --- | --- |
| `Csxaml.Runtime` | Public runtime types, component instances, state, reconciliation, WinUI projection, adapters, and `CsxamlHost`. | `Csxaml.Runtime` |
| `Csxaml.ControlMetadata` | Shared metadata contracts and generated built-in control metadata used by runtime, generator, and tooling. | `Csxaml.ControlMetadata` |
| `Csxaml.Generator` | Build-time `.csxaml` generator package and MSBuild integration assets. This should be a build package, not an app runtime API. | `Csxaml.Generator`, `build/Csxaml.props`, `build/Csxaml.targets` |
| `Csxaml.Testing` | Hostless logical-tree component testing helpers aligned with the runtime. | `Csxaml.Testing` |
| `Csxaml.Templates` | Starter template package once the starter app exists. | Not implemented yet |

Editor deliverables are separate from the core NuGet path:

- `Csxaml.VisualStudio` should ship as a VSIX.
- `VSCodeExtension` is currently local-development oriented.
- `Csxaml.Tooling.Core` and `Csxaml.LanguageServer` are implementation packages for editor hosts, not the first authoring package a WinUI app should reference.

Current repo packaging posture:

- `Csxaml.ControlMetadata`, `Csxaml.Runtime`, and `Csxaml.Testing` are packable preview library packages.
- `Csxaml.Generator` is a packable preview build package with `buildTransitive` assets and a packaged generator payload.
- demo, fixture, tooling-host, and editor projects are explicitly non-packable.

`Csxaml.ControlMetadata.Generator` should remain an internal/repo tool unless a specific public metadata-authoring story is designed later.

## Build Asset Ownership

The generator/build package should own the MSBuild assets.

Current preview layout:

- `buildTransitive/Csxaml.Generator.props`
- `buildTransitive/Csxaml.Generator.targets`
- `tools/net10.0/any/Csxaml.Generator.dll`
- generator runtime dependencies, including `Csxaml.ControlMetadata.dll`

The runtime package should not own build targets. Keeping generation assets in the generator package lets projects reference the runtime without accidentally enabling `.csxaml` compilation.

The current properties should remain recognizable:

- `EnableCsxaml`
- `CsxamlEnableDefaultSourceGlob`
- `CsxamlIntermediateRoot`
- `CsxamlGeneratedDirectory`
- `CsxamlDefaultNamespace`
- `CsxamlInternalGeneratedNamespace`

The package path resolves `CsxamlGeneratorAssembly` from the package and runs it through `dotnet exec`. `CsxamlGeneratorProject` is kept as a repo/debug override, but package consumers do not need a generator project path.

## Version Alignment

Cross-version generator/runtime mixing is not supported.

For v1, these packages should ship from the same release train and use the same package version:

- `Csxaml.Generator`
- `Csxaml.Runtime`
- `Csxaml.ControlMetadata`
- `Csxaml.Testing`

The generator may emit code that depends on runtime hooks added in the same release. `Csxaml.Testing` may depend on runtime logical-tree and activation behavior from the same release. Consumers should upgrade these packages together.

The package implementation should enforce this where practical:

- exact package dependencies for tightly coupled packages
- a generated runtime contract/version check when generated code can validate it cheaply
- clear restore/build diagnostics when mismatched packages are detected

## Intended Package Contents

`Csxaml.Runtime` should contain:

- `lib/net10.0-windows10.0.19041.0/Csxaml.Runtime.dll`
- XML documentation and symbols
- dependencies on `Microsoft.WindowsAppSDK` and `Microsoft.Extensions.DependencyInjection`

`Csxaml.ControlMetadata` should contain:

- `lib/net10.0/Csxaml.ControlMetadata.dll`
- XML documentation and symbols
- generated built-in metadata for the supported control slice

`Csxaml.Generator` should contain:

- build assets under `buildTransitive/`
- the generator executable or task payload
- generator runtime dependencies, including control metadata
- no public app runtime surface beyond the build integration

`Csxaml.Testing` should contain:

- `lib/net10.0-windows10.0.19041.0/Csxaml.Testing.dll`
- XML documentation and symbols
- an aligned dependency on `Csxaml.Runtime`

`Csxaml.Templates`, once implemented, should contain:

- a minimal WinUI CSXAML starter app
- a small component library sample only if it stays easy to understand
- no hidden dependency on the repo checkout

## Local Pack And Validation

Current pack commands produce the preview package set used for clean package-consumer validation.

From the repo root:

```powershell
dotnet restore .\Csxaml.sln
dotnet test .\Csxaml.sln --no-restore -m:1
New-Item -ItemType Directory -Force .\artifacts\packages
dotnet pack .\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Generator\Csxaml.Generator.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Testing\Csxaml.Testing.csproj -c Release -o .\artifacts\packages
```

Inspect package contents before treating any package as releasable:

```powershell
Get-ChildItem .\artifacts\packages
tar -tf .\artifacts\packages\Csxaml.ControlMetadata.*.nupkg
tar -tf .\artifacts\packages\Csxaml.Runtime.*.nupkg
tar -tf .\artifacts\packages\Csxaml.Generator.*.nupkg
tar -tf .\artifacts\packages\Csxaml.Testing.*.nupkg
```

Clean consumer validation should use only a local NuGet source plus NuGet.org for third-party dependencies. The validation project should reference `Csxaml.Generator` and `Csxaml.Runtime`, not repo-local project references:

```powershell
dotnet build .\path\to\CleanConsumer.csproj `
  /p:ImportDirectoryBuildProps=false `
  /p:ImportDirectoryBuildTargets=false `
  /p:RestoreSources="$(Resolve-Path .\artifacts\packages);https://api.nuget.org/v3/index.json"
```

The latest validation created a temporary app under `artifacts/package-consumer-*`, restored packages from `artifacts/packages`, generated `MainPage.g.cs`, wrote `MainPage.map.json`, and built successfully.

## Release Checklist

Before a v1 tag:

- run `dotnet test .\Csxaml.sln --no-restore -m:1`
- run the Milestone 14 benchmark suite and record the scale envelope
- build the demo from a clean checkout
- validate the starter sample/template from a clean checkout
- pack all intended packages into `artifacts/packages`
- inspect package contents for build assets, symbols, XML docs, and accidental repo-local paths
- consume packages from a local NuGet source in a clean app
- verify generated files and source maps land under `obj\...\Csxaml`
- verify package versions are aligned
- verify release notes describe supported features, known gaps, and breaking changes
- verify README, roadmap, compatibility policy, and supported feature matrix agree
- build and smoke-test the Visual Studio VSIX if it is part of the release
- create the release tag only after package validation passes

## Versioning Notes

Use SemVer.

- Stable release tags use `vMAJOR.MINOR.PATCH`, for example `v1.0.0`.
- Preview tags use `vMAJOR.MINOR.PATCH-preview.N`.
- Release-candidate tags use `vMAJOR.MINOR.PATCH-rc.N`.
- Breaking source, generated-code, runtime, or testing API changes after v1 require a major version.
- Additive language/runtime features can use a minor version.
- Bug fixes and diagnostics-only improvements can use a patch version when they preserve behavior.

Every release should state the required alignment between generator, runtime, metadata, and testing packages.

## Still Experimental

These areas should not be presented as finished v1 packaging commitments yet:

- starter templates
- benchmark-backed v1 scale limits
- VS Code packaging
- editor hover and richer code actions
- broad external attached-property owner discovery
- external controls beyond the documented supported shape
- named slots, fallback slot content, and fragment-root slot pass-through
- first-class CSXAML virtualization abstractions and stable very-large-list
  authoring commitments
- `DataContext`-heavy third-party control interop
- dedicated source-level lifecycle or cancellation syntax
- full trim/AOT support guarantees
