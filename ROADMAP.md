# CSXAML Roadmap

## Purpose

This document tracks the path from the current CSXAML prototype to a production-ready v1.

It is intended to be kept up to date by humans and agents. Every milestone includes:

- a clear purpose
- why it matters
- example syntax or demo targets
- explicit exit criteria
- a checkbox list for progress tracking

## How to use this file

Rules for updating this roadmap:

- Mark milestone status as work progresses.
- Keep milestone scope tight. Do not silently expand milestones.
- When a milestone changes, update both the milestone description and its checklist.
- If a milestone is split, add a note explaining why.
- Do not mark a milestone complete unless its exit criteria are met.
- If implementation reveals architecture problems, add notes under the relevant milestone.

Suggested status values:

- [ ] not started
- [~] in progress
- [x] complete
- [!] blocked or requires redesign

---

# Current Position

The project has already established early groundwork in these areas:

- build-time generation from `.csxaml`
- runtime rendering for a small control set
- React-like component model foundations
- typed component props
- conditional rendering
- repeated rendering with keyed identity
- Todo demo proving component semantics
- shared control metadata and metadata generation for curated WinUI controls
- generic native prop and event emission with metadata-driven validation
- runtime control adapters for `Border`, `Button`, `StackPanel`, and `TextBlock`
- Todo demo styling through CSXAML props for done/not-done state
- metadata, generator, and runtime regression coverage for the Milestone 3 path
- controlled `TextBox` and `CheckBox` support through metadata-driven validation and runtime adapters
- projected `OnTextChanged` and `OnCheckedChanged` events with C# delegate handlers
- Todo Editor demo proving controlled input state flow through CSXAML
- runtime coverage for controlled input suppression, native input reuse, and editor interaction flows
- retained native reconciliation for keyed and unkeyed native trees
- in-place `StackPanel` child patching to preserve retained native controls during reorder and sibling churn
- `TextBox` caret and selection restoration during controlled value patches

This roadmap starts from that point and moves toward a credible v1.

---

# V1 Definition

CSXAML v1 is ready when all of the following are true:

- developers can author `.csxaml` in a normal WinUI solution
- core component syntax is stable
- native WinUI props and events are broadly usable through metadata-driven validation
- interactive controls like `TextBox` and `CheckBox` behave correctly
- retained reconciliation preserves component and native control identity where expected
- external WinUI controls can be consumed cleanly
- Visual Studio provides IntelliSense, diagnostics, and navigation
- build and project system behavior are predictable
- diagnostics point back to `.csxaml` rather than forcing users into generated code
- documentation, samples, packaging, and versioning are ready for outside users

---

# Milestone Overview

- Milestone 1 - Compiler pipeline proof
- Milestone 2 - React-like component semantics
- Milestone 3 - Metadata-driven native props and stronger component props
- Milestone 4 - Interactive controls and controlled input model
- Milestone 5 - Retained native reconciliation and patching
- Milestone 6 - Layout breadth and attached property model
- Milestone 7 - External WinUI control interop
- Milestone 8 - Styling and theming strategy
- Milestone 9 - Slots and richer composition patterns
- Milestone 10 - Project system maturity and fast inner loop
- Milestone 11 - Visual Studio integration and IntelliSense
- Milestone 12 - Diagnostics, source mapping, and debugging
- Milestone 13 - Stabilization, compatibility, and test hardening
- Milestone 14 - Performance and large-app hardening
- Milestone 15 - Packaging, templates, docs, and v1 release

---

# Milestone 1 - Compiler Pipeline Proof

**Status:** [x]

## Purpose

Prove that a `.csxaml` file can be transformed into valid generated C# and rendered inside a WinUI app.

## Why it matters

This is the minimum proof that CSXAML can exist as a build-integrated language rather than a thought experiment.

## Representative example

```csharp
component Element Test {
    State<int> Count = new State<int>(0);

    return <StackPanel>
        <TextBlock Text={Count.Value.ToString()} />
        <Button Content="Increment" OnClick={() => Count.Value++} />
    </StackPanel>;
}
```

## Exit criteria

