# Performance And Scale

This page is the Milestone 14 home for performance posture, benchmark commands, and the measured v1 scale envelope.

`Csxaml.Benchmarks` now provides the repeatable benchmark entry point. The numbers below come from BenchmarkDotNet `ShortRun` baselines captured on 2026-04-23. They are directional release baselines, not long-horizon CI trend data.

## Current Audit Environment

Captured during the benchmark and documentation pass on 2026-04-23:

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

## Benchmark Purpose

The benchmark suite should answer four questions:

- how generation time grows as a solution gains more `.csxaml` files
- whether metadata lookup and discovery stay cheap enough for build and tooling paths
- whether retained runtime reconciliation handles realistic rerender pressure
- whether editor services remain responsive for small and larger workspaces

Benchmarks should be repeatable, outside ordinary test runs, and explicit about the environment used to collect results.

## Commands

Build and list benchmarks:

```powershell
dotnet build .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -c Release
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --list flat
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *
```

Recommended filtered runs once categories exist:

```powershell
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Generator*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Metadata*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Runtime*
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter *Tooling*
```

Use release builds for benchmark runs. Keep normal correctness verification separate:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1
```

For a quick smoke run without treating the number as a stable baseline:

```powershell
dotnet run --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -c Release -- --filter *MetadataBenchmarks.BuiltInControlLookup* --job Dry --warmupCount 1 --iterationCount 1
```

## Scenario Categories

Generator scenarios currently measure:

- 1 component
- 25 components
- 100 components
- 500 components
- components using helper code, state, `if`, `foreach`, and slots

Future generator scenarios should add external-control imports and generated file writing/stale-output pruning.

Metadata scenarios currently measure:

- built-in control metadata lookup
- attached-property metadata lookup

Future metadata scenarios should add referenced component manifest discovery, external control metadata discovery from referenced assemblies, and repeated lookup paths used by validation and completion.

Runtime scenarios currently measure:

- 1,000-item keyed list initial render
- keyed reorder
- middle insert rerender

Future runtime scenarios should add start/end insert/remove cases, nested component trees with many props, controlled input rerender while siblings change, and repeated rerender of a Todo-style editor flow.

Tooling scenarios currently measure:

- tag completion in a small workspace
- attribute completion for built-in controls
- diagnostics over a small workspace
- formatting over mixed C#/markup documents

Future tooling scenarios should add large synthetic workspaces, component-tag attributes, imported external controls, diagnostics over larger documents, and definition lookup for workspace components.

## Results Summary

Generator baselines came from:

```powershell
dotnet run -c Release --project .\Csxaml.Benchmarks\Csxaml.Benchmarks.csproj -- --filter * --job short
```

Tooling baselines were rerun after one measured optimization to capture the new completion path:

- markup tag/attribute completion now skips the C# projection/workspace path when the cursor is already in a plain markup tag or attribute-name context
- benchmark effect in the synthetic tooling workspace:
  - tag completion improved from `90.32 ms` to `30.75 ms`
  - attribute completion improved from `66.94 ms` to `29.84 ms`

| Category | Scenario | Mean | Allocated | Notes |
| --- | --- | --- | --- | --- |
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
| Tooling | Tag completion | `30.75 ms` | `2531.20 KB` | Small synthetic workspace, after markup-first completion optimization. |
| Tooling | Attribute completion | `29.84 ms` | `2532.48 KB` | Same workspace, after optimization. |
| Tooling | Diagnostics | `164.92 us` | `16.96 KB` | Small synthetic workspace. |
| Tooling | Formatting | `483.7 ns` | `3.1 KB` | Mixed C#/markup document. |

## Intended V1 Scale Envelope

With the current benchmark slice, the measured v1 envelope is:

- ordinary componentized WinUI screens should be supported
- solutions with hundreds of `.csxaml` components remain buildable and authorable
- ordinary retained rerender paths handle about 1,000 visible logical nodes comfortably in the hostless logical-tree benchmark
- keyed list reorder, insert, and remove behavior should stay predictable at that scale
- controlled `TextBox` and `CheckBox` flows remain a supported runtime goal, though the current benchmark slice does not yet model IME/focus-heavy editor scenarios

This is not a promise of native item virtualization. `foreach` creates repeated child nodes; it is not a replacement for `ListView`, `ItemsRepeater`, or another virtualization-aware WinUI control.

## Known Performance Limits

Current limits and unknowns:

- the recorded baselines are `ShortRun` release baselines, not CI trend data
- no CI trend or performance regression gate exists yet
- projected WinUI benchmarks may be environment-sensitive because WinUI activation is not equally reliable on every machine
- external-control discovery relies on referenced-assembly reflection and still needs larger reference-graph measurement
- tooling workspace loading is still the dominant editor cost in the current benchmark slice even after the markup-first completion optimization
- retained native reconciliation is covered by tests, and the current hostless benchmark looks healthy, but WinUI-projected large-churn measurement is still absent
- very large third-party control surfaces are not proven

## Deferred Work

These are not required for the first benchmark baseline, but they are likely future work:

- CI-friendly benchmark smoke runs or trend capture
- explicit "10x slower than baseline" guardrails for selected scenarios
- incremental tooling workspace refresh instead of repeated full reloads where measured
- broader caching for metadata and external-control discovery if benchmarks justify it
- virtualization-aware component patterns or native item-control interop guidance
- projected WinUI benchmark lanes for focus, caret, and native adapter behavior

Update this page after each meaningful benchmark or optimization pass so readers do not have to infer performance posture from source code.
