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
- file-level `using` directives and alias-qualified tag parsing for external controls and attached-property owners
- referenced-assembly external control discovery plus generated runtime registration for supported external controls
- demo proof for one package-provided external control (`InfoBar`) and one solution-local custom control (`StatusButton`)
- regression coverage for external control import resolution, metadata generation, validation, emitted registration, and demo-tree authoring
- shared repo-level CSXAML build assets with project opt-in instead of handwritten per-project generation targets
- deterministic component namespace behavior through explicit file namespaces or a project-default fallback
- deterministic internal generated namespaces for project-level support files such as manifests and external-control registration
- generated component manifests plus referenced-assembly component discovery for cross-project CSXAML imports
- proof that one CSXAML component library can be consumed from another CSXAML project through ordinary `using` imports and aliases
- proof that an ordinary C# test project can consume generated CSXAML components through normal `ProjectReference` usage without duplicate generator wiring
- deterministic generated-file writing, stale-file pruning, and clean integration through the shared `obj` pipeline

This roadmap starts from that point and moves toward a credible v1.

---

# V1 Definition

CSXAML v1 is ready when all of the following are true:

- developers can author `.csxaml` in a normal WinUI solution
- core component syntax is stable
- native WinUI props and events are broadly usable through metadata-driven validation
- interactive controls like `TextBox` and `CheckBox` behave correctly
- retained reconciliation preserves component and native control identity where expected
- `.csxaml` files support ordinary local helper code and file-local helper types so component logic stays readable as screens grow
- component libraries, external controls, and attached-property owners use one normal `namespace`/`using` model rather than parallel import systems
- external WinUI and library-provided controls can be consumed through ordinary `using`-based imports and aliases rather than bespoke interop glue
- async work, lifecycle, and cleanup behavior are explicit and testable
- styling and theming stay intentionally thin and compose cleanly with WinUI styles and resources
- Visual Studio provides IntelliSense, diagnostics, and navigation
- build and project system behavior are predictable
- developers can write automated tests against CSXAML components from ordinary C# test projects without depending on generated-code spelunking
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

**Status:** [x]

## Purpose

Make it possible to build real app screens with richer layout containers and layout-specific properties, while establishing the first attached-property path needed for accessibility and semantic UI testing.

## Why it matters

A real WinUI authoring model needs more than `StackPanel` and a few leaf controls. The attached-property story also becomes the natural place for automation metadata that good tests can target without inventing CSXAML-only testing syntax.

## Representative example

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="260,*">
    <TextBlock
        Grid.Row={0}
        Grid.ColumnSpan={2}
        Text="Task Editor"
        AutomationProperties.Name="Task Editor"
        FontSize={22} />
    <StackPanel Grid.Row={1} Grid.Column={0}>
        <!-- list -->
    </StackPanel>
    <Border Grid.Row={1} Grid.Column={1} Padding={16}>
        <TextBox AutomationProperties.AutomationId="SelectedTodoTitle" Text={SelectedTitle} />
    </Border>
</Grid>
```

## Exit criteria

- `Grid` works
- attached-property story exists and is validated
- common layout props are usable through metadata
- initial attached-property coverage includes an automation metadata path suitable for accessibility and semantic UI tests
- a real two-pane app screen can be built in CSXAML

## Checklist

- [x] add metadata support for `Grid`
- [x] add metadata support for `ScrollViewer`
- [x] define attached property syntax and validation
- [x] support row/column and span assignment
- [x] support common layout props like margin, padding, alignment, width, height
- [x] support initial automation attached properties such as `AutomationProperties.Name` and/or `AutomationProperties.AutomationId`
- [x] build a two-pane editor layout demo
- [x] add tests for attached property validation and runtime application

---

# Milestone 7 - External WinUI Control Interop

**Status:** [x]

## Purpose

Allow CSXAML apps to consume external WinUI controls and custom controls from project references and NuGet packages through the same authoring model as built-in controls.

## Why it matters

This is a make-or-break v1 credibility gate. If CSXAML cannot interoperate with the wider WinUI ecosystem through normal `using`-style authoring, it will feel boxed in and toy-like.

## Representative examples

### External control through a namespace alias

```csharp
using WinUi = Microsoft.UI.Xaml.Controls;

return <WinUi:InfoBar
    Severity={WinUi.InfoBarSeverity.Warning}
    IsOpen={ShowWarning}
    Message="Unsaved changes" />;