- `.csxaml` file is included in a WinUI project
- generation runs during build
- generated `.g.cs` compiles normally
- app mounts the generated component
- clicking button updates state and rerenders

## Checklist

- [x] build integration exists
- [x] minimal runtime exists
- [x] generated C# compiles
- [x] generated component renders
- [x] state invalidation rerenders UI

---

# Milestone 2 - React-like Component Semantics

**Status:** [x]

## Purpose

Prove that CSXAML is more than file-to-code generation by adding a minimal React-like component model.

## Why it matters

Without typed props, child composition, conditional rendering, and repeated rendering, CSXAML is just a thin transpiler.

## Representative examples

### Typed child component

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    return <StackPanel>
        <TextBlock Text={Title} />
        if (IsDone) {
            <TextBlock Text="Done" />
        }
        <Button Content="Toggle" OnClick={OnToggle} />
    </StackPanel>;
}
```

### Repeated rendering

```csharp
component Element TodoBoard {
    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateSeedItems());

    return <StackPanel>
        foreach (var item in Items.Value) {
            <TodoCard
                Key={item.Id.ToString()}
                Title={item.Title}
                IsDone={item.IsDone}
                OnToggle={() => Toggle(item.Id)} />
        }
    </StackPanel>;
}
```

## Exit criteria

- typed component props work
- child components can be rendered from parent components
- `if` rendering works
- repeated rendering works
- keyed component identity is preserved across rerenders

## Checklist

- [x] typed props supported
- [x] child component composition works
- [x] `if` blocks supported
- [x] constrained `foreach` supported
- [x] keyed child identity preserved
- [x] Todo demo proves component semantics

---

# Milestone 3 - Metadata-Driven Native Props and Stronger Component Props

**Status:** [x]

## Status note

Core Milestone 3 infrastructure is in place.

- shared metadata model exists
- build-time reflection metadata generation exists
- parser/validator/emitter use the metadata-driven native prop model
- runtime uses generic native nodes plus explicit control adapters
- Todo demo styling flows through CSXAML props instead of a hand-authored rendering path

Remaining rough edges around launch configuration, restore behavior, and inner-loop build stability belong to Milestone 10 rather than blocking Milestone 3 itself.

## Purpose

Replace hardcoded tiny native control surfaces with a maintainable metadata-driven native prop and event model.

## Why it matters

Without this, every native control becomes a hand-authored one-off. That does not scale and blocks future IntelliSense and tooling.

## Representative examples

### Native props and events through metadata

```xml
<Border Background={IsDone ? TodoColors.DoneBackground : TodoColors.NotDoneBackground}>
    <TextBlock Text={Title} Foreground={TodoColors.CardForeground} />
    <Button Content="Toggle" OnClick={OnToggle} />
</Border>
```

### Parent-to-child prop passing

```xml
<TodoCard
    Key={item.Id.ToString()}
    Title={item.Title}
    IsDone={item.IsDone}
    OnToggle={() => Toggle(item.Id)} />
```

## Exit criteria

- reflection-based metadata tables exist for supported controls
- parser captures native attributes generically
- validation uses metadata to validate native props and events
- generated code emits generic native prop and event structures
- runtime adapters create and patch supported native controls
- Todo cards visibly show green when done and red when not done

## Checklist

- [x] add shared control metadata model
- [x] add build-time reflection metadata generator
- [x] emit stable metadata artifact
- [x] parse native attributes generically
- [x] validate native props against metadata
- [x] validate native events against metadata
- [x] standardize component prop generation and passing
- [x] add runtime control adapter layer
- [x] support `Border`, `TextBlock`, `Button`, `StackPanel` through metadata path
- [x] update Todo demo for green/red done state
- [x] add tests for metadata generation, validation, and runtime application

---

# Milestone 4 - Interactive Controls and Controlled Input Model

**Status:** [x]

## Purpose

Prove that CSXAML can support real editing and form interaction, not just static rendering and button clicks.

## Why it matters

A UI system that cannot survive typing, checking boxes, and editing details is not ready for real use.

## Representative examples

```xml
<TextBox
    Text={Title}
    OnTextChanged={value => UpdateTitle(value)}
    Width={240} />

