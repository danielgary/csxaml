# Performance and Scale

## Purpose

This document records the measured Milestone 14 performance story for CSXAML v1.

It exists to answer three questions directly:

1. what was measured
2. what was optimized
3. what scale CSXAML v1 is honestly intended to handle

The language semantics remain defined by [LANGUAGE-SPEC.md](../LANGUAGE-SPEC.md). This document is about measured implementation posture, not new language rules.

## How To Run The Measurements

From the repo root:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/perf/run-milestone-14.ps1
```

That runs the `BenchmarkDotNet` suite in [`Csxaml.Benchmarks`](../Csxaml.Benchmarks) and writes artifacts under `artifacts/benchmarks`.

For the WinUI projection/editing smoke path:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/perf/run-winui-projection-smoke.ps1
```

That writes:

- `artifacts/benchmarks/winui-smoke.json`
- `artifacts/benchmarks/winui-smoke.md`

## What Was Measured

Milestone 14 added repeatable measurement coverage for:

- generator parsing and end-to-end generation
- metadata lookup and attached-property resolution
- hostless runtime initial render and retained rerender scenarios
- WinUI retained patch versus replacement for supported built-in input controls

The benchmark scenarios are intentionally tied to the current v1 language and runtime contract:

- medium `.csxaml` source with imports, helper code, slots, and attached properties
- multi-file generation
- flat repeated-child render
- keyed single-item update
- keyed reorder
- list/detail editor rerender
- `State<T>` no-op versus `Touch()` behavior
- retained `TextBox` projection versus replacement

## Key Results

### Generator

The generator baseline showed that parsing is not the dominant cost for normal files.

Representative results from the Milestone 14 benchmark suite:

- medium-component parse: about `7.8 us`, about `35 KB`
- medium-component generation: about `101 us`, about `346 KB`
- multi-file generation: about `155 us`, about `406 KB`

That means the generator's first performance pressure is validation/emission and generated-file shaping, not raw tokenization throughput.

### Metadata

Built-in and external lookup were already cheap. Attached-property owner resolution was the first clearly wasteful metadata path.

Before optimization:

- built-in control lookup: about `2.7 ns`, `0 B`
- attached-property owner resolution: about `232.7 ns`, about `1296 B`
- external control registry lookup: about `10.6 ns`, `0 B`

After optimization:

- built-in control lookup: about `2.8 ns`, `0 B`
- attached-property owner resolution: about `26.7 ns`, about `152 B`
- external control registry lookup: about `10.7 ns`, `0 B`

The optimization was deliberately narrow:

- attached properties are now indexed by property name
- the resolver uses explicit loops instead of repeated LINQ filtering/grouping
- alias, visibility, and ambiguity behavior remain unchanged

### Hostless Runtime

Initial render remained roughly linear and stayed within the expected v1 non-virtualized range.

After the Milestone 14 runtime optimization pass:

| Scenario | Mean | Allocated |
| --- | ---: | ---: |
| initial render, 100 items | `5.603 us` | `31.4 KB` |
| initial render, 1000 items | `54.243 us` | `327.49 KB` |
| keyed one-item update, 100 items | `24.792 us` | `110.97 KB` |
| keyed reorder, 100 items | `26.168 us` | `111.6 KB` |
| list/detail update, 100 items | `4.662 us` | `28.27 KB` |
| keyed one-item update, 1000 items | `263.161 us` | `1100.04 KB` |
| keyed reorder, 1000 items | `299.712 us` | `1111.21 KB` |
| list/detail update, 1000 items | `46.030 us` | `267.34 KB` |

The main runtime optimization was also narrow and explicit:

- `ChildComponentStore` now lazily allocates its per-render collections
- those collections are reused across render passes instead of being recreated on every component render

That matters because large repeated trees contain many leaf components whose child-component stores were previously allocating empty dictionaries and sets on every rerender.

Compared with the pre-optimization baseline:

- keyed one-item update at 1000 items improved from about `313 us` / `1490 KB` to about `263 us` / `1100 KB`
- keyed reorder at 1000 items improved from about `353 us` / `1501 KB` to about `300 us` / `1111 KB`

