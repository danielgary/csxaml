# Milestone 4 Plan - Interactive Controls and Controlled Input Model

## Status

- Drafted: 2026-04-10
- Completed: 2026-04-11
- Roadmap target: Milestone 4 in [ROADMAP.md](../../ROADMAP.md)
- Primary goal: make `TextBox` and `CheckBox` feel like first-class CSXAML controls without breaking the core language shape

---

## 1. Outcome

At the end of this milestone, a CSXAML author should be able to build an editor-style WinUI screen with:

- a selectable list of todos
- a detail editor pane
- controlled `TextBox` editing for title and notes
- controlled `CheckBox` editing for done state
- predictable rerender behavior while typing and toggling

The language surface should still feel like:

- XAML for structure
- C# for values and behavior
- metadata-driven validation rather than runtime guessing

This milestone is successful only if the demo is fully authored in `.csxaml` and ordinary typing does not immediately destroy focus or rewrite the active control state.

---

## 2. Language-Spec Constraints

This plan must preserve the rules in [LANGUAGE-SPEC.md](../../LANGUAGE-SPEC.md).

### Non-negotiable constraints

- Do not add a second expression language. All values and handlers remain ordinary C# inside `{ ... }`.
- Do not add loose bare attribute values. Attribute values remain either string literals or expression islands.
- Do not add code-behind ceremony or handler-name strings.
- Do not add parser ambiguity. Any milestone 4 surface must fit the current bounded-island parsing strategy.
- Keep native props and events metadata-driven.
- Keep event naming consistent with the normalized CSXAML style such as `OnClick`, `OnTextChanged`, and `OnCheckedChanged`.

### Practical implication

Milestone 4 is a runtime and metadata milestone, not a syntax milestone. The authoring model must look like this:

```xml
<TextBox
    Text={SelectedTitle}
    OnTextChanged={value => UpdateTitle(value)}
    Width={240} />

<CheckBox
    IsChecked={SelectedIsDone}
    OnCheckedChanged={value => UpdateDone(value)} />
```

---

## 3. Scope Decisions

These decisions are part of the plan and should be treated as the default implementation target unless real code inspection reveals a blocker.

### In scope

- add metadata support for `TextBox`
- add metadata support for `CheckBox`
- support controlled `TextBox.Text`
- support controlled `CheckBox.IsChecked`
- expose normalized CSXAML events `OnTextChanged` and `OnCheckedChanged`
- preserve stable control identity on ordinary rerenders when the tree position and tag stay the same
- add an editor-style demo
- add generator, metadata, and runtime tests

### Explicitly out of scope

- new parser syntax
- general retained reconciliation redesign beyond what is needed for stable typing
- full tri-state checkbox semantics
- full layout/property breadth promised by Milestone 6
- full source mapping or IDE diagnostics work from Milestones 11 and 12

### Controlled input conventions for this milestone

- `TextBox.Text` is the source of truth for the current rendered value.
- `OnTextChanged` receives the current string value as `Action<string>`.
- `CheckBox.IsChecked` is authored as a boolean expression in normal usage.
- `OnCheckedChanged` receives a normalized boolean value as `Action<bool>`.
- `CheckBox` tri-state behavior is deferred. If WinUI produces `null`, CSXAML normalizes it to `false` for this milestone.

This is a deliberate v1-quality developer-experience choice. It matches the roadmap examples and avoids dragging nullable tri-state UI semantics into the first interactive slice.

---

## 4. Deliverables

The milestone should land these concrete outcomes.

### Metadata and validation

- `TextBox` and `CheckBox` appear in generated control metadata
- supported milestone-4 props and events validate correctly
- string literal misuse and unknown input events still produce good diagnostics

### Runtime

- `TextBoxControlAdapter` exists
- `CheckBoxControlAdapter` exists
- control adapters suppress adapter-originated feedback loops where needed
- controlled values patch in place on retained controls

### Demo

- the demo becomes a Todo Editor rather than a toggle-only board
- editing title updates live
- editing notes updates live
- toggling done state updates live
- selecting a different item updates the editor pane without replacing controls unnecessarily

### Tests

- metadata generator tests cover new controls and projected events
- generator tests cover new validation and emission paths
- runtime tests cover value conversion, event rebinding, and controlled-input stability

---

## 5. Proposed Architecture Changes

These are the intended code-shape changes. Keep files small and explicit per [AGENTS.md](../../AGENTS.md).

## 5.1 Control metadata model

Current metadata is good for direct CLR-backed events like `Button.Click -> OnClick`, but Milestone 4 needs projected input events.

Planned change:

