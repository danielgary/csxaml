# Execution Package Phase 01: Status Alignment And Feature Contract

## Purpose

Establish the public contract and tracking surfaces before code work starts.
This phase prevents the feature track from drifting into undocumented behavior.

The output of this phase is not runtime functionality. The output is a coherent
source-of-truth set across the language spec, roadmap, supported feature
matrix, docs, samples plan, and `FEATURE_PLAN.md` implementation table.

## Scope

This package covers the documentation and planning alignment for:

- typed event-argument projection
- element refs
- content-property metadata
- property-content syntax and named slots
- broader attached-property metadata
- generated application/window/page/resource roots
- resource and template posture
- list virtualization guidance
- CSXAML highlighting in sample presenters and fixtures

## Non-Goals

- Do not implement parser, generator, runtime, metadata, or tooling behavior in
  this phase.
- Do not mark any feature as supported, preview, or complete unless code and
  tests already exist.
- Do not remove existing v1 boundaries from the docs without replacing them
  with accurate planned or preview wording.

## Files To Review First

- `FEATURE_PLAN.md`
- `LANGUAGE-SPEC.md`
- `ROADMAP.md`
- `docs/supported-feature-matrix.md`
- `README.md`
- `docs-site/articles/language/supported-features.md`
- `docs-site/articles/language/specification.md`
- `docs-site/articles/guides/native-props-and-events.md`
- `docs-site/articles/guides/external-control-interop.md`
- `docs-site/articles/guides/performance-and-scale.md`
- `docs-site/articles/getting-started/quick-start.md`
- `docs-site/articles/getting-started/create-new-app.md`
- `samples/Csxaml.ExistingWinUI/README.md`
- `templates/`

## Required Decisions

Before editing, decide and record the following:

- Which phase labels will be used in the roadmap.
- Whether this work becomes new post-v1 milestones or a new roadmap section.
- Whether generated application mode is called `Generated`, `Full`, or another
  MSBuild value. The current plan recommends `CsxamlApplicationMode=Generated`.
- Which feature-matrix status applies before implementation. Use `Not in v1`
  or `Planned` wording in prose if the matrix does not have a planned status.
- Whether docs should describe these as post-v1 roadmap items or current
  preview experiments.

If any decision is ambiguous, choose the more conservative public wording:
planned, not supported.

## Implementation Steps

### 1. Update `FEATURE_PLAN.md`

- Confirm the implementation status table contains every phase.
- Add links from the status table to the execution package files after this
  directory exists.
- Keep all statuses as `Planned` unless code already exists.
- Add a note that phase packages are execution instructions, not commitments
  that the behavior has shipped.

### 2. Update `LANGUAGE-SPEC.md`

- Add a revision-log entry for the planning alignment.
- Add a non-normative future-work section if one does not already exist for
  post-v1 features.
- Record the intended direction for:
  - event-argument projection as metadata-defined senderless `Action<TArgs>`
  - explicit element refs instead of `x:Name` as the primary imperative handle
  - property-content syntax using `Owner.Property` child elements
  - generated root kinds using `component Application`, `component Window`,
    `component Page`, and `component ResourceDictionary`
  - generated application mode replacing `App.xaml`, `App.xaml.cs`,
    `MainWindow.xaml`, and `MainWindow.xaml.cs`
  - resource dictionaries as supported future roots, while deep template
    authoring remains a separate design
  - `foreach` remaining non-virtualized
- Make sure all wording clearly says these are planned directions unless they
  are implemented.

### 3. Update `ROADMAP.md`

- Add a new post-v1 milestone section or a new feature-track section.
- Include purpose, why it matters, representative examples, exit criteria, and
  checklist items.
- Add every phase as an unchecked item.
- Add a notes-log entry with the current date describing the planning pass.
- Do not mark any implementation item complete.

### 4. Update `docs/supported-feature-matrix.md`

- Add rows for the planned features if they are not already represented.
- Keep unsupported/planned features out of `Supported in v1`.
- Use the existing matrix vocabulary. If a new `Planned` status is desired,
  update the status legend consistently across README, roadmap, and docs.
- Make sure generated application mode is visible as a future capability, not
  implied by existing starter-template support.

### 5. Update DocFX Docs

- Update `docs-site/articles/language/supported-features.md` if it includes
  the shared matrix.
- Add or update a short roadmap-oriented docs page only if the current docs
  have a place for future features.
- Avoid duplicating long design details from `FEATURE_PLAN.md`. Public docs
  should tell users what exists today and what is planned, not expose internal
  package mechanics.

### 6. Update README And Samples Only If Needed

- If README currently implies that CSXAML will always require a XAML shell,
  add careful wording that generated app roots are planned.
- If sample READMEs mention limitations, keep them accurate.
- Do not rewrite starter samples in this phase.

## Validation Checks

Run these after edits:

```powershell
rg -n "generated application|component Application|component Window|ElementRef|property-content|SelectionChanged|virtualization|ResourceDictionary" FEATURE_PLAN.md LANGUAGE-SPEC.md ROADMAP.md docs docs-site README.md
```

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

If the docs build is too expensive for the local pass, run at least:

```powershell
dotnet tool restore
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

## Documentation Update Instructions

Every changed status surface must agree on:

- current status
- whether the feature is implemented
- whether the feature is v1, preview, experimental, or planned
- user-facing examples
- known non-goals

Use exact status vocabulary from `docs/supported-feature-matrix.md`.

## Completion Criteria

This phase is complete only when:

- `FEATURE_PLAN.md` links to all execution packages.
- `LANGUAGE-SPEC.md` records the planned direction without implying support.
- `ROADMAP.md` has a track or milestone for the work.
- `docs/supported-feature-matrix.md` has accurate status rows.
- DocFX docs build successfully.
- No public doc says a planned feature is currently supported.

