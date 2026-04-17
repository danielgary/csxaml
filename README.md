# csxaml

CSXAML is an experimental source-generated language for building WinUI apps with XAML-like structure and real C# expressions.

The repo and public release artifacts are licensed under Apache-2.0.

## Repo At A Glance

The repo is organized around four main layers:

- compiler and metadata:
  `Csxaml.ControlMetadata`, `Csxaml.ControlMetadata.Generator`, `Csxaml.Generator`
- runtime:
  `Csxaml.Runtime`, `Csxaml.Testing`
- performance and measurement:
  `Csxaml.Benchmarks`
- tooling and editor integration:
  `Csxaml.Tooling.Core`, `Csxaml.LanguageServer`, `Csxaml.VisualStudio`, `VSCodeExtension/`
- demo and build fixtures:
  `Csxaml.Demo`, `Csxaml.ExternalControls`, `Csxaml.ProjectSystem.Components`, `Csxaml.ProjectSystem.Consumer`

## Project Guide

### Core language and runtime

- `Csxaml.ControlMetadata`  
  Shared metadata model for controls, properties, events, child-content rules, and value-kind hints.

- `Csxaml.ControlMetadata.Generator`  
  Generates that metadata from WinUI and supported external controls.

- `Csxaml.Generator`  
  Parses `.csxaml`, validates it, and emits deterministic generated C#.

- `Csxaml.Runtime`  
  Retained-mode runtime that reconciles logical trees and projects them to WinUI.

- `Csxaml.Testing`  
  Test support helpers used by runtime and integration tests.

- `Csxaml.Benchmarks`  
  BenchmarkDotNet and WinUI smoke harnesses for generator, metadata, runtime, and projection measurements.

### Tooling and editor integration

- `Csxaml.Tooling.Core`  
  Shared language-service logic: completion, definitions, formatting, semantic tokens, markup scanning, and C# projection.

- `Csxaml.LanguageServer`  
  LSP host built on top of `Csxaml.Tooling.Core`.

- `Csxaml.VisualStudio`  
  Visual Studio extension host and VSIX packaging.

- `VSCodeExtension/`  
  VS Code extension assets, snippets, grammar, and extension host code.

### Demo and fixture apps

- `Csxaml.Demo`  
  Main demo WinUI app showing the current CSXAML authoring model.

- `Csxaml.ExternalControls`  
  Sample external controls used to prove external-control interop.

- `Csxaml.ProjectSystem.Components`  
  Fixture component library for project-reference validation.

- `Csxaml.ProjectSystem.Consumer`  
  Fixture consumer app that references the generated component library.

### Test projects

- `Csxaml.ControlMetadata.Generator.Tests`
- `Csxaml.Generator.Tests`
- `Csxaml.Runtime.Tests`
- `Csxaml.Tooling.Core.Tests`
- `Csxaml.VisualStudio.Tests`
- `Csxaml.ProjectSystem.Tests`

These projects provide regression coverage for metadata generation, parsing, validation, emission, runtime behavior, tooling, editor hosts, and project-system integration.

## Non-Project Folders

- `build/` - shared MSBuild targets and generation wiring
- `docs/` - supporting documentation
- `scripts/` - helper scripts
- `artifacts/` - build outputs and packaged artifacts

## Where To Start

If you are new to the repo, this reading order works well:

1. `LANGUAGE-SPEC.md`
2. `Csxaml.Generator/`
3. `Csxaml.Runtime/`
4. `Csxaml.Demo/`
5. `Csxaml.Tooling.Core/`

## Key Docs

- [Language Specification](LANGUAGE-SPEC.md)
- [Roadmap](ROADMAP.md)
- [Package Installation](docs/package-installation.md)
- [Native Props And Events](docs/native-props-and-events.md)
- [Performance And Scale](docs/performance-and-scale.md)
- [Release And Versioning](docs/release-and-versioning.md)
- [Component Testing](docs/component-testing.md)
- [Agent Working Rules And Project Inventory](AGENTS.md)
- [VS Code Extension](VSCodeExtension/README.md)

## Install Story

For outside consumers, the intended package install path is:

- install `Csxaml`
- keep `Microsoft.WindowsAppSDK` in the app project
- author `.csxaml` files normally

The package install guide lives in [docs/package-installation.md](docs/package-installation.md).

For a small outside-consumer example that uses the package path instead of repo-local project references, see [samples/PackageHello](samples/PackageHello/README.md).

## Release Model

CSXAML now treats release governance as part of the product surface:

- semantic versioning is the public version contract
- Conventional Commits are the semantic input to release notes and version bumps
- `git-cliff` generates `CHANGELOG.md` and release notes
- GitHub Actions is the system of record for CI, packaging, and publishing
- pushes to `develop` create preview releases, and pushes to `master` create stable releases
- release tags are created by automation after publish succeeds

The current release and versioning policy lives in [docs/release-and-versioning.md](docs/release-and-versioning.md).
