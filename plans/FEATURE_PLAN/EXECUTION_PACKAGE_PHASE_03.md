# Execution Package Phase 03: Element References

## Purpose

Add explicit element references for focus, scrolling, animation targets, and
imperative WinUI interop.

The preferred CSXAML shape is `Ref={SomeRef}`, backed by a typed
`ElementRef<TElement>` runtime object. This should be the primary imperative
handle model instead of `x:Name`.

## User-Facing Target

```csxaml
component Element SearchPanel {
    ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();
    ElementRef<ScrollViewer> ResultsScroller = new ElementRef<ScrollViewer>();

    render <StackPanel>
        <TextBox Ref={SearchBox} />
        <ScrollViewer Ref={ResultsScroller}>
            <StackPanel />
        </ScrollViewer>
        <Button
            Content="Focus"
            OnClick={() => SearchBox.Current?.Focus(FocusState.Programmatic)} />
    </StackPanel>;
}
```

## Non-Goals

- Do not implement `x:Name` symbolic lookup in this phase.
- Do not generate partial fields from names.
- Do not support component refs until a separate design decides what they mean.
- Do not make ref assignment trigger rerender.
- Do not keep stale refs after element removal or root disposal.

## Files To Review First

- `Csxaml.Runtime/Rendering/WinUiNodeRenderer.cs`
- `Csxaml.Runtime/Rendering/RenderedChildMatcher.cs`
- `Csxaml.Runtime/Adapters/RenderedNativeElement.cs`
- `Csxaml.Runtime/Nodes/NativeElementNode.cs`
- `Csxaml.Runtime/Nodes/NativePropertyValue.cs`
- `Csxaml.Runtime/Hosting/CsxamlHost.cs`
- `Csxaml.Generator/Ast/MarkupNode.cs`
- `Csxaml.Generator/Ast/PropertyNode.cs`
- `Csxaml.Generator/Parsing/ChildNodeParser.cs`
- `Csxaml.Generator/Validation/NativeElementValidator.cs`
- `Csxaml.Generator/Emission/NativeAttributeEmitter.cs`
- `Csxaml.Generator/Emission/ChildNodeEmitter.cs`
- `Csxaml.Tooling.Core` completion, hover, diagnostics, and semantic token files
- existing renderer retention and disposal tests

## Design Requirements

Add a runtime type:

```csharp
public sealed class ElementRef<TElement>
    where TElement : class
{
    public TElement? Current { get; }
    public bool TryGet([NotNullWhen(true)] out TElement? element);
}
```

Runtime internals may use an internal setter/clearer.

Ref semantics:

- `Ref` is valid on native elements.
- `Ref` value must be a C# expression assignable to `ElementRef<T>`.
- The native element must be assignable to `T`.
- `Ref` is set after native element creation.
- `Ref` stays set across retained rerenders.
- `Ref` updates if the native element is replaced.
- `Ref` clears when the element leaves the tree.
- `Ref` clears when the root is disposed.
- `Ref` does not participate in diff identity.
- `Ref` does not cause rerender.

## Implementation Steps

### 1. Add Runtime Ref Type

- Add `Csxaml.Runtime/Refs/ElementRef.cs` or another small obvious folder.
- Keep public API tiny.
- Use internal methods for renderer assignment and clearing.
- Add XML docs because this becomes app-facing API.

### 2. Add AST Representation

- Add a dedicated ref value to `MarkupNode` or a small related type.
- Do not store `Ref` as a normal `NativePropertyValue`.
- Preserve source span for diagnostics and runtime exception context.

### 3. Parser And Validation

- Recognize `Ref` as a reserved native attribute.
- Reject string-literal `Ref="Name"`.
- Reject duplicate `Ref`.
- Reject `Ref` on component tags in the first slice.
- Reject `Ref` on property-content nodes once those exist.
- Ensure `Ref` does not collide with real native property metadata.

### 4. Emission

- Emit a `NativeElementRefValue` or equivalent dedicated runtime node value.
- Preserve source info for runtime errors.
- Keep generated code readable and deterministic.
- Add source mapping around ref expression values.

### 5. Runtime Assignment

- Update `WinUiNodeRenderer` to set refs when native elements are created or
  retained.
- Clear old refs before replacing or disposing an element.
- Avoid clearing and resetting a ref unnecessarily on ordinary retained
  rerender if the underlying element is unchanged.
- Include source-contextual runtime exceptions for type mismatches.

### 6. Testing

Add runtime tests for:

- ref assigned on initial render
- ref preserved on retained rerender
- ref changed on replacement
- ref cleared on conditional removal
- ref cleared on root disposal
- invalid `ElementRef<T>` type produces clear runtime failure
- external controls can be referenced
- no rerender happens merely because ref is assigned

Add generator tests for:

- valid `Ref={SearchBox}`
- duplicate ref
- string ref diagnostic
- component ref diagnostic

### 7. Tooling

- Completion should offer `Ref` for native elements.
- Hover should explain lifetime and show `ElementRef<TElement>`.
- Diagnostics should classify invalid ref syntax early where possible.
- Semantic tokens should classify `Ref` as a framework/reserved attribute or
  native-like attribute consistently.
- C# projection should include ref expressions so C# diagnostics work.

### 8. Documentation Updates

Update:

- `FEATURE_PLAN.md` implementation status table.
- `LANGUAGE-SPEC.md`: imperative interop and source grammar.
- `ROADMAP.md`: phase checklist and notes log.
- `docs/supported-feature-matrix.md`: status.
- `docs-site/articles/guides/native-props-and-events.md` or a new interop
  guide section.
- `docs-site/articles/language/lifecycle.md`: ref clearing and disposal.
- `docs-site/articles/guides/component-testing.md`: testing implications.
- `README.md` if the getting-started story now mentions refs.

## Validation Checks

```powershell
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

If public runtime API changed:

```powershell
dotnet build .\Csxaml.Runtime\Csxaml.Runtime.csproj --no-restore
dotnet build .\Csxaml.Testing\Csxaml.Testing.csproj --no-restore
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

- `ElementRef<T>` is public and documented.
- `Ref={...}` works on built-in and external native controls.
- refs clear correctly on removal and disposal.
- invalid ref usage produces source-facing diagnostics or contextual runtime
  failures.
- tooling helps authors discover and use refs.
- spec, roadmap, feature matrix, docs, samples, and implementation table agree.

