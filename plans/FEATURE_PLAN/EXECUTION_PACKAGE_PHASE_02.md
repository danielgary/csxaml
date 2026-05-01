# Execution Package Phase 02: Typed Event-Argument Projection

## Purpose

Add typed event-argument projection for common WinUI events while preserving the
existing CSXAML event feel:

- event attributes use normalized `OnEventName`
- handlers are ordinary C# delegates or lambdas
- sender is omitted
- event args are strongly typed
- value-normalized events remain explicit special cases

## User-Facing Target

```csxaml
<ListView OnSelectionChanged={args => Select(args.AddedItems)} />
<Slider OnValueChanged={args => Volume.Value = args.NewValue} />
<AutoSuggestBox OnQuerySubmitted={args => Search(args.QueryText)} />
<TextBox OnKeyDown={args => SubmitOnEnter(args)} />
<Border OnPointerPressed={args => StartDrag(args)} />
<Frame OnNavigated={args => TrackPage(args.SourcePageType)} />
```

## Non-Goals

- Do not convert all events into value-normalized delegates.
- Do not expose `(sender, args)` lambdas as the preferred CSXAML shape.
- Do not infer event projections solely by event name.
- Do not add broad reflection-based event binding to hot runtime paths.
- Do not break `OnClick`, `OnTextChanged`, or `OnCheckedChanged`.

## Files To Review First

- `Csxaml.ControlMetadata/Model/EventMetadata.cs`
- `Csxaml.ControlMetadata/Model/EventBindingKind.cs`
- `Csxaml.ControlMetadata.Generator/Filtering/CuratedControlSet.cs`
- `Csxaml.ControlMetadata.Generator/Filtering/CuratedEventDefinition.cs`
- `Csxaml.ControlMetadata.Generator/Emission/MetadataSourceEmitter.cs`
- `Csxaml.ControlMetadata/Generated/GeneratedControlMetadata.cs`
- `Csxaml.Generator/Emission/NativeAttributeEmitter.cs`
- `Csxaml.Runtime/Nodes/NativeEventValue.cs`
- `Csxaml.Runtime/Adapters/NativeEventBindingStore.cs`
- `Csxaml.Runtime/Adapters/ExternalEventBinder.cs`
- `Csxaml.Runtime/Adapters/ButtonControlAdapter.cs`
- `Csxaml.Runtime/Adapters/TextBoxControlAdapter.cs`
- `Csxaml.Runtime/Adapters/CheckBoxControlAdapter.cs`
- `Csxaml.Tooling.Core/Net10/Completion/CsxamlCompletionService.cs`
- `Csxaml.Tooling.Core/Net10/Hover/CsxamlHoverService.cs`
- existing generator/runtime/tooling event tests

## Design Requirements

Add a metadata-defined event binding kind for platform event args.

Recommended shape:

```csharp
public enum EventBindingKind
{
    Direct,
    TextValueChanged,
    BoolValueChanged,
    EventArgs
}
```

Expected delegate shape for `EventArgs`:

```csharp
Action<TEventArgs>
```

The runtime binder should adapt WinUI events like:

```csharp
control.SomeEvent += (_, args) => handler(args);
```

Keep `Direct` as `Action` and keep existing value-normalized event kinds.

## Implementation Steps

### 1. Extend Metadata Model

- Add the new `EventBindingKind`.
- Confirm `EventMetadata.HandlerTypeName` can carry event-args delegate types.
- If needed, add `EventArgsTypeName` only if `HandlerTypeName` is not enough.
- Keep the model small. Do not add a generic event transformation framework.

### 2. Curate Built-In Events

Add metadata for the first built-in event set:

- `FrameworkElement.Loaded` as `OnLoaded`
- `Selector.SelectionChanged` for supported selector controls
- `RangeBase.ValueChanged` for supported range controls
- `ListViewBase.ItemClick`
- `AutoSuggestBox.QuerySubmitted`
- `AutoSuggestBox.SuggestionChosen`
- `UIElement.KeyDown`
- common pointer events:
  - `PointerPressed`
  - `PointerReleased`
  - `PointerMoved`
  - `PointerEntered`
  - `PointerExited`
  - `PointerCanceled`
  - `PointerCaptureLost`
  - `PointerWheelChanged`
- `Frame.Navigating`
- `Frame.Navigated`
- `Frame.NavigationFailed`
- `Frame.NavigationStopped`

Add controls only when runtime creation/property coverage is sufficient for
tests. If a control needs metadata but no adapter exists, either add the
adapter in a small file or defer that control with a roadmap note.

### 3. Update Metadata Emission

- Update generated metadata emission for the new binding kind.
- Regenerate or update generated metadata according to the repo's existing
  metadata workflow.
- Keep generated output deterministic and ordered.

### 4. Update Generator Emission

- Ensure `NativeAttributeEmitter` casts event expressions to the handler type
  from metadata.
- Confirm the emitted type for `Action<TEventArgs>` is fully qualified.
- Add source mapping around event expression values.
- Add diagnostics or compile-fail tests for invalid event lambda shapes.

### 5. Update Runtime Event Binding

- Add a small reusable event-args binder for built-in adapters where possible.
- Keep each control adapter readable and under file-size limits.
- For controls needing direct adapter code, add one adapter per control or
  control family.
- Update `ExternalEventBinder` only after external metadata can identify a
  safe `Action<TEventArgs>` projection.
- Ensure rebinding removes old handlers before adding new handlers.
- Ensure null or absent handlers unbind previous handlers.

### 6. External Control Support

Add a solution-local external control fixture with a public event that follows
the normal WinUI pattern:

```csharp
public event EventHandler<StatusChangedEventArgs>? StatusChanged;
```

Metadata should expose:

```text
OnStatusChanged: Action<StatusChangedEventArgs>
```

Runtime should bind it as senderless args.

### 7. Tooling Updates

- Completion should list new `On...` event names.
- Hover should show delegate shape, including `Action<TEventArgs>`.
- C# projection should include the correct event expression type.
- Diagnostics should continue to report unknown events and invalid attributes.
- Semantic tokens should still classify event attributes consistently.

### 8. Documentation Updates

Update required surfaces in the same change:

- `FEATURE_PLAN.md` implementation status table: move this row to in progress
  or preview only when code exists.
- `LANGUAGE-SPEC.md`: event payload normalization and typed event args.
- `ROADMAP.md`: checklist and notes log.
- `docs/supported-feature-matrix.md`: status and notes.
- `docs-site/articles/guides/native-props-and-events.md`: examples and event
  table.
- `docs-site/articles/language/state-and-events.md`: event handler model.
- `docs-site/articles/guides/external-control-interop.md`: external event args
  support and boundaries.
- VS Code snippets if useful.

## Validation Checks

Run focused tests first:

```powershell
dotnet test .\Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj --no-restore -m:1
dotnet test .\Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj --no-restore -m:1 -p:UseAppHost=false
```

Run extension tests if grammar/snippets/tooling integration changed:

```powershell
Push-Location VSCodeExtension
npm test
Pop-Location
```

Run docs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/docs/Invoke-DocsBuild.ps1 -SkipProjectBuild
```

Run full solution when the focused tests pass:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1 -nr:false
```

## Completion Criteria

This phase is complete only when:

- common event args compile from `.csxaml`.
- runtime handlers receive the expected typed args.
- event rebinding remains leak-free.
- existing events remain compatible.
- external-control typed event args work for the fixture.
- tooling completion and hover show the new delegate shapes.
- docs, roadmap, spec, feature matrix, and implementation status table agree.