<CheckBox
    IsChecked={IsDone}
    OnCheckedChanged={value => UpdateDone(value)} />
```

## Demo target

A Todo Editor app with:

- sidebar list of items
- selected-item editor pane
- editable title
- editable notes
- done checkbox

## Exit criteria

- `TextBox` works through CSXAML
- `CheckBox` works through CSXAML
- state updates flow predictably from control events to component state
- typing does not immediately destroy focus on rerender
- editor demo is fully expressible in CSXAML

## Checklist

- [x] add metadata support for `TextBox`
- [x] add metadata support for `CheckBox`
- [x] add runtime adapters for input controls
- [x] define controlled input conventions
- [x] support `OnTextChanged`
- [x] support `OnCheckedChanged`
- [x] build editor-style demo
- [x] test focus and typing stability on ordinary rerenders

---

# Milestone 5 - Retained Native Reconciliation and Patching

**Status:** [x]

## Purpose

Move from rebuilding native controls aggressively to retaining and patching native controls when identity is stable.

## Why it matters

Without retained reconciliation, interactive controls feel fragile and performance will degrade quickly.

## Representative example

If a selected todo is open in a `TextBox`, toggling another item in the sidebar should not replace that `TextBox` or reset focus.

## Exit criteria

- native controls are reused when type and identity match
- supported properties patch in place
- keyed lists preserve native and component identity correctly
- input controls survive normal parent rerenders

## Checklist

- [x] introduce mounted native instance model
- [x] define native identity matching rules
- [x] patch supported properties in place
- [x] rebind events safely on retained controls
- [x] preserve focus when possible
- [x] preserve caret/selection when possible
- [x] test keyed reorder behavior
- [x] test retained editor behavior under rerender

---

# Milestone 6 - Layout Breadth and Attached Property Model

**Status:** [ ]

## Purpose

Make it possible to build real app screens with richer layout containers and layout-specific properties.

## Why it matters

A real WinUI authoring model needs more than `StackPanel` and a few leaf controls.

## Representative example

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="260,*">
    <TextBlock Grid.Row={0} Grid.ColumnSpan={2} Text="Task Editor" FontSize={22} />
    <StackPanel Grid.Row={1} Grid.Column={0}>
        <!-- list -->
    </StackPanel>
    <Border Grid.Row={1} Grid.Column={1} Padding={16}>
        <!-- details -->
    </Border>
</Grid>
```

## Exit criteria

- `Grid` works
- attached-property story exists and is validated
- common layout props are usable through metadata
- a real two-pane app screen can be built in CSXAML

## Checklist

- [ ] add metadata support for `Grid`
- [ ] add metadata support for `ScrollViewer`
- [ ] define attached property syntax and validation
- [ ] support row/column and span assignment
- [ ] support common layout props like margin, padding, alignment, width, height
- [ ] build a two-pane editor layout demo
- [ ] add tests for attached property validation and runtime application

---

# Milestone 7 - External WinUI Control Interop

**Status:** [ ]

## Purpose

Allow CSXAML apps to consume external WinUI controls and custom controls, not just the controls CSXAML explicitly knows about.

## Why it matters

This is one of the biggest v1 credibility gates. If CSXAML cannot interoperate with the wider WinUI ecosystem, it will feel boxed in.

## Representative examples

### External control

```xml
<FluentUi:InfoBar
    Severity="Warning"
    IsOpen={ShowWarning}
    Message="Unsaved changes" />
```

### Internal custom control

```xml
<Controls:UserAvatar
    UserId={CurrentUserId}
    Size={48}
    OnClick={OpenProfile} />
```

## Exit criteria

- namespace/import model exists for external controls
- metadata generation can target external controls
- validation works for external control props/events
- runtime can create and patch external controls through the same adapter path

## Checklist

- [ ] define namespace/import model for non-built-in controls
- [ ] support metadata generation for referenced external control types
- [ ] validate external control props/events
- [ ] support runtime creation of external controls
- [ ] add at least one third-party or custom control demo
- [ ] document interop boundaries and limitations

