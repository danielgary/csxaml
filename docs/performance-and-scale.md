# Performance and Scale

## Purpose

This document records the measured Milestone 14 performance story for CSXAML v1.

It exists to answer four questions directly:

1. how generation time grows as a solution gains more `.csxaml` files
2. whether metadata lookup and discovery stay cheap enough for build and tooling paths
3. whether retained runtime reconciliation handles realistic rerender pressure
4. whether editor services remain responsive for small and larger workspaces

The language semantics remain defined by [LANGUAGE-SPEC.md](../LANGUAGE-SPEC.md). This document is about measured implementation posture, not new language rules.

## Audit Environment

The BenchmarkDotNet `ShortRun` baselines were captured on 2026-04-23.

- OS: Windows `10.0.26200.8246`
- Platform: `win-arm64`
- .NET SDK: `10.0.203`
- .NET host runtime: `10.0.7`
- Installed SDKs: `8.0.420`, `10.0.203`

The latest documented full verification used:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1
```

That sequential solution run passed with 199 passed, 17 skipped, and 216 total tests. The sequential `-m:1` form remains the reliable command while a parallel VSIX packaging file-lock race is unresolved.

## Commands

Build and list benchmarks:

```powershell
dotnet build .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -c Release
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --list flat
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *
```

Run focused categories:

```powershell
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Generator*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Metadata*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Runtime*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Tooling*
```

For the WinUI projection/editing smoke path:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/perf/run-winui-projection-smoke.ps1
```

That writes:

- `artifacts/benchmarks/winui-smoke.json`
- `artifacts/benchmarks/winui-smoke.md`

## Scenario Categories

Generator scenarios measure:

- 1 component
- 25 components
- 100 components
- 500 components
- components using helper code, state, `if`, `foreach`, and slots

Metadata scenarios measure:

- built-in control metadata lookup
- attached-property metadata lookup
- external control registry lookup

Runtime scenarios measure:

- keyed-list initial render
- keyed reorder
- middle insert rerender
- list/detail editor rerender
- `State<T>` no-op versus `Touch()` behavior

Tooling scenarios measure:

- tag completion in a small workspace
- attribute completion for built-in controls
- diagnostics over a small workspace
- formatting over mixed C#/markup documents

## Results Summary

| Category | Scenario | Mean | Allocated | Notes |
| --- | --- | ---: | ---: | --- |
| Generator | 1 component | `145.7 us` | `247.24 KB` | Direct generator path, not process startup. |
| Generator | 25 components | `3.602 ms` | `6050.87 KB` | Same scenario shape as above. |
| Generator | 100 components | `18.068 ms` | `24169.98 KB` | Same scenario shape as above. |
| Generator | 500 components | `91.597 ms` | `121019.99 KB` | Same scenario shape as above. |
| Metadata | Built-in control lookup | `25.39 ns` | `0 B` | Eight built-in control lookups. |
| Metadata | Attached-property lookup | `39.84 ns` | `248 B` | Four attached-property lookups. |
| Runtime | Initial render, 100 items | `49.05 us` | `249.05 KB` | Hostless keyed-list initial render. |
| Runtime | Reverse rerender, 100 items | `34.28 us` | `146.51 KB` | Retained keyed reorder. |
| Runtime | Middle insert rerender, 100 items | `35.14 us` | `149.30 KB` | Retained keyed insert. |
| Runtime | Initial render, 1000 items | `625.37 us` | `2478.34 KB` | Hostless keyed-list initial render. |
| Runtime | Reverse rerender, 1000 items | `436.43 us` | `1467.80 KB` | Retained keyed reorder. |
| Runtime | Middle insert rerender, 1000 items | `434.58 us` | `1484.66 KB` | Retained keyed insert. |
| Tooling | Tag completion | `30.75 ms` | `2531.20 KB` | Small synthetic workspace after markup-first completion optimization. |
| Tooling | Attribute completion | `29.84 ms` | `2532.48 KB` | Same workspace after optimization. |
| Tooling | Diagnostics | `164.92 us` | `16.96 KB` | Small synthetic workspace. |
| Tooling | Formatting | `483.7 ns` | `3.1 KB` | Mixed C#/markup document. |

## What Was Optimized

### Attached-property resolution

Attached-property owner resolution was the first clearly wasteful metadata path.

The optimization was deliberately narrow:

- attached properties are now indexed by property name
- the resolver uses explicit loops instead of repeated LINQ filtering/grouping
- alias, visibility, and ambiguity behavior remain unchanged

### Child-component store reuse

Large repeated trees contain many leaf components whose child-component stores were previously allocating empty dictionaries and sets on every rerender.

`ChildComponentStore` now lazily allocates per-render collections and reuses those collections across render passes, which reduced allocation pressure in large keyed rerenders.

### Markup-first tooling completion

The clearest tooling hot spot was completion. Plain markup tag and attribute-name contexts now short-circuit the heavier C# projection/workspace path.

Measured effect in the synthetic tooling workspace:

- tag completion improved from `90.32 ms` to `30.75 ms`
- attribute completion improved from `66.94 ms` to `29.84 ms`

## WinUI Projection and Editing

The WinUI smoke path measures retained patch versus replacement for supported built-in input controls.

Representative retained versus replacement results:

| Scenario | Mean | Notes |
| --- | ---: | --- |
| retained `TextBox` patch | about `98 us/op` | same root and same `TextBox` instance retained |
| `TextBox` replacement | about `320 us/op` | same root retained, editor replaced |

That is the expected shape for the current runtime:

- retained patching is materially cheaper than replacement
- replacement is more likely to disrupt user-visible editing state

Focus-sensitive WinUI smoke scenarios are environment-dependent. The harness records explicit `unavailable` results when the machine cannot establish programmatic focus for the projected control.

## Intended V1 Scale Envelope

CSXAML v1 is intended to be comfortable for:

- ordinary settings screens
- editor/detail panes
- dashboards and forms
- nested component trees in the small-to-medium range
- repeated-child surfaces where item counts are in the low hundreds
- list/detail flows with retained controls

The measured stress envelope includes:

- 1000-item `foreach` trees
- repeated keyed reorders at 1000 items
- large hostless rerender passes used to understand reconciliation cost

These scenarios are useful for bounding behavior. They are not the recommended everyday authoring target for large enterprise list surfaces.

## Outside the V1 Scale Story

CSXAML v1 is not a replacement for WinUI virtualization primitives.

For large scrolling data surfaces, authors should still expect to use native virtualization-aware controls and future dedicated interop/design work rather than plain `foreach`.

This milestone does not claim parity with:

- `ListView`
- `ItemsRepeater`
- `DataTemplate`-driven virtualization
- `INotifyCollectionChanged`-optimized native large-list behavior

## Known Limits

- the recorded baselines are `ShortRun` release baselines, not CI trend data
- no CI trend or performance regression gate exists yet
- projected WinUI benchmarks may be environment-sensitive because WinUI activation is not equally reliable on every machine
- external-control discovery relies on referenced-assembly reflection and still needs larger reference-graph measurement
- tooling workspace loading remains an important editor cost even after the markup-first completion optimization
- very large third-party control surfaces are not proven

## Bottom Line

Milestone 14 closes with a measured, documented, and more efficient v1 performance story:

- the repo now has repeatable benchmark and WinUI smoke entry points
- the first real hot spots were optimized without weakening the spec
- the intended v1 scale is documented directly instead of being implied

That is the line CSXAML v1 should hold: honest scale, explicit tradeoffs, and predictable behavior first.