```

### Local or referenced custom control

```csharp
using Controls = MyApp.Controls;

return <Controls:UserAvatar
    UserId={CurrentUserId}
    Size={48}
    OnClick={OpenProfile} />;
```

## Intentional v1 direction

- file-level imports should use ordinary C#-shaped `using` forms, including `using Namespace;` and `using Alias = Namespace;`
- bare control names may work when imported and unambiguous, but aliases should be the preferred example path for external controls
- external controls should enter CSXAML through referenced-assembly metadata generation rather than a hardcoded built-in list
- supported external controls should render through generated or explicitly registered adapter paths, not ad hoc hot-path reflection
- the first supported slice should be explicit and documented rather than pretending every WinUI control shape is automatically supported

## Landed slice

- file-level `using` directives and alias-qualified tags now resolve both package-provided controls and solution-local custom controls
- referenced managed assemblies are scanned deterministically through a dedicated load context rather than a hardcoded built-in control list
- generated support code registers discovered external controls with runtime descriptors and shared metadata
- supported external control types are public non-abstract non-generic `FrameworkElement`-derived controls with public parameterless constructors
- supported external properties include recognized writable dependency properties plus recognized simple writable CLR properties (`string`, `bool`, numeric, enum, `Thickness`, `Brush`, `Style`)
- supported external events currently project public `void` events into direct `Action`-style handlers
- the current supported slice and its limitations are documented in `docs/external-control-interop.md`
- unsupported shapes still fail deterministically with source diagnostics instead of silently falling through to runtime guesswork

## Exit criteria

- a file-level `using` and alias model exists for external controls and is consistent with the language spec
- external control name resolution is deterministic, including clear ambiguity diagnostics
- metadata generation can target supported controls from referenced projects and NuGet packages
- validation works for supported external control props/events using the same metadata path as built-in controls
- runtime can create and patch supported external controls through generated or registered adapter paths
- proof exists with at least one NuGet-provided control and one solution-local custom control
- interop boundaries and unsupported cases are documented clearly enough that developers know what v1 does and does not promise

## Checklist

- [x] define file-level `using` semantics for imported control namespaces
- [x] define alias semantics using C#-style `using Alias = Namespace;`
- [x] define ambiguity diagnostics for bare external control names
- [x] support referenced-assembly metadata generation for supported external control types
- [x] decide how external control support is declared or discovered so metadata generation stays deterministic
- [x] support validation for external control props/events through the shared metadata model
- [x] support runtime creation and patching of supported external controls through generated adapters or an explicit registration model
- [x] prove interop with at least one NuGet-provided control
- [x] prove interop with at least one solution-local custom control library
- [x] add regression tests for import resolution, metadata generation, validation, and runtime interop
- [x] document the supported external-control slice and its limitations

---

# Milestone 8 - Styling and Theming Strategy

**Status:** [x]

## Purpose

Give app authors a coherent, intentionally thin way to reuse visual values and styles without repeating raw props everywhere.

## Why it matters

Production apps need visual consistency and a sane theme story, but v1 should not invent a second framework-owned styling system when WinUI styles/resources and plain C# objects already cover the need.

## Design constraints

- styling stays inside ordinary attribute expressions and ordinary C# helper code
- WinUI `Style`, resource dictionaries, and typed theme helpers remain the primary reuse mechanisms
- v1 does not add CSS-like classes, selector syntax, or a framework-owned styling DSL
- reusable style helpers must remain compatible with hostless logical-tree tests; WinUI style realization can be deferred until projected rendering time

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

- v1 styling/theming story is explicit and intentionally thin
- reusable visual constants and helper strategy exists
- WinUI `Style`/resource-style reuse and typed theme helpers are usable without awkward markup duplication
- theme-aware examples exist
- hostless logical-tree tests can observe reusable style intent without depending on live WinUI activation
- richer styling direction is documented without committing v1 to a custom styling DSL

## Checklist

- [x] decide v1 theme/value reuse approach
- [x] support passing existing WinUI `Style` and theme/resource objects where appropriate
- [x] keep reusable style helpers compatible with hostless logical-tree tests
- [x] document when to use raw props vs reusable styles vs existing WinUI style/resource mechanisms
- [x] define what is intentionally out of scope for v1 styling so a parallel DSL does not accrete accidentally
- [x] update demo(s) to use reusable theme values
- [x] document the compatibility path for richer styling later
- [x] avoid premature full styling DSL unless clearly justified

---

# Milestone 9 - Slots, Local Helper Code, and Richer Composition Patterns

**Status:** [x]

## Purpose

Let components become reusable composition primitives rather than prop-only wrappers, while keeping real component files readable by allowing ordinary helper code in the same `.csxaml` file.

## Why it matters

This is where the component authoring experience becomes expressive enough for real UI architecture. Real components need both child composition and a place to name local calculations and helper behavior without collapsing into giant inline expressions.

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

### Local helper code in a component file

```csharp
component Element TodoBoard {
    State<List<TodoItemModel>> Items = ...;
    var selected = Items.Value.Single(item => item.Id == SelectedItemId.Value);

    string HeaderText() => $"{selected.Title} ({Items.Value.Count})";

    return <TodoEditor Title={selected.Title} Header={HeaderText()} />;
}
```

## Exit criteria

- components can accept child content cleanly
- component files can contain ordinary local helper code plus file-local helper types without obscuring the render path
- composition patterns like `Card`, `FormSection`, and layout wrappers feel natural
- slot semantics are documented and testable
- named-slot direction is documented even if v1 ships with default-slot-only support

## Checklist

- [x] define how local helper code lives in a `.csxaml` file while preserving one-component-per-file discipline
- [x] support local variables and local functions before the final markup return
- [x] support file-local helper types and deterministic lowering for any additional same-file helper declarations
- [x] define child-content model for component slots
- [x] support default slot
- [x] explicitly defer named slots in v1 while documenting the compatibility path
- [x] add reusable layout/helper-code demos
- [x] test helper-code parsing, composition nesting, and identity behavior

---

# Milestone 10 - Project System Maturity and Fast Inner Loop

**Status:** [x]

## Status note

Milestone 10 is now landed for the current repo-level build model.

- shared MSBuild integration lives in repo-level `Directory.Build.props`, `Directory.Build.targets`, `build/Csxaml.props`, and `build/Csxaml.targets`
- generating projects opt in through one obvious property rather than hand-copying custom targets
- `.csxaml` files now generate into either their explicit file namespace or a deterministic project-default namespace
- project-level helper output now lives in a deterministic internal namespace based on the project-default namespace
- generating assemblies emit a project-level component manifest provider through the shared metadata contract
- downstream generator runs load referenced component manifests from referenced assemblies and resolve imported components through the same `using` and alias model used for external controls
- generated output is written under `obj`, unchanged files are not rewritten, stale files are pruned, and `clean` removes the intermediate CSXAML directory
- fixture projects prove library-to-library component import plus ordinary C# test-project consumption without duplicate generator wiring

This milestone does not close full IDE tooling or rich design-time authoring. Those concerns still belong to Milestones 11 and 12.

## Purpose

Turn the build experience from a working custom integration into a predictable project experience across app projects, component libraries, and test projects.

## Why it matters

Production readiness depends on reliable builds, stable multi-project behavior, fast iteration, and a testing story that does not require bespoke generation steps in every test project.

## Representative scenarios

- add a component in project A and consume it from project B
- move a component into a library namespace and import it with the same normal `using` model used for external controls
- reference a CSXAML component library from a test project and instantiate generated component types predictably
- add a NuGet package or project reference that exposes WinUI controls and have CSXAML metadata/update behavior stay deterministic
- change child prop signature and get immediate build errors
- clean/rebuild behaves predictably
- stale generated files do not linger and confuse the build

## Exit criteria

- robust MSBuild integration exists
- incremental generation works predictably
- design-time/build-time behavior is stable enough for normal development
- component and control namespace resolution works predictably across projects using the same `namespace`/`using` model
- multi-project component discovery works
- public package/API boundaries for runtime, generator, metadata, testing, and tooling are defined before downstream integrations freeze
- test projects can consume generated components predictably without hand-authored duplicate generation wiring

## Checklist

- [x] harden MSBuild targets
- [x] support incremental generation
- [x] ensure generated files in `obj` are deterministic and cleaned correctly
- [x] define file-scoped namespace behavior and default namespace conventions for `.csxaml`
- [x] support cross-project component references
- [x] support cross-project component references through the same `using`-based import model as external controls
- [x] support stable generated namespace/output conventions for downstream project and test-project references
- [x] support test-project references to component projects without custom regeneration steps
- [x] support predictable invalidation and regeneration when referenced control libraries or package versions change
- [x] define stable public package/API boundaries for runtime, generator, metadata, testing, and tooling before release-hardening work
- [x] support stable design-time generation behavior where possible
- [x] test clean/rebuild and rename scenarios

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
- after adding `using Fluent = ...;`, typing `<Fluent:` suggests imported external controls
- inside `<TodoCard ...>` you get component prop completion
- formatting keeps mixed markup and helper code readable
- invalid props get squiggles before build

## Exit criteria

- syntax highlighting exists
- tag/prop/event IntelliSense exists
- component prop IntelliSense exists
- diagnostics surface in the editor
- baseline formatting and indentation support exist
- navigation to component definitions is possible

## Checklist

- [ ] define tooling-facing metadata API
- [ ] expose native control metadata to tooling layer
- [ ] expose imported external control metadata and import-resolution context to the tooling layer
- [ ] expose component symbol metadata to tooling layer
- [ ] implement syntax highlighting
- [ ] implement completion for tags
- [ ] implement completion for props and events
- [ ] implement completion for component props
- [ ] implement editor diagnostics
- [ ] implement baseline formatting and indentation rules for mixed markup/C# files
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

Turn the framework from a moving prototype into something teams can depend on, including an explicit automated component testing story and a minimal lifecycle/disposal model.

## Why it matters

Production readiness is as much about stability and change control as features. Real components need a boring, explicit answer for subscriptions, async work, and cleanup.

## Representative outcomes

- syntax stability promises
- documented supported control set
- compatibility policy for generator/runtime changes
- explicit lifecycle/disposal guidance that avoids framework magic
- C#-first component testing APIs that do not require generated-code spelunking
- broad regression coverage

## Exit criteria

- compatibility policy exists
- supported feature matrix exists
- lifecycle/async/disposal model exists and is documented
- developers can write automated component tests against CSXAML components without manually booting a full app when logical-tree testing is sufficient
- regression suite covers major language/runtime features
- breaking changes are documented and versioned intentionally

## Checklist

- [ ] define syntax compatibility policy
- [ ] define runtime/generator compatibility policy
- [ ] publish supported control/feature matrix
- [ ] define the minimal lifecycle/disposal model and keep it smaller than a custom effect framework
- [ ] define async state-update behavior and unmounted-component behavior
- [ ] add a hostless C#-first component testing harness over the logical tree/runtime coordinator path
- [ ] add semantic query helpers that prefer text, name, and automation metadata over brittle child-index assertions
- [ ] add interaction helpers for common component tests such as click, text input, and checked-state changes
- [ ] expand golden tests for generator output
- [ ] expand runtime reconciliation regression tests
- [ ] add regression tests for mount/unmount cleanup and async completion safety
- [ ] add interop regression tests, including referenced package and solution-local controls
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
- docs and samples for automated component testing
- versioned release notes

## Exit criteria

- packages can be consumed in a normal solution
- sample apps exist
- docs cover the supported v1 surface
- v1 release process is defined

## Checklist

- [ ] publish finalized package boundaries and package names
- [ ] publish NuGet packaging plan
- [ ] add starter template or example app
- [x] write syntax guide
- [ ] write native props/events guide
- [x] write external control interop guide
- [ ] write component testing guide
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

External WinUI controls and solution/library controls can be consumed through ordinary `using` imports and aliases with predictable validation and runtime behavior.

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
- [ ] external control interop depends on broad implicit assembly scanning instead of a deterministic metadata path
- [ ] helper-code support turns the parser into a quasi-general-purpose C# parser
- [ ] component namespaces and external-control namespaces drift into separate mental models

## Runtime risks

- [ ] keys are not required or enforced consistently enough
- [ ] retained reconciliation becomes ad hoc and hard to reason about
- [ ] focus/caret preservation proves brittle
- [ ] event rebinding leaks handlers or duplicates handlers
- [ ] external control rendering falls back to reflection-heavy generic patching in hot paths
- [ ] lifecycle/disposal behavior becomes implicit or surprising

## Tooling risks

- [ ] build-time and design-time behavior diverge too much
- [ ] generated-code debugging remains necessary too often
- [ ] metadata is not rich enough to power IntelliSense cleanly
- [ ] component testing requires too much manual generation wiring or direct knowledge of generated namespaces

## Product risks

- [ ] syntax grows faster than runtime semantics stabilize
- [ ] interop with external controls remains awkward
- [ ] docs lag behind implementation too much
- [ ] automated component testing remains too structural and brittle because semantic query hooks arrive too late
- [ ] styling grows into a second framework-owned DSL instead of staying thin
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
- 2026-04-13: Testability is now being treated as an explicit v1 concern. The roadmap assumes no separate CSXAML test DSL; instead, the preferred direction is ordinary C# test projects, early automation-property support through the attached-property milestone, predictable cross-project/test-project generation behavior, and a hostless component testing harness built on the logical tree/runtime path.
- 2026-04-13: External control interop is being treated as an explicit make-or-break v1 requirement. The intended author-facing direction is C#-style file-level `using` directives and aliases, backed by referenced-assembly metadata generation plus generated or explicitly registered runtime adapter support rather than bespoke per-app glue.
- 2026-04-13: Local helper code in `.csxaml` is now being treated as a core readability requirement, not a nice-to-have. The intended direction is one component declaration per file plus ordinary local helper code and file-local helper types, using the same `namespace`/`using` model across components, external controls, and attached-property owners.
- 2026-04-13: The v1 plan is intentionally steering away from framework sprawl. Styling should stay thin over WinUI styles/resources, and lifecycle/async work should land as a minimal explicit model rather than a custom effect system.
- 2026-04-13: Milestone 6 completed. The prototype now supports `Grid`, `ScrollViewer`, common layout properties, owner-qualified attached properties for the initial built-in slice (`Grid.*`, `AutomationProperties.Name`, `AutomationProperties.AutomationId`), component-usage attached-property carry-through, and a two-pane Todo demo that exposes semantic automation hooks for future C#-first component tests. Runtime coverage includes logical-tree and reconciliation proofs in all environments plus direct WinUI projection tests that go inconclusive when the host machine cannot activate WinUI controls.
- 2026-04-13: Milestone 7 completed. CSXAML now supports file-level `using` directives and aliases for external controls, deterministic referenced-assembly discovery, generated runtime registration for supported external controls, and end-to-end demo proof with both a package-provided `InfoBar` and a solution-local `StatusButton`. The current slice intentionally supports recognized writable dependency properties plus recognized simple writable CLR properties, with direct `Action`-style event projection and deterministic diagnostics for unsupported shapes.
- 2026-04-13: Added projected WinUI retention tests that exercise real `TextBox` focus and selection preservation across ordinary parent rerenders, controlled text updates, and sibling keyed-list reorders. This closes a prior proof gap in Milestones 4 and 5, where the runtime behavior existed but the strongest assertions still lived mostly in fake-host coverage.
- 2026-04-13: Wrote `docs/external-control-interop.md` and aligned the language spec plus roadmap checklists with the actual supported external-control slice: normal `using` imports and aliases, deterministic referenced-assembly metadata discovery, generated runtime registration, current property/event/child-content limits, and explicit non-goals that still remain outside the v1 promise.
- 2026-04-13: Milestone 8 completed. Built-in and supported external controls now accept `Style` values through shared metadata and runtime coercion, the demo reuses keyed application styles through ordinary CSXAML expressions, and hostless tests assert deferred style intent without requiring live WinUI activation. The styling story remains intentionally thin: WinUI styles/resources and typed C# helpers are the reuse mechanism, not a new CSXAML-specific styling DSL.
- 2026-04-13: Milestone 9 completed. `.csxaml` files now support one file-scoped `namespace`, file-local helper declarations, and component-local helper code before the final render return. Components can accept explicit default child content through a bare `<Slot />` outlet, while named slots and root-level slot pass-through remain intentionally deferred. The demo now uses named local helper functions plus a reusable slotted `TodoPanel`, and hostless generator/runtime tests cover helper-code scanning, slot validation, emitted child-content transport, keyed identity retention through wrapper composition, and wrapper rerender stability.
- 2026-04-13: Milestone 10 completed. CSXAML now uses shared repo-level build assets for opt-in generation, deterministic project-default and file-scoped component namespaces, deterministic internal generated namespaces, explicit generated component manifests for referenced-assembly discovery, write-if-changed plus stale-output pruning under `obj`, and fixture proof for both library-to-library component imports and ordinary test-project consumption through normal `ProjectReference` usage. Clean/rebuild behavior is covered by the shared intermediate-root cleanup path plus output-writer regression coverage for shrinking generated sets.
