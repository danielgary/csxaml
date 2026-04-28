# Supported Feature Matrix

## Release Posture

These labels are used consistently across the README, roadmap, packaging docs, getting-started guide, and release notes.

| Label | Meaning |
| --- | --- |
| `Supported in v1` | Implemented now, documented, and inside the current compatibility promise. |
| `Preview shipping path` | Exists and is intended to ship, but the current artifact is still a preview or local-validation path rather than part of a tagged v1 release. |
| `Experimental` | Usable for exploration, but outside the current v1 promise. |
| `Not in v1` | Explicitly deferred; do not build against it as an expected v1 capability. |

Feature-matrix status mapping:

- `Supported in v1`: covered by Milestone 13 behavior and regression tests
- `Experimental`: available, but not yet part of the stable Milestone 13 promise
- `Not in v1`: intentionally outside the current Milestone 13 and v1 contract

| Area | Status | Notes |
| --- | --- | --- |
| Component parameters | Supported in v1 | Public prop surface only. |
| `State<T>` declarations | Supported in v1 | Component-local mutable UI state. |
| `inject Type name;` | Supported in v1 | Required typed services resolved once per component instance. |
| Conditional markup (`if`) | Supported in v1 | Covered by generator/runtime regression tests. |
| Repeated markup (`foreach`) | Supported in v1 | Includes keyed child retention behavior. |
| Slots | Supported in v1 | Default slot only. Named slots, fallback content, and fragment-root pass-through remain outside v1. |
| Attached properties | Supported in v1 | Supported for the current built-in owner slice, including semantic automation metadata plus owner resolution through ordinary imports and explicit type aliases. Broader owner discovery remains outside v1. |
| Built-in control property/event binding | Supported in v1 | Covered by generator/runtime tests. |
| External controls from referenced assemblies | Supported in v1 | Covered by validation, emission, runtime, and demo interop tests for the documented supported shape only. |
| Solution-local external controls | Supported in v1 | Covered by external-control validation and runtime regression paths for the documented supported shape only. |
| Root instance host path | Supported in v1 | `CsxamlHost(panel, instance)` and hostless coordinator/test flows remain valid. |
| Root type activation with services | Supported in v1 | `CsxamlHost(panel, typeof(Component), services)` and hostless test rendering support this path. |
| `IServiceProvider` activation boundary | Supported in v1 | Public DI boundary for runtime/testing. |
| `ActivatorUtilities` constructor activation | Supported in v1 | Used by the default activator when creating component instances. |
| `OnMounted()` | Supported in v1 | Runtime hook for handwritten `ComponentInstance` types; `.csxaml` source does not yet have dedicated lifecycle syntax. |
| `IDisposable` cleanup | Supported in v1 | Removed components and disposed roots are cleaned up once; handwritten component types can implement cleanup directly. |
| `IAsyncDisposable` cleanup | Supported in v1 | Async root disposal is supported; sync removal paths block on async disposal when needed. |
| Post-unmount state invalidation no-op | Supported in v1 | Stale component instances do not rerender the tree after removal/disposal. |
| `Csxaml.Testing` root-instance render | Supported in v1 | Hostless logical-tree rendering. |
| `Csxaml.Testing` root-type render | Supported in v1 | Supports DI activation. |
| `Csxaml.Testing` service overrides | Supported in v1 | `Action<ServiceCollection>` overloads are available. |
| `Csxaml.Testing` query by automation id/name | Supported in v1 | Preferred semantic query surface. |
| `Csxaml.Testing` query by text/content | Supported in v1 | Works over logical tree properties. |
| `Csxaml.Testing` click/text/checked interactions | Supported in v1 | Designed for common component workflows. |
| Tag and helper-code hover | Supported in v1 | Covers component tags, control tags, native properties/events, attached properties, component parameters, and projected C# helper-code symbols that can be resolved from the current file/workspace. |
| Suggestion-based quick fixes | Supported in v1 | Covers single-symbol replacements for misspelled visible tags and attribute names. |
| Generator/runtime cross-version mixing | Not in v1 | Upgrade generator, runtime, control metadata, and testing together. |
| Keyed/named/optional DI syntax | Not in v1 | Outside Milestone 13. |
| Property injection or markup injection | Not in v1 | Use explicit `inject` declarations instead. |
| Per-component DI scopes | Not in v1 | Host services are authoritative. |
| Subtree service-provider overrides | Not in v1 | Outside Milestone 13. |
| `OnUpdated` / effect hooks / dependency arrays | Not in v1 | Lifecycle remains intentionally small. |
| Full visual/UI automation testing framework | Experimental | WinUI projection tests exist, but hostless logical-tree testing is the supported Milestone 13 story. |

## Productization Snapshot

| Area | Status | Notes |
| --- | --- | --- |
| NuGet package consumption | Preview shipping path | Clean validation exists from locally packed `0.1.0-preview.1` artifacts, matching the public package examples. |
| Starter sample app | Supported in v1 | `samples/Csxaml.Starter` exists as a small example app. |
| Starter `dotnet new` template | Not in v1 | No public starter-template package is documented in the current preview docs. Use the blank WinUI app path or `samples/Csxaml.Starter` instead. |
| Visual Studio VSIX workflow | Supported in v1 | Documented bootstrap and packaging path exist for the current Visual Studio 18 authoring story. |
| VS Code extension workflow | Preview shipping path | The repo-local extension still supports local iteration, and `scripts/Package-VSCodeExtension.ps1` produces an installable VSIX aligned to the current `0.1.0-preview.1` package line with a bundled language server. |
| Benchmark runner and local perf gate | Supported in v1 | `scripts/Run-Benchmarks.ps1` writes timestamped benchmark snapshots, maintains `artifacts/benchmarks/baseline.json` and `baseline.md`, and gates the 1000-item hostless runtime rerender lanes on the audited runner while metadata, generator, and tooling remain report-only. |

## Current Release Watch Items

These are not blockers for the current preview line, but they remain important follow-up work:

- no CI trend lane exists yet for benchmark history
- broader editor code-action coverage remains intentionally small

## Not in V1

These areas are intentionally outside the current v1 contract:

- named slots and slot fallback content
- fragment-root slot pass-through
- broader external attached-property owner discovery
- richer event-argument projection beyond the current supported slice
- virtualization and very large visible-list strategies
- `DataContext`-heavy third-party control interop
- dedicated source-level lifecycle or cancellation syntax
