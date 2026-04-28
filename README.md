# CSXAML

CSXAML is an experimental source-generated language for building WinUI apps with XAML-like markup and real C# expressions.

The repo and public release artifacts are licensed under Apache-2.0.

## Current Status

The compiler/runtime path is broad enough for a credible v1 slice, and the remaining work is mostly release validation, public hosting, and polish around distribution.

Current productization state:

- public package boundary: `Csxaml` and `Csxaml.Runtime`
- starter sample and template work exist in the repo
- benchmark and performance documentation exist for Milestone 14
- VS Code and Visual Studio extension packaging paths exist
- XML API documentation is enabled on developer-facing assemblies
- DocFX site source exists and generates API reference from XML docs at build time
- GitHub Actions owns CI, packaging, release prep, publish, and docs-site deployment workflows

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
  Hostless component test support helpers.

- `Csxaml.Benchmarks`  
  BenchmarkDotNet and WinUI smoke harnesses for generator, metadata, runtime, tooling, and projection measurements.

### Tooling and editor integration

- `Csxaml.Tooling.Core`  
  Shared language-service logic: completion, definitions, formatting, hover, semantic tokens, markup scanning, C# projection, and diagnostics.

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
- `docs-site/` - DocFX documentation site source
- `scripts/` - helper scripts
- `samples/` - outside-consumer and starter examples
- `templates/` - template package sources
- `artifacts/` - build outputs and packaged artifacts

## Where To Start

If you are new to the repo, this reading order works well:

1. `LANGUAGE-SPEC.md`
2. `docs-site/index.md`
3. `Csxaml.Generator/`
4. `Csxaml.Runtime/`
5. `Csxaml.Demo/`
6. `Csxaml.Tooling.Core/`

## Key Docs

- [Documentation Site Source](docs-site/index.md)
- [Language Specification](LANGUAGE-SPEC.md)
- [Roadmap](ROADMAP.md)
- [Package Installation](docs/package-installation.md)
- [Native Props And Events](docs/native-props-and-events.md)
- [Performance And Scale](docs/performance-and-scale.md)
- [Release And Versioning](docs/release-and-versioning.md)
- [Component Testing](docs/component-testing.md)
- [Agent Working Rules And Project Inventory](AGENTS.md)
- [VS Code Extension](VSCodeExtension/README.md)

## Documentation Site

The DocFX documentation site lives under `docs-site/` and generates API reference pages from XML documentation comments at build time.

Build it locally from the repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\docs\Invoke-DocsBuild.ps1
```

Preview it locally:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\docs\Invoke-DocsBuild.ps1 -Serve
```

Generated API YAML is written under `obj\docfx\api`, and the static site is written under `_site`.

## Install Story

For outside consumers, the intended package install path is:

- install `Csxaml`
- keep `Microsoft.WindowsAppSDK` in the app project
- author `.csxaml` files normally

The package install guide lives in [docs/package-installation.md](docs/package-installation.md).

For a small outside-consumer example that uses the package path instead of repo-local project references, see [samples/PackageHello](samples/PackageHello/README.md).

## Release Model

CSXAML treats release governance as part of the product surface:

- semantic versioning is the public version contract
- Conventional Commits are the semantic input to release notes and version bumps
- `git-cliff` generates `CHANGELOG.md` and release notes
- GitHub Actions is the system of record for CI, packaging, docs, and publishing
- pushes to `develop` create preview releases, and pushes to `master` create stable releases
- release tags are created by automation after publish succeeds

The current release and versioning policy lives in [docs/release-and-versioning.md](docs/release-and-versioning.md).

## Current Limitations

These areas remain intentionally outside the current v1 promise:

- named slots and slot fallback content
- broader external attached-property owner discovery
- richer event-argument projection beyond the current supported slice
- virtualization and very large visible-list strategies
- `DataContext`-heavy third-party control interop
- dedicated source-level lifecycle or cancellation syntax

The supported feature matrix lives in [docs/supported-feature-matrix.md](docs/supported-feature-matrix.md).
