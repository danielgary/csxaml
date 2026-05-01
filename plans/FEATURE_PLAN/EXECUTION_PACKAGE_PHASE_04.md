# Execution Package Phase 04: Content-Property Metadata Discovery

## Purpose

Teach metadata generation and runtime interop how external controls accept
child content, especially controls with `[ContentProperty]` attributes that do
not use a property named `Content`.

This phase should improve metadata and runtime behavior before introducing new
property-content syntax.

## User-Facing Target

An external control like:

```csharp
[ContentProperty(Name = "Example")]
public sealed class ControlExample : Control
{
    public UIElement? Example { get; set; }
    public UIElement? Options { get; set; }
}
```

should allow default CSXAML child content to bind to `Example` once metadata
describes that shape:

```csxaml
<Gallery:ControlExample>
    <Button Content="Run" />
</Gallery:ControlExample>
```

## Non-Goals

- Do not add `<Control.Property>` syntax in this phase.
- Do not support templates or `DataTemplate` authoring here.
- Do not make arbitrary object graphs work through reflection-only heuristics.
- Do not remove existing `Content`, `Child`, `Children`, `Panel`, or
  `ContentControl` behavior.

## Files To Review First

- `Csxaml.ControlMetadata/Model/ControlMetadata.cs`
- `Csxaml.ControlMetadata/Model/ControlChildKind.cs`
- `Csxaml.ControlMetadata/Model/PropertyMetadata.cs`
- `Csxaml.Generator/Semantics/ExternalControlChildKindResolver.cs`
- `Csxaml.Generator/Semantics/ExternalControlMetadataBuilder.cs`
- `Csxaml.Runtime/Adapters/ExternalChildSetter.cs`
- `Csxaml.Runtime/Adapters/ExternalControlAdapter.cs`
- `Csxaml.ExternalControls/StatusButton.cs`
- external-control tests in generator and runtime projects

## Design Requirements

The existing `ControlChildKind` enum is too small for external controls. Add a
content model that can express:

- no child content
- single child property
- collection child property
- default content property name
- CLR type of the content property
- collection item type when known
- whether the source came from `[ContentProperty]`, base type convention, or
  known built-in metadata

Keep the model understandable. Prefer a small record over many unrelated flags.

Example:

```csharp
public sealed record ControlContentMetadata(
    string? DefaultPropertyName,
    ControlContentKind Kind,
    string? PropertyTypeName,
    string? ItemTypeName);
```

Only add this if it genuinely improves clarity over extending existing records.

## Implementation Steps

### 1. Extend Metadata Model

- Add a content metadata record or extend `ControlMetadata`.
- Keep compatibility for existing generated metadata.
- Update `GeneratedControlMetadata` emission.
- Update registries and tests.

### 2. Discover `[ContentProperty]`

- Load `Microsoft.UI.Xaml.Markup.ContentPropertyAttribute` by name to avoid
  unnecessary direct coupling if needed.
- Check inherited attributes.
- Extract the named property.
- Validate that the property is public and instance-level.
- Determine if the property accepts:
  - one `UIElement`
  - one object
  - a collection of `UIElement`
  - a known item collection used by WinUI controls
- Prefer explicit content attribute metadata over name-based conventions.

### 3. Preserve Existing Conventions

Continue supporting:

- `Panel.Children`
- `Children` returning `UIElementCollection`
- `ContentControl.Content`
- `Border.Child`
- `ScrollViewer.Content`
- public `Child`
- public `Content`

When conventions and `[ContentProperty]` disagree, prefer the attribute and add
tests proving the precedence.

### 4. Runtime External Child Setter

- Replace `ExternalChildSetter` assumptions about `Child` and `Content` with
  metadata-driven property lookup.
- Keep collection patching for `UIElementCollection`.
- Support setting a single child property by name.
- Clear a single child property when no child content is present.
- Preserve useful runtime errors for missing or incompatible properties.

### 5. Validation

- Native/external validation should reject child content when metadata says
  none.
- Validation should enforce single versus multiple child count.
- Diagnostics should mention the content property name when helpful.

### 6. Fixture Controls

Add external controls that cover:

- `[ContentProperty(Name = "Example")]` single UIElement
- inherited content property
- `Content` fallback
- `Child` fallback
- `Children` collection
- unsupported content property type

Keep fixture files small and explicit.

### 7. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `LANGUAGE-SPEC.md`: external control content metadata behavior.
- `ROADMAP.md`: checklist and notes log.
- `docs/supported-feature-matrix.md`.
- `docs-site/articles/guides/external-control-interop.md`: supported content
  property shapes and boundaries.
- `docs-site/articles/language/external-controls.md`.

## Validation Checks

```powershell
dotnet test .\Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj --no-restore -m:1
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

- metadata records the actual external content property.
- runtime sets external child content by metadata, not just by `Content` or
  `Child`.
- unsupported content shapes fail clearly.
- existing controls still render correctly.
- docs and status surfaces accurately describe the supported external-control
  content shape.