---

# Milestone 8 - Styling and Theming Strategy

**Status:** [ ]

## Purpose

Give app authors a coherent way to reuse visual values and styles without repeating raw props everywhere.

## Why it matters

Production apps need visual consistency and a sane theme story.

## Representative examples

```xml
<Button
    Content="Save"
    Background={Theme.PrimaryBrush}
    Foreground={Theme.OnPrimaryBrush}
    Style={AppStyles.PrimaryButton} />
```

```xml
<TodoCard
    Title={item.Title}
    IsDone={item.IsDone}
    AccentBrush={item.IsDone ? Theme.SuccessBrush : Theme.DangerBrush} />
```

## Exit criteria

- reusable visual constants and helper strategy exists
- style-like reuse is possible without awkward markup duplication
- theme-aware examples exist

## Checklist

- [ ] decide v1 theme/value reuse approach
- [ ] support passing style-like objects where appropriate
- [ ] document when to use raw props vs reusable styles
- [ ] update demo(s) to use reusable theme values
- [ ] avoid premature full styling DSL unless clearly justified

---

# Milestone 9 - Slots and Richer Composition Patterns

**Status:** [ ]

## Purpose

Let components become reusable composition primitives rather than prop-only wrappers.

## Why it matters

This is where the component authoring experience becomes expressive enough for real UI architecture.

## Representative examples

### Default slot

```xml
<Card>
    <TextBlock Text="Deployment Settings" />
    <Button Content="Apply" OnClick={Apply} />
</Card>
```

### Named slots later if needed

```xml
<Card>
    <Header>
        <TextBlock Text="Deployment Settings" />
    </Header>
    <Body>
        <TextBox Text={EnvironmentName} />
    </Body>
</Card>
```

## Exit criteria

- components can accept child content cleanly
- composition patterns like `Card`, `FormSection`, and layout wrappers feel natural
- slot semantics are documented and testable

## Checklist

- [ ] define child-content model for component slots
- [ ] support default slot
- [ ] decide whether named slots are in v1 or post-v1
- [ ] add reusable layout primitive demos
- [ ] test composition nesting and identity behavior

---

# Milestone 10 - Project System Maturity and Fast Inner Loop

**Status:** [ ]

## Purpose

Turn the build experience from a working custom integration into a predictable project experience.

## Why it matters

Production readiness depends on reliable builds, stable multi-project behavior, and fast iteration.

## Representative scenarios

- add a component in project A and consume it from project B
- change child prop signature and get immediate build errors
- clean/rebuild behaves predictably
- stale generated files do not linger and confuse the build

## Exit criteria

- robust MSBuild integration exists
- incremental generation works predictably
- design-time/build-time behavior is stable enough for normal development
- multi-project component discovery works

## Checklist

- [ ] harden MSBuild targets
- [ ] support incremental generation
- [ ] ensure generated files in `obj` are deterministic and cleaned correctly
- [ ] support cross-project component references
- [ ] support stable design-time generation behavior where possible
- [ ] test clean/rebuild and rename scenarios

---

# Milestone 11 - Visual Studio Integration and IntelliSense

**Status:** [ ]

## Purpose

Provide a first-class authoring experience in Visual Studio.

## Why it matters

This is one of the explicit v1 goals. Developers should not have to guess syntax or spelunk generated files.

## Representative experience goals

- typing `<But` suggests `Button`
- inside `<Button ...>` you get `Content`, `Background`, `Foreground`, `OnClick`
- inside `<TodoCard ...>` you get component prop completion
- invalid props get squiggles before build

## Exit criteria

- syntax highlighting exists
- tag/prop/event IntelliSense exists
- component prop IntelliSense exists
- diagnostics surface in the editor
- navigation to component definitions is possible

## Checklist

- [ ] define tooling-facing metadata API
- [ ] expose native control metadata to tooling layer
- [ ] expose component symbol metadata to tooling layer
- [ ] implement syntax highlighting
- [ ] implement completion for tags
- [ ] implement completion for props and events
- [ ] implement completion for component props
- [ ] implement editor diagnostics
- [ ] implement basic navigation or go-to-definition

