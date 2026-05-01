# Execution Package Phase 06: Generated Page And Window Root Foundation

## Purpose

Add the root-kind foundation and generated `Page` / `Window` support. This
phase removes `MainWindow.xaml` for generated windows, but does not yet own the
application entry point. Generated application mode is completed in phase 07.

## User-Facing Target

```csxaml
component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;

    render <HomePage />;
}

component Page HomePage {
    render <Grid>
        <TextBlock Text="Home" />
    </Grid>;
}
```

## Non-Goals

- Do not generate the app entry point in this phase.
- Do not remove `App.xaml` or `App.xaml.cs` yet.
- Do not implement generated `Application` or generated
  `ResourceDictionary` roots here.
- Do not make `Window` or `Page` inherit from `ComponentInstance`.

## Files To Review First

- `Csxaml.Generator/Ast/ComponentDefinition.cs`
- `Csxaml.Generator/Parsing/Parser.cs`
- `Csxaml.Generator/Validation/ComponentDefinitionValidator.cs`
- `Csxaml.Generator/Emission/ComponentEmitter.cs`
- `Csxaml.Generator/Emission/CodeEmitter.cs`
- `Csxaml.Generator/Cli/GeneratorRunner.cs`
- `Csxaml.Runtime/Hosting/CsxamlHost.cs`
- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Runtime/Rendering/WinUiNodeRenderer.cs`
- `samples/Csxaml.ExistingWinUI/MainWindow.xaml`
- `samples/Csxaml.ExistingWinUI/MainWindow.xaml.cs`
- generator, project-system, and runtime host tests

## Design Requirements

Add `ComponentKind` to the AST:

```csharp
internal enum ComponentKind
{
    Element,
    Page,
    Window,
    Application,
    ResourceDictionary
}
```

For this phase, fully implement:

- `Element`
- `Page`
- `Window`

Parse but reject or defer:

- `Application`
- `ResourceDictionary`

Generated shape:

- `component Page Name` generates a public `Name : Page`.
- `component Window Name` generates a public `Name : Window`.
- Each generated root owns a private retained body component.
- The body component is generated similarly to existing element components.
- The root type mounts the body into `Page.Content` or `Window.Content`.

## Implementation Steps

### 1. Extend Parser And AST

- Parse `component Element`, `component Page`, and `component Window`.
- Store `ComponentKind`.
- Keep existing `component Element` behavior unchanged.
- Add diagnostics for unsupported root kinds until phase 07.
- Add parser tests for all root kinds.

### 2. Validate Root Kinds

- Add `RootKindValidator` or similarly focused validator.
- Ensure `Page` and `Window` have no props in first slice unless deliberately
  supported.
- Support common root property declarations only if the syntax is specified.
- Ensure visual roots still have exactly one `render <Root />;`.
- Reject slots in root kinds unless the spec says otherwise.

### 3. Runtime Content Host

- Add a runtime content host that can mount into `Window.Content` or
  `Page.Content`.
- Reuse `ComponentTreeCoordinator`.
- Match `CsxamlHost` disposal behavior.
- Dispose retained root body when the generated window closes or page unloads.
- Keep the host type small and documented.

### 4. Emit Private Body Components

- Split `ComponentEmitter` if needed:
  - `ElementComponentEmitter`
  - shared body emitter
  - `WindowRootEmitter`
  - `PageRootEmitter`
- Generate deterministic private body type names.
- Keep generated files under existing output directories.
- Preserve source maps.

### 5. Emit Public Root Types

- Generate `Window` and `Page` subclasses in the source namespace.
- Wire constructors to mount the private body.
- Support `IServiceProvider` handoff from constructor parameters if needed for
  existing DI root activation.
- Keep generated constructors boring and readable.

### 6. Project-System Integration

- Ensure generated root files are included in `Compile`.
- Ensure clean removes generated root files.
- Ensure generated root names appear in manifests if other projects need to
  reference pages.

### 7. Sample Proof

- Add a small generated-window fixture project or update a sample branch-style
  fixture.
- Keep the existing starter sample hybrid until phase 07 if necessary.
- Prove handwritten `App.xaml.cs` can instantiate generated `MainWindow`.

### 8. Tooling

- TextMate grammar recognizes `component Page` and `component Window`.
- Snippets include page and window roots.
- Completion/hover understands root kinds.
- Diagnostics understand root-kind constraints.
- Formatting handles root files.

### 9. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `LANGUAGE-SPEC.md`: component kinds and root semantics.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `docs-site/articles/getting-started/create-new-app.md`: generated
  window/page preview or planned status.
- `docs-site/articles/language/component-model.md`.
- `docs-site/articles/api/runtime.md` if content host is public.
- sample README for any fixture/sample added.

## Validation Checks

```powershell
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

If Visual Studio packaging or file classification changes:

```powershell
dotnet test .\Csxaml.VisualStudio.Tests\Csxaml.VisualStudio.Tests.csproj --no-restore -m:1
```

Docs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Full verification:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1 -nr:false
```

## Completion Criteria

This phase is complete only when:

- `component Page` and `component Window` compile and instantiate.
- generated roots host retained CSXAML bodies.
- disposal behavior is tested.
- handwritten app startup can launch a generated window.
- tooling and docs describe the preview/current status accurately.

