# Execution Package Phase 09: Resource And Template Guidance

## Purpose

Make the resource/template story explicit after generated dictionaries and
property-content syntax exist.

The key message:

- generated `component ResourceDictionary` roots are first-class for app-owned
  resources
- XAML dictionaries remain valid and recommended for deep WinUI template and
  theme machinery
- CSXAML should not silently invent `StaticResource`, `ThemeResource`,
  `Binding`, or `DataContext` semantics

## Non-Goals

- Do not implement `DataTemplate` or `ControlTemplate` authoring unless a
  separate design exists.
- Do not remove XAML resource dictionary guidance.
- Do not make generated dictionaries appear more capable than they are.

## Files To Review First

- `FEATURE_PLAN.md`
- `LANGUAGE-SPEC.md`
- `docs/supported-feature-matrix.md`
- `docs-site/articles/guides/native-props-and-events.md`
- `docs-site/articles/guides/external-control-interop.md`
- `docs-site/articles/guides/performance-and-scale.md`
- `docs-site/articles/language/native-controls.md`
- `docs-site/articles/getting-started/quick-start.md`
- `docs-site/articles/getting-started/create-new-app.md`
- `docs-site/toc.yml`
- generated-root and resource dictionary implementation from phase 07

## Implementation Steps

### 1. Create Resource Guide

Add:

```text
docs-site/articles/guides/resources-and-templates.md
```

Cover:

- generated CSXAML resource dictionaries
- app-owned brushes, thicknesses, styles, and simple objects
- merged XAML dictionaries
- when to keep XAML
- how generated application roots hook resources
- what is not supported
- migration from `App.xaml` resources

### 2. Update TOC

- Add the guide to `docs-site/toc.yml`.
- Place it near native props/events, external control interop, and
  performance/scale.

### 3. Update Spec

- Clarify resource propagation.
- Clarify generated `ResourceDictionary` semantics.
- Clarify template deferral.
- Clarify that plain C# expressions do not gain theme-resource invalidation.

### 4. Update Getting Started

- Generated app quick start should use `AppResources.csxaml` if generated mode
  is complete.
- Hybrid app quick start should explain when XAML dictionaries remain useful.
- Avoid making new users touch `App.xaml` in the generated-root path.

### 5. Update Native/External Interop Docs

- Explain assigning object-valued properties from resources.
- Explain property-content versus resources.
- Explain when external controls expect XAML templates or `DataContext`.

### 6. Add Examples And Docs Checks

Add examples for:

- generated dictionary with merged dictionaries
- generated app using generated resources
- XAML dictionary interop for deep template work
- unsupported template authoring with a clear alternative

Ensure examples use `csxaml` fences where appropriate.

### 7. Update Status Surfaces

- `FEATURE_PLAN.md` status table.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `README.md` if generated-root getting started changed.

## Documentation Update Instructions

This phase is documentation-heavy, so treat docs as the deliverable:

- Every example of generated resources must state whether it is supported,
  preview, experimental, or planned.
- Every XAML fallback example must explain why XAML remains the better choice
  for that scenario.
- The generated app path must not tell users to edit `App.xaml`.
- The hybrid path may mention XAML dictionaries, but it must not imply generated
  app mode requires them.
- The feature matrix and guide pages must use the same status wording.
- Any new guide must be linked from the DocFX TOC and from nearby relevant
  guide pages.

## Validation Checks

Docs build:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Search for stale wording:

```powershell
rg -n "App.xaml|ResourceDictionary|StaticResource|ThemeResource|DataTemplate|ControlTemplate|resources-and-templates" docs docs-site README.md LANGUAGE-SPEC.md FEATURE_PLAN.md
```

If samples/templates changed:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-StarterTemplate.ps1
dotnet test .\Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj --no-restore -m:1
```

## Completion Criteria

This phase is complete only when:

- docs tell a new user how resources work in generated CSXAML apps.
- docs tell existing WinUI users when to keep XAML dictionaries.
- unsupported template/resource behavior is named directly.
- status surfaces agree.
- docs build passes.