---

# Milestone 12 - Diagnostics, Source Mapping, and Debugging

**Status:** [ ]

## Purpose

Make errors understandable from the original `.csxaml` source.

## Why it matters

Users will not trust the system if every failure sends them into generated C#.

## Representative examples

### Invalid prop diagnostic

```xml
<Button Text="Save" />
```

Should produce a CSXAML diagnostic like:

- `Text` is not a supported property on `Button`
- did you mean `Content`?

## Exit criteria

- parser/generator diagnostics point to source spans in `.csxaml`
- build errors are readable in source terms
- runtime failures can identify relevant component/tag context where possible
- source mapping exists between `.csxaml` and generated code

## Checklist

- [ ] improve source-span tracking in parser and emitter
- [ ] map diagnostics back to `.csxaml`
- [ ] add helpful suggestion messages where possible
- [ ] improve runtime exception context
- [ ] define source mapping format or strategy
- [ ] document debugging workflow

---

# Milestone 13 - Stabilization, Compatibility, and Test Hardening

**Status:** [ ]

## Purpose

Turn the framework from a moving prototype into something teams can depend on.

## Why it matters

Production readiness is as much about stability and change control as features.

## Representative outcomes

- syntax stability promises
- documented supported control set
- compatibility policy for generator/runtime changes
- broad regression coverage

## Exit criteria

- compatibility policy exists
- supported feature matrix exists
- regression suite covers major language/runtime features
- breaking changes are documented and versioned intentionally

## Checklist

- [ ] define syntax compatibility policy
- [ ] define runtime/generator compatibility policy
- [ ] publish supported control/feature matrix
- [ ] expand golden tests for generator output
- [ ] expand runtime reconciliation regression tests
- [ ] add interop regression tests
- [ ] document known limitations clearly

---

# Milestone 14 - Performance and Large-App Hardening

**Status:** [ ]

## Purpose

Ensure CSXAML remains responsive under realistic application load.

## Why it matters

A coherent design still fails if typing, rerendering, or metadata processing becomes sluggish.

## Representative stress cases

- 1,000-item list
- repeated rerenders while editing selected item
- nested component trees with many props
- frequent patching of retained native controls

## Exit criteria

- benchmark suite exists
- major hot paths are understood
- typing/editing latency is acceptable
- list and rerender performance are acceptable for intended v1 scale

## Checklist

- [ ] create benchmark scenarios
- [ ] benchmark generator performance
- [ ] benchmark metadata lookup paths
- [ ] benchmark runtime reconciliation and patching
- [ ] optimize known hot spots
- [ ] document v1 scale expectations

---

# Milestone 15 - Packaging, Templates, Docs, and V1 Release

**Status:** [~]

## Purpose

Make CSXAML adoptable by someone other than the original author.

## Why it matters

A production-ready v1 needs packaging, samples, templates, docs, and a clear supported story.

## Representative deliverables

- NuGet packages
- starter template or sample app
- docs for syntax, native props, interop, and debugging
- versioned release notes

## Exit criteria

- packages can be consumed in a normal solution
- sample apps exist
- docs cover the supported v1 surface
- v1 release process is defined

## Checklist

- [ ] define package boundaries
- [ ] publish NuGet packaging plan
- [ ] add starter template or example app
- [x] write syntax guide
- [ ] write native props/events guide
- [ ] write external control interop guide
- [ ] write debugging and diagnostics guide
- [ ] publish release process and versioning notes
- [ ] tag and announce v1 release

---

# Major V1 Gates

These are the biggest checkpoints on the road to v1.

## Gate A - Native Surface Gate

CSXAML can express a meaningful subset of WinUI controls and properties through metadata-driven validation and runtime application.

Related milestones:
- Milestone 3
- Milestone 4
- Milestone 6

## Gate B - Runtime Credibility Gate

Interactive controls behave correctly, and retained reconciliation preserves identity and focus well enough for real apps.

Related milestones:
- Milestone 4
- Milestone 5

## Gate C - Ecosystem Gate