- extend `EventMetadata` to describe the CSXAML-exposed delegate shape, not just the raw CLR event
- add a small enum describing projected event behavior
- allow curated event definitions that are either:
  - direct CLR-backed events
  - synthetic projected events handled explicitly by runtime adapters

Recommended model additions:

- `EventBindingKind.cs`
- update `EventMetadata.cs`
- replace dictionary-only event mappings with explicit event definitions in the metadata generator

Recommended binding kinds:

- `Action`
- `TextValueChanged`
- `BoolValueChanged`

The exact enum names may vary, but the model must make `OnCheckedChanged` a first-class projected event rather than a hack.

## 5.2 Metadata generator

Current curated metadata uses a `Dictionary<string, string>` for event mappings. That is too weak for projected input events.

Planned change:

- introduce a curated event definition type
- support direct and synthetic event definitions
- add `TextBox` and `CheckBox` curated definitions
- extend value-kind resolution to unwrap `Nullable<T>` before choosing the hint

This keeps the metadata path explicit and future-friendly.

## 5.3 Generator

The parser should not change for this milestone.

Planned generator changes:

- validation continues to resolve native props/events from metadata
- event emission should include event value-kind hints, not only property value-kind hints
- validation tests should cover projected events as ordinary native events from the authoring perspective

## 5.4 Runtime

The runtime will need two new adapters plus a tiny amount of value-conversion work.

Planned runtime changes:

- add `TextBoxControlAdapter.cs`
- add `CheckBoxControlAdapter.cs`
- register both in `ControlAdapterRegistry.cs`
- extend `NativePropertyValueConverter.cs` to handle:
  - `bool`
  - `bool?`
  - widening from `bool` to `bool?`
- keep feedback-loop suppression local to the relevant adapters

Do not introduce a giant runtime-wide input manager.

Preferred implementation style:

- adapter-local state via a small helper type or `ConditionalWeakTable`
- one helper per concern if needed
- explicit comparisons before setting `TextBox.Text` or `CheckBox.IsChecked`

## 5.5 Demo

The demo should become a real editor screen, but it should stay within the current control/layout surface.

Recommended shape:

- keep `TodoBoard.csxaml` as the top-level shell
- add `SelectedItemId` state
- add a right-side editor component or inline editor section
- use a left sidebar made of existing controls already supported in Milestone 3 plus new input controls

Do not block this milestone on `Grid`. A horizontal `StackPanel` plus `Border` and nested panels is sufficient.

---

## 6. Concrete Control Surface For This Milestone

The point is to support the demo and validate the controlled-input model, not to finish the entire WinUI input/control matrix.

## 6.1 TextBox

Minimum metadata surface:

- `Text`
- `PlaceholderText`
- `Width`
- `AcceptsReturn`
- `TextWrapping`
- `MinHeight`

Projected event:

- `OnTextChanged`

Event delegate shape:

- `Action<string>`

Runtime behavior:

- read handler with `NativeElementReader.TryGetEventHandler<Action<string>>`
- bind to WinUI `TextChanged`
- invoke handler with the current `control.Text ?? string.Empty`
- do not reapply `Text` if the desired value already equals the current `TextBox.Text`

## 6.2 CheckBox

Minimum metadata surface:

- `IsChecked`
- `Content`

Projected event:

- `OnCheckedChanged`

Event delegate shape:

- `Action<bool>`

Runtime behavior:

- bind to the relevant WinUI checked-state events
- normalize current state to `bool`
- treat `null` as `false` for this milestone
- do not reapply `IsChecked` if the normalized desired value already matches the current state

---

## 7. Execution Phases

Execute these phases in order. Each phase should leave the repo buildable and tested before moving on.

## Phase 1 - Formalize projected input events

### Goal

Create a metadata model that can represent `OnTextChanged` and `OnCheckedChanged` cleanly.

### Tasks

- add a small event-binding-kind enum in `Csxaml.ControlMetadata/Model`
- update `EventMetadata.cs` to include projected binding information
- replace `CuratedControlDefinition` event dictionary usage with explicit curated event definitions
- update `SupportedControlFilter.cs` to build metadata from the new event definitions
- update `MetadataSourceEmitter.cs` and its tests

### Expected files

- `Csxaml.ControlMetadata/Model/EventBindingKind.cs`
- `Csxaml.ControlMetadata/Model/EventMetadata.cs`
- `Csxaml.ControlMetadata.Generator/Filtering/CuratedControlDefinition.cs`
- `Csxaml.ControlMetadata.Generator/Filtering/CuratedEventDefinition.cs`
- `Csxaml.ControlMetadata.Generator/Filtering/SupportedControlFilter.cs`
- `Csxaml.ControlMetadata.Generator/Emission/MetadataSourceEmitter.cs`
- related tests under `Csxaml.ControlMetadata.Generator.Tests`