### WinUI Projection And Editing

The WinUI smoke path measures retained patch versus replacement for supported built-in input controls.

Representative retained versus replacement results:

| Scenario | Mean | Notes |
| --- | ---: | --- |
| retained `TextBox` patch | about `98 us/op` | same root and same `TextBox` instance retained |
| `TextBox` replacement | about `320 us/op` | same root retained, editor replaced |

That is the expected shape for the current runtime:

- retained patching is materially cheaper than replacement
- replacement is also more likely to disrupt user-visible editing state

Focus-sensitive WinUI smoke scenarios are environment-dependent. The harness records explicit `unavailable` results when the machine cannot establish programmatic focus for the projected control. That does not change the runtime contract; it only limits what the perf smoke can measure automatically on a given machine.

Focus and selection correctness are still covered by the existing WinUI runtime tests on environments that support live WinUI activation.

## Intended V1 Scale

CSXAML v1 is intended to be comfortable for:

- ordinary settings screens
- editor/detail panes
- dashboards and forms
- nested component trees in the small-to-medium range
- repeated-child surfaces where item counts are in the low hundreds

CSXAML v1 can tolerate higher counts as a stress case, but that is not the same thing as promising virtualization-class behavior.

### Comfortable

- detail editors with retained controls
- list/detail flows
- repeated sidebars and boards in the 50 to 200 item range
- component composition where rerenders touch a focused editor while unrelated siblings update

### Stress Case, But Measured

- 1000-item `foreach` trees
- repeated keyed reorders at 1000 items
- large hostless rerender passes used to understand reconciliation cost

These scenarios are useful for bounding behavior. They are not the recommended everyday authoring target for large enterprise list surfaces.

### Outside The Intended V1 Scale Story

CSXAML v1 is not a replacement for WinUI virtualization primitives.

For large scrolling data surfaces, authors should still expect to need native virtualization-aware controls and future dedicated interop/design work rather than plain `foreach`.

In particular, this milestone does not claim parity with:

- `ListView`
- `ItemsRepeater`
- `DataTemplate`-driven virtualization
- `INotifyCollectionChanged`-optimized native large-list behavior

## What Was Optimized

The milestone intentionally optimized only measured hot spots.

### Landed

1. attached-property resolution
   - file(s): `Csxaml.ControlMetadata/Registry/AttachedPropertyMetadataRegistry.cs`, `Csxaml.ControlMetadata/Resolution/AttachedPropertyReferenceResolver.cs`
   - problem: repeated LINQ-based filtering/grouping and no property-name index
   - result: attached-property lookup dropped to roughly one-ninth of the previous time and reduced allocations sharply

2. child-component store reuse
   - file(s): `Csxaml.Runtime/Components/ChildComponentStore.cs`
   - problem: per-component per-render collection allocation, including on leaf components with no child components
   - result: large keyed rerenders allocate substantially less and complete noticeably faster

### Intentionally Not Optimized Further In Milestone 14

- generator parsing throughput, because it was already cheap relative to end-to-end generation
- built-in native-control registry lookup, because it was already negligible
- external-control registry lookup, because it was already negligible
- broad WinUI focus-sensitive perf automation, because the remaining gap is environment activation/focus availability rather than an obvious hot-path bug

## Remaining Limits And Honest Caveats

- WinUI perf smoke is repeatable, but focus-sensitive scenarios may report `unavailable` on machines that cannot establish programmatic focus for the projected controls.
- The hostless runtime measurements intentionally separate reconciliation cost from live WinUI projection cost. They are useful, but they are not the whole UI-thread story.
- The benchmark numbers in this document come from one machine and should be treated as comparative evidence, not universal hard guarantees.
- `foreach` remains a repeated-child construct, not a virtualization feature.

## Bottom Line

Milestone 14 closes with a measured, documented, and more efficient v1 performance story:

- the repo now has repeatable benchmark and WinUI smoke entry points
- the first real hot spots were optimized without weakening the spec
- the intended v1 scale is now documented directly instead of being implied

That is the line CSXAML v1 should hold: honest scale, explicit tradeoffs, and predictable behavior first.
