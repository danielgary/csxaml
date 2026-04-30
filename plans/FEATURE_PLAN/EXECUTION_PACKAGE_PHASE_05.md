# Execution Package Phase 05: Property-Content Syntax And Named Slots

## Purpose

Add one coherent syntax for native property content and component named slots:

```csxaml
<Button>
    <Button.Flyout>
        <Flyout />
    </Button.Flyout>
</Button>
```

```csxaml
<ControlExample>
    <ControlExample.Example>
        <Button Content="Run" />
    </ControlExample.Example>
</ControlExample>
```

The syntax should feel familiar to XAML authors and remain explicit enough for
CSXAML parsing, validation, emission, runtime, and tooling.

## Non-Goals

- Do not implement template authoring in this phase.
- Do not allow property-content nodes as render roots.
- Do not support fallback slot content unless explicitly designed.
- Do not allow slot outlets inside `foreach`.
- Do not silently merge attribute and property-content values.

## Files To Review First

- `Csxaml.Generator/Ast/ChildNode.cs`
- `Csxaml.Generator/Ast/MarkupNode.cs`
- `Csxaml.Generator/Ast/SlotOutletNode.cs`
- `Csxaml.Generator/Parsing/ChildNodeParser.cs`
- `Csxaml.Generator/Parsing/ParserContext.cs`
- `Csxaml.Generator/Validation/SlotDefinitionValidator.cs`
- `Csxaml.Generator/Validation/NativeElementValidator.cs`
- `Csxaml.Generator/Validation/ComponentTagValidator.cs`
- `Csxaml.Generator/Emission/ChildNodeEmitter.cs`
- `Csxaml.Runtime/Nodes/NativeElementNode.cs`
- `Csxaml.Runtime/Nodes/ComponentNode.cs`
- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Tooling.Core/Common/Markup`
- parser, validation, emission, runtime slot tests

## Source Rules

Implement these rules unless the spec is updated first:

- A child element named `ParentTag.Property` directly under `ParentTag` is a
  property-content element.
- The owner part must match the containing element source tag name or resolved
  component tag name.
- Property-content elements are not native elements themselves.
- Property-content elements cannot be the root render node.
- Property-content elements cannot have `Key`, events, attached properties, or
  `Ref`.
- Attribute assignment and property-content assignment to the same property is
  a diagnostic unless metadata explicitly defines merge behavior.
- Single-value property content accepts at most one child.
- Collection property content accepts multiple children.
- Default slot content remains ordinary child content.
- Named component slot content uses the same property-content syntax.
- Named slot outlets use `<Slot Name="..."/>` inside the component definition.

## Implementation Steps

### 1. Update Spec First

- Add grammar for property-content elements.
- Define owner matching.
- Define invalid placements.
- Define collision with attributes.
- Define named slot declaration and call-site transport.
- Update the existing named-slot rejection wording.

### 2. Add AST Types

- Add `PropertyContentNode` or equivalent.
- Store owner name, property name, child nodes, and spans.
- Keep this separate from `MarkupNode` so emission and validation do not treat
  it as a real element.

### 3. Parser Changes

- Teach `ChildNodeParser` to recognize dotted child element tags.
- Only parse them in child positions.
- Preserve good recovery for malformed closing tags.
- Add diagnostics for root-level property content.
- Add parser tests for nested property content.

### 4. Native Validation

- Resolve property-content names against metadata.
- Use content metadata from phase 04 where relevant.
- Validate single versus collection property content.
- Reject unknown property content.
- Reject owner mismatch.
- Reject duplicate assignment with attributes.
- Reject invalid attributes on property-content nodes.

### 5. Component Named Slot Validation

- Allow `<Slot Name="Example" />` in component definitions.
- Preserve default slot behavior for bare `<Slot />`.
- Reject duplicate named slot declarations.
- Reject named slot outlets inside `foreach`.
- Validate call-site named slots against component metadata.
- Reject unknown named slot content.
- Reject duplicate single named slot content if the slot is single.

### 6. Metadata And Manifest Updates

- Extend `ComponentMetadata` to include named slot definitions.
- Update generated component manifests.
- Update referenced-component catalog loading.
- Keep default slot compatibility unchanged.

### 7. Emission

- Emit native property content separately from normal children.
- Emit component named slot content separately from default child content.
- Keep generated code readable.
- Preserve source maps for property-content children.
- Ensure generated arrays/lists are deterministic.

### 8. Runtime

- Extend native nodes to carry property content.
- Update adapters or generic property application to set object/collection
  property content.
- Extend component node transport for named slot content.
- Update reconciliation to preserve identity within named slot content.
- Keep slot disposal rules clear and tested.

### 9. Tooling

- Scanner should identify property-content tags.
- Completion after `<Button.` should suggest supported property-content names.
- Completion inside component usages should suggest named slots.
- Hover should explain property-content target and type.
- Formatting should indent property-content blocks predictably.
- Semantic tokens should distinguish owner and property portions.
- Go-to-definition for component named slots should navigate to the slot outlet
  if feasible.

### 10. Documentation Updates

Update:

- `FEATURE_PLAN.md` status table.
- `LANGUAGE-SPEC.md`.
- `ROADMAP.md`.
- `docs/supported-feature-matrix.md`.
- `docs-site/articles/language/syntax.md`.
- `docs-site/articles/language/component-model.md`.
- `docs-site/articles/language/native-controls.md`.
- `docs-site/articles/guides/external-control-interop.md`.
- `docs-site/articles/guides/native-props-and-events.md`.
- VS Code snippets.

## Validation Checks

```powershell
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
dotnet test .\Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj --no-restore -m:1
```

VS Code grammar and snippets if changed:

```powershell
Push-Location VSCodeExtension
npm test
Pop-Location
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

- property-content syntax parses, validates, emits, and renders.
- named slots work across local and referenced components.
- default slots remain compatible.
- invalid property-content and named-slot usage produces precise diagnostics.
- tooling supports completion, hover, formatting, and semantic tokens.
- docs and status surfaces agree on the supported shape.

