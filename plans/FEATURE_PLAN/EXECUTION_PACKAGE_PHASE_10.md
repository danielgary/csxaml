# Execution Package Phase 10: List Virtualization Guidance

## Purpose

Make it difficult for authors to mistake CSXAML `foreach` for native
virtualization.

This phase is mostly docs, samples, hovers, and benchmarks framing. Compiler
warnings should be avoided unless a reliable static signal exists.

## User-Facing Message

- Use `foreach` for small and moderate visible repeated UI.
- Use stable keys when identity matters.
- Use native virtualized controls for large scrolling item surfaces.
- Use external controls or handwritten adapters for `ItemsRepeater`, `ListView`,
  `GridView`, and template-heavy item surfaces.

## Non-Goals

- Do not implement a virtualized collection abstraction in this phase.
- Do not add noisy warnings based on arbitrary collection names or unknown
  runtime sizes.
- Do not imply 1000-item benchmark stress tests are authoring
  recommendations.

## Files To Review First

- `LANGUAGE-SPEC.md`
- `docs/performance-and-scale.md`
- `docs-site/articles/guides/performance-and-scale.md`
- `docs-site/articles/language/syntax.md`
- `docs-site/articles/language/component-model.md`
- `docs-site/articles/tutorials/todo-app.md`
- `Csxaml.Tooling.Core/Net10/Hover/CsxamlHoverService.cs`
- `Csxaml.Benchmarks/`
- runtime reconciliation benchmarks and docs

## Implementation Steps

### 1. Update Performance Docs

Add a decision table:

| Scenario | Recommended shape |
| --- | --- |
| 5 to 100 visible component rows | CSXAML `foreach` with stable keys |
| hundreds of simple retained rows | CSXAML `foreach` can be acceptable when measured |
| thousands of rows | native virtualized control |
| data-template-heavy item surface | XAML `DataTemplate` or native control interop |
| dynamic list with editing controls | measure retained identity and focus behavior |

Explain:

- retained identity
- stable keys
- why virtualization is different from reconciliation
- when to wrap native controls

### 2. Update Language Spec

- Keep `foreach` semantics clear.
- State that `foreach` creates repeated child nodes.
- State that virtualization is not implied.
- Link or refer to performance docs if appropriate.

### 3. Add Interop Example

Add a small docs example showing:

```csxaml
<VirtualizedTodoList ItemsSource={Items.Value} />
```

Explain that `VirtualizedTodoList` can be a native/external control that owns
virtualization.

### 4. Tooling Hover

- Add or update hover text for `foreach`.
- Keep hover concise.
- Link to docs if the tooling supports links.
- Do not show warnings in normal authoring.

### 5. Benchmark Framing

- Ensure benchmark docs call 1000-item tests stress bounds.
- Do not present stress bounds as recommended app design.
- If new benchmarks are added, document their intent.

### 6. Status Updates

- `FEATURE_PLAN.md` status table.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- DocFX performance guide.

## Documentation Update Instructions

Keep every list-performance document consistent:

- `LANGUAGE-SPEC.md` defines `foreach` semantics and states that it is not
  virtualization.
- `docs-site/articles/guides/performance-and-scale.md` gives practical author
  guidance and the decision table.
- `docs/performance-and-scale.md` records benchmark posture and measured
  stress cases.
- `docs/supported-feature-matrix.md` states that first-class virtualization is
  not part of the current supported surface unless implementation status
  changes.
- Tutorial and starter docs should avoid using huge `foreach` examples.
- Any native virtualization wrapper example must clearly say where the native
  control owns virtualization.

## Validation Checks

Docs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Tooling tests if hover changed:

```powershell
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

Search for stale or overpromising text:

```powershell
rg -n "virtual|virtualization|foreach|1000-item|ItemsRepeater|ListView" LANGUAGE-SPEC.md docs docs-site README.md
```

## Completion Criteria

This phase is complete only when:

- docs clearly distinguish `foreach` from virtualization.
- hover/tooling guidance is helpful and not noisy.
- benchmarks are framed as measurements, not promises.
- status surfaces agree.