External WinUI controls can be consumed cleanly.

Related milestones:
- Milestone 7

## Gate D - Tooling Gate

Visual Studio authoring experience is good enough that developers stop thinking about the generator most of the time.

Related milestones:
- Milestone 10
- Milestone 11
- Milestone 12

## Gate E - Productization Gate

Documentation, packaging, tests, and versioning are ready for outside users.

Related milestones:
- Milestone 13
- Milestone 14
- Milestone 15

---

# Risks to Watch

Use this section to track recurring risks as the project evolves.

## Architecture risks

- [ ] parser, validation, and emission begin mixing together
- [ ] runtime and WinUI-specific rendering become too coupled
- [ ] metadata model becomes inconsistent between runtime, generator, and tooling
- [ ] component props drift toward weakly typed bags
- [ ] control adapter layer turns into giant switch statements

## Runtime risks

- [ ] keys are not required or enforced consistently enough
- [ ] retained reconciliation becomes ad hoc and hard to reason about
- [ ] focus/caret preservation proves brittle
- [ ] event rebinding leaks handlers or duplicates handlers

## Tooling risks

- [ ] build-time and design-time behavior diverge too much
- [ ] generated-code debugging remains necessary too often
- [ ] metadata is not rich enough to power IntelliSense cleanly

## Product risks

- [ ] syntax grows faster than runtime semantics stabilize
- [ ] interop with external controls remains awkward
- [ ] docs lag behind implementation too much
- [ ] breaking changes happen without a clear policy

---

# Notes

Use this section to capture milestone-specific discoveries that affect future planning.

## Notes log

- Add dated notes here as architecture or tooling decisions evolve.
- Record when milestones are split or re-scoped.
- Record which milestones are considered blockers for v1.
- 2026-04-10: Milestone 3 core implementation completed. Metadata-driven native props/events, generic native node emission, runtime control adapters, and Todo demo red/green styling are all in place. Remaining demo launch/build friction is being treated as project-system maturity work under Milestone 10.
- 2026-04-10: Added a draft language specification focused on C# compatibility, XAML familiarity, parseability, and developer experience. This is real progress toward Milestone 15, but it does not close the broader documentation and release-preparation work.
- 2026-04-10: Added a `VSCodeExtension` scaffold for CSXAML syntax highlighting. It uses a hybrid TextMate grammar with XML/XAML-style markup scopes and embedded C# regions. This helps experimentation and editor usability, but it does not replace the Milestone 11 Visual Studio tooling goals.
- 2026-04-10: Extended `VSCodeExtension` with snippets and a lightweight semantic token provider that distinguishes native tags, component tags, native props, native events, component props, and the reserved `Key` attribute. This improves VS Code experimentation, but it is still not a substitute for full Milestone 11 tooling.
- 2026-04-10: Added a detailed Milestone 4 execution plan in `docs/plans/milestone-4-interactive-controls.md`. The plan locks the milestone to a syntax-preserving controlled-input slice centered on `TextBox`, `CheckBox`, projected input events, runtime adapter stability, and a Todo Editor demo.
- 2026-04-10: Milestone 4 completed. `TextBox` and `CheckBox` now flow through shared metadata, projected `OnTextChanged` and `OnCheckedChanged` events, runtime input adapters, and a Todo Editor demo. Input-stability coverage is provided by controlled-write suppression tests plus retained native reuse tests for `TextBox` and `CheckBox`.
- 2026-04-11: Fixed a live Milestone 4 regression where typing into a `TextBox` immediately destabilized input focus. The cause was container adapters reassigning identical retained children on each rerender. `StackPanel` and `Border` now preserve unchanged child references instead of tearing down and reattaching the same subtree.
- 2026-04-11: Milestone 5 completed. Native reconciliation now retains mounted controls by tag plus key, rejects duplicate sibling keys, patches `StackPanel` child collections in place for reorder scenarios, and restores `TextBox` selection when controlled writes change the underlying value. Runtime coverage now includes keyed native reorder tests plus an editor-retention scenario where unrelated sidebar changes do not replace the selected editor field.
