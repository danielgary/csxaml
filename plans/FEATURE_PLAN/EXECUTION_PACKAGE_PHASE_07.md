# Execution Package Phase 07: Full Generated Application Mode

## Purpose

Complete the generated app-shell experience. A starter app should build and run
with:

- no `App.xaml`
- no `App.xaml.cs`
- no `MainWindow.xaml`
- no `MainWindow.xaml.cs`

CSXAML owns the source-level app shell through:

- `component Application`
- `component Window`
- `component Page`
- `component ResourceDictionary`
- generated WinUI entry point

## User-Facing Target

```csxaml
component Application App {
    startup MainWindow;
    resources AppResources;

    IServiceProvider ConfigureServices() {
        var services = new ServiceCollection();
        services.AddSingleton<ITodoService, InMemoryTodoService>();
        return services.BuildServiceProvider();
    }
}
```

```csxaml
component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;

    render <HomePage />;
}
```

```csxaml
component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>;
}
```

Project file:

```xml
<CsxamlApplicationMode>Generated</CsxamlApplicationMode>
```

## Non-Goals

- Do not support every WinUI activation mode.
- Do not generate package manifests.
- Do not implement full template/resource authoring.
- Do not hide conflicts with existing XAML roots.
- Do not make generated mode the only way to use CSXAML.

## Files To Review First

- Phase 06 generated root files.
- `build/Csxaml.props`
- `build/Csxaml.targets`
- `Csxaml.Generator/Cli/GeneratorOptions.cs`
- `Csxaml.Generator/Cli/GeneratorOptionsParser.cs`
- `Csxaml.Generator/Cli/GeneratorRunner.cs`
- `Csxaml.Generator/Emission`
- `Csxaml.Generator/IO/OutputWriter.cs`
- current WinUI template generated `App.g.i.cs` from a sample build
- `samples/Csxaml.ExistingWinUI`
- `templates/`
- project-system tests

## Design Requirements

Generated mode:

- Requires exactly one `component Application`.
- Generates an `Application` subclass.
- Generates the WinUI startup entry point that `App.xaml` normally provides.
- Forbids `ApplicationDefinition` items such as `App.xaml`.
- Creates and activates the configured startup window.
- Hooks generated or referenced app resources.
- Creates services through user C# helper code if present.
- Passes services to generated windows, pages, and components.
- Cleans up generated outputs on clean.

Hybrid mode:

- Does not generate an entry point.
- Allows existing `App.xaml` / `App.xaml.cs`.
- Allows generated pages and windows.
- Produces a diagnostic if `component Application` exists without generated
  mode, unless the spec allows application roots as inert types.

## Implementation Steps

### 1. Specify Generated App Mode

- Update the spec before code.
- Define `CsxamlApplicationMode` values:
  - `Hybrid`
  - `Generated`
- Define default behavior:
  - package/new starter may default to `Generated`
  - existing projects should remain `Hybrid` unless opted in
- Define conflict diagnostics.

### 2. MSBuild Inputs

- Add `CsxamlApplicationMode` defaulting.
- Detect `@(ApplicationDefinition)` or `App.xaml` in generated mode.
- Pass application mode to the generator through project context or command
  line.
- Include relevant MSBuild properties in generator inputs so incremental builds
  rerun when mode changes.
- Add generated entry point files to `Compile`.
- Add generated files to `FileWrites`.

### 3. Generator Options

- Extend `GeneratorOptions`.
- Parse application mode.
- Include mode in project generation context.
- Add diagnostics for invalid mode values.

### 4. Parse And Validate `component Application`

- Implement application root parsing.
- Support:
  - `startup MainWindow;`
  - `resources AppResources;`
  - ordinary helper code such as `ConfigureServices`
- Validate startup type exists and is a generated or referenced window.
- Validate resources type exists and is a generated or referenced dictionary.
- Enforce one application root in generated mode.
- Produce source-facing diagnostics for missing or duplicate declarations.

### 5. Generate `Application`

- Emit `public partial class App : Application`.
- Emit `OnLaunched` or the current Windows App SDK equivalent.
- Call `ConfigureServices` if present.
- Create the startup window.
- Pass services into the startup window.
- Activate the window.
- Retain the window field to avoid premature collection.
- Hook resources before window creation if required.

### 6. Generate Entry Point

- Add `ApplicationEntryPointEmitter`.
- Generate only in `Generated` mode.
- Match current WinUI startup requirements.
- Keep generated file small and isolated.
- Add tests comparing shape to a current WinUI template or compile fixture.

### 7. Generate `ResourceDictionary`

- Implement basic `component ResourceDictionary`.
- Support merged dictionaries first.
- Support keyed simple resources only if key syntax is specified and tested.
- Allow generated `Application` to instantiate and assign resources.
- Keep deep template authoring deferred.

### 8. Starter Template

- Add or update a starter template that contains:
  - `App.csxaml`
  - `MainWindow.csxaml`
  - `AppResources.csxaml`
  - optional `Pages/HomePage.csxaml`
  - no `App.xaml`
  - no `App.xaml.cs`
  - no `MainWindow.xaml`
  - no `MainWindow.xaml.cs`
- Set `CsxamlApplicationMode=Generated`.
- Keep a hybrid template option if useful for migration.

### 9. Tests

Add project-system tests for:

- generated mode without XAML roots builds
- generated mode with `App.xaml` fails clearly
- generated mode with no application root fails clearly
- generated mode with two application roots fails clearly
- hybrid mode with application root fails with guidance or behaves as
  specified
- clean removes generated entry point and root files
- package-consumer generated starter builds

Add runtime/smoke tests for:

- generated app creates and activates startup window where environment allows
- services reach generated root body
- resources are available to generated root content

### 10. Tooling

- Completion and hover for `startup` and `resources`.
- Diagnostics for application root constraints.
- Formatting for application root files.
- Snippets for generated app, window, page, and dictionary roots.
- VS Code grammar and semantic token updates.

### 11. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `LANGUAGE-SPEC.md`.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `README.md`: starter app shape.
- `docs-site/articles/getting-started/create-new-app.md`.
- `docs-site/articles/getting-started/quick-start.md`.
- `docs-site/articles/getting-started/prerequisites.md`.
- `docs-site/articles/language/syntax.md`.
- `docs-site/articles/language/component-model.md`.
- `docs-site/articles/guides/package-installation.md`.
- `docs-site/articles/guides/resources-and-templates.md` if created.
- template README files.
- release/packaging docs if package behavior changes.

## Validation Checks

Focused:

```powershell
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

Template:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-StarterTemplate.ps1
```

Docs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Full:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1 -nr:false
```

## Completion Criteria

This phase is complete only when:

- generated app mode builds a starter without default WinUI shell files.
- generated mode owns the entry point.
- conflicts with `App.xaml` fail clearly.
- resources and services flow through generated startup.
- hybrid mode remains available and documented.
- docs and status surfaces present generated mode as the recommended new-app
  developer experience only when it is truly working.