### Done when

- generated metadata can describe projected events without faking them as plain CLR event aliases
- existing `Button.OnClick` still works after the refactor

## Phase 2 - Add `TextBox` and `CheckBox` metadata

### Goal

Expose the minimum useful input-control surface through the same metadata path used for the existing controls.

### Tasks

- add `TextBox` to `CuratedControlSet.cs`
- add `CheckBox` to `CuratedControlSet.cs`
- add the milestone-specific property lists
- add projected event definitions for `OnTextChanged` and `OnCheckedChanged`
- extend `ValueKindHintResolver.cs` to unwrap `Nullable<T>`
- regenerate `GeneratedControlMetadata.cs`

### Done when

- `ControlMetadataRegistry` recognizes `TextBox` and `CheckBox`
- metadata tests assert the new controls and events exist

## Phase 3 - Update generator validation and emission

### Goal

Make the generator treat the new input controls as ordinary native controls from the author’s point of view.

### Tasks

- keep the parser unchanged
- ensure `NativeElementValidator.cs` accepts the new input events via metadata
- keep string literal event diagnostics intact
- update `NativeAttributeEmitter.cs` to emit event value-kind hints
- add generator tests for:
  - valid `TextBox` usage
  - valid `CheckBox` usage
  - invalid string literal event values on input controls
  - invalid props or events on the new controls
  - emitted code that includes the new event/value hints

### Done when

- the authoring surface for input controls is indistinguishable from other native controls
- no parser changes were required

## Phase 4 - Add runtime value conversion support

### Goal

Teach the runtime to read boolean and nullable-boolean native values safely.

### Tasks

- extend `NativePropertyValueConverter.cs` to support:
  - `bool`
  - `bool?`
  - `bool -> bool?`
- keep existing double conversion behavior intact
- add focused tests in `Csxaml.Runtime.Tests/Adapters`

### Done when

- `NativeElementReader.TryGetPropertyValue<bool?>` succeeds for a `bool` expression value on `IsChecked`
- invalid boolean values still throw clear exceptions

## Phase 5 - Implement `TextBoxControlAdapter`

### Goal

Support controlled text input without obvious feedback loops or focus churn.

### Tasks

- add `TextBoxControlAdapter.cs`
- support the milestone-4 property list
- bind `OnTextChanged` to WinUI `TextChanged`
- normalize emitted handler shape to `Action<string>`
- suppress adapter-originated change notifications during controlled writes
- skip setting `Text` when the control already holds the desired value
- register the adapter in `ControlAdapterRegistry.cs`

### Required tests

- event rebinding replaces the old text handler with the new one
- re-render with the same text does not rewrite the control value
- controlled typing path updates state without replacing the control

### Implementation note

Use a small adapter-local state object. Do not turn `NativeEventBindingStore` into a generic state bag unless a very small extension is clearly cleaner.

## Phase 6 - Implement `CheckBoxControlAdapter`

### Goal

Support a simple controlled checked-state model that matches the roadmap examples.

### Tasks

- add `CheckBoxControlAdapter.cs`
- support `IsChecked` and `Content`
- bind `OnCheckedChanged`
- normalize `Checked`, `Unchecked`, and `Indeterminate` into `Action<bool>`
- suppress adapter-originated loops during controlled writes
- skip setting `IsChecked` when the normalized state already matches
- register the adapter in `ControlAdapterRegistry.cs`

### Required tests

- toggling to `true` invokes the latest handler with `true`
- toggling to `false` invokes the latest handler with `false`
- `null` input is normalized to `false`
- rerender with the same checked state does not replace the control

## Phase 7 - Build the Todo Editor demo

### Goal

Prove the milestone in a real CSXAML authoring experience.

### Recommended demo shape

- left pane: list of todo items as buttons
- right pane: editor area inside a `Border`
- title field: `TextBox`
- notes field: multiline `TextBox`
- done field: `CheckBox`

### Recommended file changes

- update `Csxaml.Demo/Models/TodoItemModel.cs` to include `Notes`
- update `Csxaml.Demo/Components/TodoBoard.csxaml`
- optionally add `Csxaml.Demo/Components/TodoEditor.csxaml`
- update `Csxaml.Demo/Support/TodoColors.cs` only if the new editor needs extra palette values

### Required state in the demo

- `Items`
- `SelectedItemId`

### Required interactions

