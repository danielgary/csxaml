# CSXAML

CSXAML is an experimental source-generated language for building WinUI apps with XAML-like markup and real C# expressions.

The project is currently a broad prototype rather than a packaged v1 release. The compiler/runtime path is well covered through Milestone 14 of the roadmap; the remaining v1 work is mostly packaging, templates, release process, and more polished distribution.

## Current Status

- Milestones 1-14 are implemented and documented in [ROADMAP.md](ROADMAP.md).
- Milestone 14, performance and large-app hardening, is complete with recorded BenchmarkDotNet baselines and one measured tooling optimization.
- Milestone 15, packaging/templates/docs/v1 release, is in progress with preview packages and clean package-consumer validation in place.
- The repo contains a working WinUI demo, a shared language server, a Visual Studio extension host, a local VS Code extension, and hostless component testing APIs.
- A small starter sample lives under `samples/Csxaml.Starter`.
- Benchmark scaffolding lives under `Csxaml.Benchmarks`.
- The supported source contract is described in [LANGUAGE-SPEC.md](LANGUAGE-SPEC.md), with compatibility limits in [docs/compatibility-policy.md](docs/compatibility-policy.md).

## What Works Today

The current prototype supports:

- `.csxaml` source generation through shared MSBuild assets in `build/` and the preview `Csxaml.Generator` package
- one `component Element` declaration per `.csxaml` file
- file-scoped namespaces, normal `using` directives, aliases, and `using static`
- typed component parameters and generated props records
- component-local `State<T>`
- explicit `inject Type name;` service declarations
- local helper code and file-local helper declarations
- `render <Root />;` as the final markup statement
- native elements, component elements, attributes, events, and attached properties
- `if` and constrained `foreach` blocks in markup
- keyed child/component identity preservation
- default slots through `<Slot />`
- metadata-driven WinUI property/event validation and emission
- retained native rendering with adapters for the supported control set
- external controls from referenced assemblies through normal `using` imports and aliases
- hostless logical-tree component tests through `Csxaml.Testing`
- source maps, `#line` mappings, and wrapped runtime exception context
- shared tooling semantics through `Csxaml.Tooling.Core` and `Csxaml.LanguageServer`

## Supported Control Slice

Built-in metadata currently covers:

- `Border`
- `Button`
- `CheckBox`
- `Grid`
- `ScrollViewer`
- `StackPanel`
- `TextBlock`
- `TextBox`

Attached-property metadata currently covers:

- `Grid.Row`
- `Grid.Column`
- `Grid.RowSpan`
- `Grid.ColumnSpan`
- `AutomationProperties.Name`
- `AutomationProperties.AutomationId`

External controls are supported when they can be discovered deterministically from project references and fit the documented supported shape. See [docs/external-control-interop.md](docs/external-control-interop.md).

## Repo Map

| Path | Purpose |
| --- | --- |
| `Csxaml.Generator` | CLI generator, parser, validator, emitter, diagnostics, source maps |
| `Csxaml.ControlMetadata` | Shared built-in and generated control metadata model |
| `Csxaml.ControlMetadata.Generator` | Reflection-based metadata generator for the curated WinUI control slice |
| `Csxaml.Runtime` | Logical nodes, component instances, state, reconciliation, WinUI projection, control adapters, hosting |
| `Csxaml.Testing` | Hostless C# component testing helpers |
| `Csxaml.Benchmarks` | BenchmarkDotNet scenarios for generator, metadata, runtime, and tooling performance |
| `Csxaml.Tooling.Core` | Shared editor services for completion, diagnostics, semantic tokens, formatting, definitions, and projected C# |
| `Csxaml.LanguageServer` | LSP wrapper over the shared tooling core |
| `Csxaml.VisualStudio` | Visual Studio 2026 / v18 extension host and VSIX packaging path |
| `VSCodeExtension` | Local VS Code extension using TextMate grammar plus the shared language server |
| `Csxaml.Demo` | WinUI Todo demo that dogfoods DI, controlled inputs, layout, slots, styles, and external controls |
| `samples/Csxaml.Starter` | Minimal WinUI CSXAML starter sample |
| `Csxaml.ProjectSystem.*` | Cross-project generation and consumption fixtures |
| `*.Tests` | MSTest coverage for generator, runtime, tooling, metadata, project-system, and VS packaging behavior |
| `docs/` | Focused authoring, compatibility, diagnostics, lifecycle, testing, interop, and bootstrap docs |

## Build And Test

Typical verification from the repo root after restore:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1
```

The sequential `-m:1` form avoids a Visual Studio VSIX packaging race seen during parallel solution builds on this machine.

To build the demo:

```powershell
dotnet build .\Csxaml.Demo\Csxaml.Demo.csproj
```

To build the starter sample:

```powershell
dotnet build .\samples\Csxaml.Starter\Csxaml.Starter.csproj --no-restore
```

To build and smoke the benchmark harness:

```powershell
dotnet build .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj
dotnet run --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -c Release -- --filter *MetadataBenchmarks.BuiltInControlLookup* --job Dry --warmupCount 1 --iterationCount 1
```

To build the language server:

```powershell
dotnet build .\Csxaml.LanguageServer\Csxaml.LanguageServer.csproj
```

To build the Visual Studio extension:

```powershell
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj
```

The current local environment used for this audit was .NET SDK `10.0.203` on Windows ARM64.

## Documentation

Start here:

- [ROADMAP.md](ROADMAP.md): milestone status, v1 gates, and planning risks
- [LANGUAGE-SPEC.md](LANGUAGE-SPEC.md): normative draft language contract
- [docs/supported-feature-matrix.md](docs/supported-feature-matrix.md): supported, experimental, and unsupported feature table
- [docs/compatibility-policy.md](docs/compatibility-policy.md): Milestone 13 compatibility promise
- [docs/getting-started.md](docs/getting-started.md): repo-local and preview-package first-use paths
- [docs/native-props-events.md](docs/native-props-events.md): built-in native control props/events guide
- [docs/external-control-interop.md](docs/external-control-interop.md): referenced-control interop model and limits
- [docs/component-testing.md](docs/component-testing.md): hostless component testing APIs
- [docs/debugging-and-diagnostics.md](docs/debugging-and-diagnostics.md): source maps, diagnostics, and runtime failure context
- [docs/lifecycle-and-async.md](docs/lifecycle-and-async.md): mount, disposal, and async-after-unmount behavior
- [docs/performance-and-scale.md](docs/performance-and-scale.md): benchmark commands and v1 scale envelope
- [docs/packaging-and-release.md](docs/packaging-and-release.md): package boundaries and release process
- [docs/visual-studio-bootstrap.md](docs/visual-studio-bootstrap.md): VSIX bootstrap and experimental-instance workflow
- [VSCodeExtension/README.md](VSCodeExtension/README.md): local VS Code extension workflow
- [AGENTS.md](AGENTS.md): repository working rules and code-organization expectations

## Known Gaps

The important unfinished areas are:

- no CI trend or performance regression gate exists yet
- starter template package is not implemented yet
- the current packages are preview validation packages, not a tagged v1 release
- VS Code packaging is still local-development oriented
- hover and richer code actions are not implemented in the shared language server
- named slots, external attached-property owner discovery, richer event-argument projection, virtualization, and `DataContext`-heavy third-party interop remain outside the current v1 slice

This repo intentionally prefers clear, explicit, small units over clever compression. A working implementation that is hard to read is not considered done here.
