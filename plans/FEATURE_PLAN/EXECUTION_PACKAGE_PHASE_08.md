# Execution Package Phase 08: Broader Attached-Property Metadata

## Purpose

Expand attached-property metadata and runtime application to cover normal WinUI
layout, tooltip, and accessibility scenarios used by Gallery-like apps.

Initial owners:

- `Canvas`
- `RelativePanel`
- `ToolTipService`
- `VariableSizedWrapGrid`
- expanded `AutomationProperties`
- existing `Grid`

## Non-Goals

- Do not claim every WinUI attached property is supported.
- Do not create one giant attached-property applicator.
- Do not bypass ordinary `using` and type-alias owner lookup rules.
- Do not use namespace aliases as attached-property owners.

## Files To Review First

- `Csxaml.ControlMetadata/Model/AttachedPropertyMetadata.cs`
- `Csxaml.ControlMetadata/Registry/AttachedPropertyMetadataRegistry.cs`
- `Csxaml.ControlMetadata/Resolution/AttachedPropertyReferenceResolver.cs`
- `Csxaml.ControlMetadata/Generated/GeneratedAttachedPropertyMetadata.cs`
- `Csxaml.Generator/Semantics/AttachedPropertyBindingResolver.cs`
- `Csxaml.Generator/Validation/AttachedPropertyValidator.cs`
- `Csxaml.Generator/Emission/NativeAttributeEmitter.cs`
- `Csxaml.Runtime/Adapters/AttachedPropertyApplicator.cs`
- `Csxaml.Runtime/Adapters/GridAttachedPropertyApplicator.cs`
- `Csxaml.Runtime/Adapters/AutomationPropertiesAttachedPropertyApplicator.cs`
- `Csxaml.Runtime/Adapters/NativeAttachedPropertyValueConverter.cs`
- attached-property tests in generator, runtime, and tooling projects

## Design Requirements

Keep owner-specific files:

- `CanvasAttachedPropertyApplicator`
- `RelativePanelAttachedPropertyApplicator`
- `ToolTipServiceAttachedPropertyApplicator`
- `VariableSizedWrapGridAttachedPropertyApplicator`
- expanded `AutomationPropertiesAttachedPropertyApplicator`

Clearing behavior:

- prefer `ClearValue(Owner.PropertyNameProperty)` when possible
- otherwise set known defaults explicitly
- test clearing on rerender when an attached property disappears

Metadata should include:

- owner source name
- owner CLR type
- property name
- CLR value type
- value-kind hint
- required parent tag if needed
- dependency-property field name if available

## Implementation Steps

### 1. Inventory Properties

- Review WinUI attached properties for each owner.
- Pick the practical Gallery-used subset first.
- Record any intentionally skipped properties in docs and roadmap notes.

Suggested first subset:

- `Canvas.Left`
- `Canvas.Top`
- `Canvas.ZIndex`
- `RelativePanel.AlignLeftWithPanel`
- `RelativePanel.AlignTopWithPanel`
- `RelativePanel.RightOf`
- `RelativePanel.Below`
- `ToolTipService.ToolTip`
- `VariableSizedWrapGrid.ColumnSpan`
- `VariableSizedWrapGrid.RowSpan`
- `AutomationProperties.Name`
- `AutomationProperties.AutomationId`
- `AutomationProperties.HelpText`
- `AutomationProperties.ItemStatus`
- `AutomationProperties.ItemType`
- `AutomationProperties.LabeledBy`
- other Gallery-required automation properties discovered during sample audit

### 2. Generate Or Curate Metadata

- Prefer systematic discovery of public static `GetX` / `SetX` pairs and
  `XProperty` fields.
- If discovery is too broad, keep a curated owner/property list.
- Preserve deterministic ordering.
- Add tests for metadata presence and value types.

### 3. Runtime Applicators

- Add one file per owner.
- Keep methods short.
- Add shared conversion helpers only when duplication becomes confusing.
- Clear each supported property.
- Wrap runtime failures with source info.

### 4. Validation

- Verify owner lookup still follows imports and type aliases.
- Verify namespace aliases do not become owner types.
- Verify parent restrictions where applicable.
- Verify unknown attached properties produce suggestions.

### 5. Tooling

- Completion should include new attached properties.
- Hover should show owner, property, type, and required parent if any.
- Semantic tokens should classify dotted attached-property names.
- Code actions should suggest close attached-property names.

### 6. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `LANGUAGE-SPEC.md`.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `docs-site/articles/guides/native-props-and-events.md`.
- `docs-site/articles/language/native-controls.md`.
- `docs-site/articles/guides/component-testing.md` for automation properties.
- Any Gallery/sample docs that use these properties.

## Validation Checks

```powershell
dotnet test .\Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
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

- each targeted owner has metadata, validation, runtime application, clearing,
  tooling, tests, and docs.
- existing attached-property behavior remains compatible.
- docs clearly list supported owners and do not imply full WinUI coverage.