- selecting a todo updates the editor pane
- editing title updates the selected todo
- editing notes updates the selected todo
- checking done updates the selected todo

### Demo authoring rule

Do not hide the milestone behind helper C# UI code. The screen should be visibly powered by CSXAML markup plus C# expressions.

## Phase 8 - Add regression coverage and manual checks

### Goal

Close the milestone with tests that make future refactors safer.

### Generator tests

- `TokenizerTests.cs` only if syntax changes are needed, which they should not be
- `NativeValidationTests.cs` for new input controls
- `CodeEmitterTests.cs` for input-control emission snapshots or targeted assertions

### Runtime tests

- `NativeElementReaderTests.cs` for bool/bool? conversion
- renderer tests for adapter reuse and event rebinding
- demo tests proving editor interactions mutate the logical tree as expected

### Manual verification

- launch the demo in the `x64` debug profile
- type into the title field and confirm focus stays in the field
- type into the notes field and confirm text is not reset on each keystroke
- toggle the done checkbox and confirm the editor and list stay in sync
- switch selection and confirm the editor updates predictably

---

## 8. Detailed File-Level Checklist

Use this as the agent execution checklist.

- [x] add projected event metadata model in `Csxaml.ControlMetadata/Model`
- [x] add curated event definition support in `Csxaml.ControlMetadata.Generator/Filtering`
- [x] add nullable-aware value-kind resolution in `Csxaml.ControlMetadata.Generator/Discovery/ValueKindHintResolver.cs`
- [x] add `TextBox` metadata definition in `Csxaml.ControlMetadata.Generator/Filtering/CuratedControlSet.cs`
- [x] add `CheckBox` metadata definition in `Csxaml.ControlMetadata.Generator/Filtering/CuratedControlSet.cs`
- [x] regenerate `Csxaml.ControlMetadata/Generated/GeneratedControlMetadata.cs`
- [x] update metadata generator tests
- [x] update `Csxaml.Generator/Emission/NativeAttributeEmitter.cs` to emit event hints
- [x] add generator validation tests for `TextBox` and `CheckBox`
- [x] extend `Csxaml.Runtime/Adapters/NativePropertyValueConverter.cs` for bool/bool?
- [x] add `Csxaml.Runtime/Adapters/TextBoxControlAdapter.cs`
- [x] add `Csxaml.Runtime/Adapters/CheckBoxControlAdapter.cs`
- [x] register new adapters in `Csxaml.Runtime/Adapters/ControlAdapterRegistry.cs`
- [x] add runtime adapter and conversion tests
- [x] update the demo model and components for the editor experience
- [x] add or update demo-focused runtime tests
- [x] build and manually smoke-test the demo
- [x] update `ROADMAP.md` milestone 4 status/checklist only after exit criteria are truly met

---

## 9. Verification Commands

Run these at minimum before calling the milestone done.

```powershell
dotnet test Csxaml.ControlMetadata.Generator.Tests\Csxaml.ControlMetadata.Generator.Tests.csproj
dotnet test Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj
dotnet test Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj
dotnet build Csxaml.Demo\Csxaml.Demo.csproj -p:Platform=x64
```

If the environment allows it, also launch the demo through the VS Code `Csxaml.Demo (x64)` profile and perform the manual checks listed above.

---

## 10. Risks and Guardrails

### Risk: synthetic events get hacked in ad hoc

Guardrail:

- represent projected events explicitly in metadata instead of hiding them inside adapter-only magic

### Risk: milestone 4 leaks into milestone 5

Guardrail:

- fix only the input-stability behavior required for ordinary rerenders
- do not attempt a full retained native reconciliation redesign here

### Risk: `CheckBox` null semantics spread through the API

Guardrail:

- normalize milestone-4 `OnCheckedChanged` to `Action<bool>`
- defer tri-state support deliberately

### Risk: layout scope balloons

Guardrail:

- add only the layout-related props needed for the editor demo
- leave general width/height/alignment breadth to Milestone 6

### Risk: adapter code grows opaque

Guardrail:

- keep helper methods short
- keep suppression logic local and named clearly
- add tests before adding tricky event/write guards

---

## 11. Exit Criteria Restated

Milestone 4 is complete only when all of the following are true:

- `TextBox` works through CSXAML
- `CheckBox` works through CSXAML
- state updates flow predictably from control events to component state
- typing does not immediately destroy focus on rerender
- the Todo Editor demo is fully expressible in CSXAML
- the new behavior is covered by tests

Milestone 4 is now complete in the roadmap. If a future regression reopens any of these guarantees, mark the milestone back to in progress until the behavior is restored.
