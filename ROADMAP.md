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
- source-first diagnostics for parser/validator failures, `#line`-mapped build errors from direct C# regions, deterministic source-map sidecars under `obj`, and wrapped runtime exception context

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
- app-level services can flow into components through an explicit DI model that stays separate from props and local state
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

    render <StackPanel>
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
    render <StackPanel>
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

    render <StackPanel>
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

render <WinUi:InfoBar
    Severity={WinUi.InfoBarSeverity.Warning}
    IsOpen={ShowWarning}
    Message="Unsaved changes" />;
```

### Local or referenced custom control

```csharp
using Controls = MyApp.Controls;

render <Controls:UserAvatar
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

    render <TodoEditor Title={selected.Title} Header={HeaderText()} />;
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
- [x] support local variables and local functions before the final render statement
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

**Status:** [x]

## Status note

Milestone 11 is now landed for the current Visual Studio 18 host path.

- `Csxaml.Tooling.Core`, `Csxaml.LanguageServer`, and `Csxaml.VisualStudio` now form one shared authoring stack rather than a Visual-Studio-only semantic implementation
- the extension activates in Visual Studio 18 experimental instances with the VSIX/runtime-host/bootstrap issues resolved
- semantic coloring, tag and attribute completion, projected C# completion, diagnostics, formatting, and go-to-definition are all available through the shared language-server boundary
- repo coverage now includes tooling-core behavior tests, protocol-level language-server smoke tests, and VSIX packaging tests for both runtime targeting and language-server payload contents

Remaining work around richer source mapping, readable build/runtime failure translation, and explicit debugging workflows belongs to Milestone 12 rather than keeping Milestone 11 open

## Purpose

Provide a first-class authoring experience in Visual Studio without trapping CSXAML language intelligence in a Visual-Studio-only implementation.

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

- [x] add a shared tooling-core project for milestone 11 bootstrap work
- [x] add an initial Visual Studio host project and VSIX packaging path
- [x] prove an experimental-instance bootstrap loop that stages the built extension and launches Visual Studio
- [x] stand up a shared language-server or equivalent shared request boundary so Visual Studio lands first without forcing a second semantic implementation for VS Code later
- [x] define tooling-facing metadata API
- [x] expose native control metadata to tooling layer
- [x] expose imported external control metadata and import-resolution context to the tooling layer
- [x] expose component symbol metadata to tooling layer
- [x] implement syntax highlighting
- [x] implement completion for tags
- [x] implement completion for props and events
- [x] implement completion for component props
- [x] implement editor diagnostics
- [x] implement baseline formatting and indentation rules for mixed markup/C# files
- [x] implement basic navigation or go-to-definition

---

# Milestone 12 - Diagnostics, Source Mapping, and Debugging

**Status:** [x]

## Status note

Milestone 12 is now landed.

- generator diagnostics now carry richer source spans instead of only point locations
- validator and tag-resolution failures now provide targeted suggestions for close native props, component props, and visible tag names
- emitted components now carry deterministic source-map sidecars under `obj\<tfm>\Csxaml\Maps` plus narrow `#line` coverage for direct source-authored C# regions
- representative helper-code and expression-island compiler failures now map back to `.csxaml`
- runtime failures now wrap with staged component/file/tag/member context through `CsxamlRuntimeException`
- the debugging workflow is documented in [docs/debugging-and-diagnostics.md](docs/debugging-and-diagnostics.md)

Remaining unmapped cases are intentionally narrow: internal scaffold or framework-contract compile failures may still remain on generated `.g.cs`, and the sidecar map is the truthful fallback breadcrumb for those cases rather than a fake precise remap

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

- [x] improve source-span tracking in parser and emitter
- [x] map diagnostics back to `.csxaml`
- [x] add helpful suggestion messages where possible
- [x] improve runtime exception context
- [x] define source mapping format or strategy
- [x] document debugging workflow

---

# Milestone 13 - Stabilization, Compatibility, and Test Hardening

**Status:** [x]

## Purpose

Turn the framework from a moving prototype into something teams can depend on, including an explicit automated component testing story and a minimal lifecycle/disposal model.

## Why it matters

Production readiness is as much about stability and change control as features. Real components need a boring, explicit answer for subscriptions, async work, and cleanup.

## Representative outcomes

- syntax stability promises
- documented supported control set
- compatibility policy for generator/runtime changes
- explicit lifecycle/disposal guidance that avoids framework magic
- explicit component service injection that builds on `IServiceProvider` / `Microsoft.Extensions.DependencyInjection` instead of a CSXAML-specific container
- C#-first component testing APIs that do not require generated-code spelunking
- broad regression coverage

## Dependency injection direction

CSXAML should support dependency injection, but it should do so in a way that keeps the line between props, state, and services easy to see.

The intended v1 direction is:

- props remain the way parent components pass render data and callbacks
- `State<T>` remains the way a component owns mutable UI state
- DI is reserved for app services such as repositories, clocks, navigation, dialogs, logging, or other long-lived collaborators
- the runtime should build on the normal .NET `IServiceProvider` story rather than inventing a second container abstraction
- component activation should become explicitly service-aware, but rendering should not turn into ad hoc service-locator calls on every rerender

Representative authoring target:

```csharp
component Element TodoBoard {
    inject ITodoRepository Todos;
    inject IClock Clock;

    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(LoadInitialItems());

    List<TodoItemModel> LoadInitialItems()
    {
        return Todos.LoadAll(Clock.Now);
    }

    render <StackPanel>
        ...
    </StackPanel>;
}
```

Representative host/testing target:

```csharp
var services = new ServiceCollection()
    .AddSingleton<ITodoRepository, TodoRepository>()
    .AddSingleton<IClock, SystemClock>()
    .BuildServiceProvider();

var host = new CsxamlHost(ComponentHost, typeof(TodoBoardComponent), services);
host.Render();
```

The preferred implementation shape is intentionally narrow:

- add optional `IServiceProvider` plumbing through `CsxamlHost`, the tree coordinator, and child-component activation
- replace raw `Activator.CreateInstance` child creation with an explicit component activator that can use `ActivatorUtilities`
- keep the existing root-instance path for simple demos and tests that do not use DI
- add explicit component-level `inject Type Name;` declarations near `State<T>` declarations rather than hiding DI inside markup attributes or helper-code conventions; this is now the intended v1 spec surface
- generate constructor-injected readonly members or equivalent cached component members so injected services are resolved once per component instance, not once per render
- keep DI scoped to components for v1; external controls should continue to follow the explicit supported-constructor rules already documented for interop

Intentional non-goals for the first DI slice:

- no implicit "props from DI" behavior
- no property injection or magical member scanning
- no per-component child scopes
- no subtree provider overrides in markup
- no requirement that every app use DI just to mount a component
- no external-control constructor injection story in v1

## Exit criteria

- compatibility policy exists
- supported feature matrix exists
- lifecycle/async/disposal model exists and is documented
- components can consume registered app services through an explicit DI model that preserves the distinction between props, state, and services
- developers can write automated component tests against CSXAML components without manually booting a full app when logical-tree testing is sufficient
- regression suite covers major language/runtime features
- breaking changes are documented and versioned intentionally

## Checklist

- [x] define syntax compatibility policy
- [x] define runtime/generator compatibility policy
- [x] publish supported control/feature matrix
- [x] define the minimal lifecycle/disposal model and keep it smaller than a custom effect framework
- [x] plumb optional `IServiceProvider` support through the host, tree coordinator, and component activation path
- [x] replace raw child-component activation with an explicit activator that can use `ActivatorUtilities`
- [x] define and implement explicit `inject Type Name;` declarations as the first DI authoring syntax
- [x] define root-host scope ownership and disposal behavior without introducing per-component scopes
- [x] define async state-update behavior and unmounted-component behavior
- [x] add a hostless C#-first component testing harness over the logical tree/runtime coordinator path
- [x] add semantic query helpers that prefer text, name, and automation metadata over brittle child-index assertions
- [x] add interaction helpers for common component tests such as click, text input, and checked-state changes
- [x] expand golden tests for generator output
- [x] expand runtime reconciliation regression tests
- [x] add DI regression tests for required service resolution, missing-service failures, keyed child reuse, and test-time service overrides
- [x] add regression tests for mount/unmount cleanup and async completion safety
- [x] add interop regression tests, including referenced package and solution-local controls
- [x] document known limitations clearly

## Notes

- Milestone 13 adds `Csxaml.Testing` as the supported hostless logical-tree test surface.
- Runtime DI now flows through `IServiceProvider`, an explicit activator seam, and `inject Type name;` generated resolution hooks.
- The Todo demo now dogfoods the DI path end to end: `App` builds a standard `ServiceCollection`, `MainWindow` mounts `TodoBoardComponent` through the root-type host path, and `TodoBoard` injects `ITodoService` for initial load plus snapshot persistence through `SaveItems(...)` while local `State<>` remains the live UI source of truth.
- Lifecycle remains intentionally small: mount-once notification, subtree disposal, and post-unmount invalidation no-op behavior are implemented and documented.
- Dedicated lifecycle authoring syntax inside `.csxaml` source remains intentionally out of scope; the runtime hook exists for handwritten `ComponentInstance` types.

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
- [x] write debugging and diagnostics guide
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
- [ ] dependency injection becomes a second ambient props channel instead of a narrow app-service boundary
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
- 2026-04-13: Milestone 9 completed. `.csxaml` files now support one file-scoped `namespace`, file-local helper declarations, and component-local helper code before the final render statement. Components can accept explicit default child content through a bare `<Slot />` outlet, while named slots and root-level slot pass-through remain intentionally deferred. The demo now uses named local helper functions plus a reusable slotted `TodoPanel`, and hostless generator/runtime tests cover helper-code scanning, slot validation, emitted child-content transport, keyed identity retention through wrapper composition, and wrapper rerender stability.
- 2026-04-13: Milestone 10 completed. CSXAML now uses shared repo-level build assets for opt-in generation, deterministic project-default and file-scoped component namespaces, deterministic internal generated namespaces, explicit generated component manifests for referenced-assembly discovery, write-if-changed plus stale-output pruning under `obj`, and fixture proof for both library-to-library component imports and ordinary test-project consumption through normal `ProjectReference` usage. Clean/rebuild behavior is covered by the shared intermediate-root cleanup path plus output-writer regression coverage for shrinking generated sets.
- 2026-04-14: Milestone 11 is now in progress. The repo contains a first `Csxaml.Tooling.Core` bootstrap slice with unit coverage, a `Csxaml.VisualStudio` host that builds a real `.vsix`, and a documented/scripted experimental-instance loop that stages the built extension into the Visual Studio `Exp` hive and launches the repo solution there. IntelliSense is not closed yet, and version-18 host activation still needs explicit validation before the bootstrap gap can be treated as closed.
- 2026-04-14: Milestone 11 planning now explicitly treats Visual Studio as the first client over a shared language-service boundary so future VS Code reuse does not require re-implementing completion, diagnostics, navigation, or formatting semantics in a second stack.
- 2026-04-14: Milestone 11 implementation is broader than the bootstrap slice now in the repo: `Csxaml.LanguageServer` exists over `Csxaml.Tooling.Core`, the Visual Studio host packages that server into a real `.vsix`, and the codebase contains completion, navigation, formatting, semantic-coloring, and source-diagnostic paths. The milestone remains in progress until the Visual Studio 2026 extension host reliably activates that package and those features are re-verified end to end.
- 2026-04-14: Milestone 11 also added projected C# editor semantics for helper code, state declarations, control-flow expressions, and markup expression islands. Tooling now maps those regions into synthetic C# documents backed by Roslyn compilation so helper/local symbols, member access, and C# errors surface through the same shared language-service boundary instead of editor-specific heuristics.
- 2026-04-14: Milestone 11 bootstrap hardening now forces `devenv /RootSuffix Exp /UpdateConfiguration`, clears stale experimental-instance caches, and emits both an activity log and a lightweight extension startup log. That supportability path matters because new `.csxaml` document-type and language-server contributions can otherwise be present on disk but invisible to Visual Studio until the Exp hive is refreshed.
- 2026-04-14: Milestone 11 debugging uncovered a second host-activation trap after the package itself began loading: Visual Studio 18 was injecting its private `.NET 8` `DOTNET_ROOT`, which caused the framework-dependent `Csxaml.LanguageServer` `net10.0` executable to die immediately with a `ConnectionLostException` in the editor. The Visual Studio host now resolves a compatible runtime root from the language server's `.runtimeconfig.json` and prefers a matching machine-wide `C:\Program Files\dotnet` install when the injected Visual Studio root is incompatible.
- 2026-04-14: Milestone 11 editor diagnostics now honor the metadata-driven CSXAML coercions for brush, style, and thickness values inside projected C# documents. This closes a false-positive tooling gap where valid expressions such as `ArgbColor` backgrounds, `DeferredStyle` style values, and numeric thickness shorthands compiled and rendered correctly at runtime but still showed type-conversion squiggles in Visual Studio.
- 2026-04-14: Milestone 11 bootstrap packaging now rewrites the generated VSIX manifest to declare `DotnetTargetVersions` as `net8.0;net10.0`, and a focused manifest test now guards that output. This closes a Visual Studio 18 extension-manager warning where the package appeared to support only `net8.0` even though the host can run on both `.NET 8` and `.NET 10`.
- 2026-04-14: Repo-level restore configuration now includes `NuGet.org` again instead of only the local cache plus the Visual Studio offline feed. This closes a project-system regression where Visual Studio and ordinary repo restores could fail on newer `.NET 10` runtime-pack patch versions such as `10.0.5` even though command lines that manually appended `https://api.nuget.org/v3/index.json` succeeded.
- 2026-04-14: Milestone 11 is now considered complete for the current Visual Studio 18 path. The shared tooling core, language server, and VSIX host now cover semantic coloring, completion, diagnostics, formatting, and component-definition navigation, and the repo now carries protocol-level plus VSIX-packaging regression coverage so the milestone closes on proof rather than screenshots.
- 2026-04-14: Dependency injection is now being treated as part of the v1 runtime/testing story rather than an app-level workaround. The preferred direction is a narrow `IServiceProvider`-based model: service-aware host/component activation, explicit `inject Type Name;` declarations for components, cached per-instance service resolution, and no drift toward props-from-DI, per-component scopes, or a second framework-owned container.
- 2026-04-14: Milestone 12 completed. Generator diagnostics now carry richer spans and suggestions, emitted components now produce deterministic `obj\<tfm>\Csxaml\Maps\*.map.json` sidecars plus narrow `#line` mappings for direct source-authored C# regions, representative build failures land back on `.csxaml`, runtime failures wrap with staged component/tag/member context, and the debugging workflow is documented in `docs/debugging-and-diagnostics.md`.
- 2026-04-15: The language spec now defines DI more explicitly: component-scoped services use dedicated `inject Type Name;` declarations in the component prologue, remain separate from props and markup, resolve once per component instance from the ambient service provider, and intentionally exclude markup injection, attribute scanning, and service-locator-first patterns from the intended v1 model.
- 2026-04-15: The Todo demo now exercises the DI slice end to end. `App` builds a normal `ServiceCollection`, `TodoBoard` consumes `inject ITodoService todoService;`, and runtime demo tests verify both injected seed data and persisted edits through that service boundary.
- 2026-04-15: The demo todo service boundary now treats the component as the owner of live UI state again. `ITodoService` exposes `SaveItems(...)` for persistence, while `TodoBoard` keeps one local `UpdateItem(itemId, updater)` helper and persists updated snapshots instead of treating the service as an item-level store.
- 2026-04-15: Followed the language-spec clarification pass with a focused conformance hardening slice. Runtime `State<T>` invalidation is now assignment-driven even for equal/reference-equal assignments, render-phase state writes now fail with a clear non-reentrant runtime error instead of recursively reentering rendering, and simple tag resolution in both generator and tooling now reports ambiguity when a built-in/native tag collides with a visible component instead of silently preferring the native control.
- 2026-04-15: Hardened the spec-facing source contract and repo examples around three realities that were previously too fuzzy: the final render statement now uses the dedicated `render <Root />;` syntax across parser/tooling/demos/docs, the WinUI/runtime posture now explicitly documents controlled input, retained native reconciliation, lifecycle cleanup, event rebinding, theme/resource limits, and trim/AOT intent, and the broader attached-property-owner import story remains called out as a known implementation gap instead of being implied as already complete.
- 2026-04-15: Replaced the old final-markup `return` forms with a dedicated `render` statement. The parser, formatter, semantic tokens, VS Code grammar/snippets, demo components, and regression fixtures now treat `render <Root />;` as the only valid final markup statement, while both `return <Root />;` and `return ( <Root /> );` are rejected with targeted guidance.
- 2026-04-15: Tightened the contract further around the places where retained-mode UI gets sharp edges. The spec now makes duplicate sibling key rejection explicit, clarifies that preserved instances rerender with updated props/child content and the same resolved injected bindings, states that state initializers run once per instance creation, strengthens the compile pipeline from architecture guidance into contract language, and documents current render/projection failure semantics including aborted passes and lack of transactional native rollback. Runtime reconciliation now also rejects duplicate keyed component siblings deterministically instead of leaving that path implicit.
- 2026-04-15: Replaced the repo-root `plan.md` with a smaller post-`render` spec-tightening plan focused on the next contract gaps: source-level `State<T>` semantics versus raw C# field-initializer expectations, compile-time versus runtime render-phase state-write diagnostics, controlled-input comparison/IME behavior, slot-placement rules, dispatcher/synchronous-DI wording, and the remaining file-surface cleanup around `using static`, `component Element`, and app-shell posture.
- 2026-04-15: Tightened that repo-root plan again after the next review pass. `plan.md` now also tracks helper-declaration status as an explicit v1-contract question, compile-time versus runtime duplicate-key failure phrasing, render-parser examples and rationale cleanup, the event-normalization contract surface, failure/disposal precision, and a small audit to keep support-slice/product-posture material from sprawling through the core language sections.
- 2026-04-15: Tightened `plan.md` again from a compiler-engineering perspective. The active plan now explicitly tracks bounded-island lexing realism for modern C# string forms, the need for specialized interactive-control adapters rather than naive generic property patching, and the fact that virtualization, `DataContext` interop for third-party controls, and named slots are deferred areas that also function as real production-risk gaps rather than harmless future polish.
- 2026-04-15: The repo-root `plan.md` is now also an execution document rather than only a direction document. It includes codebase touchpoints, per-workstream closure rules, an implementation issue log, concrete regression commands, and a final agent sign-off checklist so future spec-tightening work can be tracked through parser, runtime, tooling, tests, docs, and roadmap updates without leaving silent drift behind.
- 2026-04-15: Executed the final pre-1.0 language-spec tightening pass. `LANGUAGE-SPEC.md` now carries a maintained revision log, file-local helper declarations are stated as part of the v1 surface rather than an aspiration, `using static` is implemented and tested as ordinary C# lookup only, slot outlets are rejected inside `foreach`, bounded-island lexical promises are backed by raw-string regression coverage, and the controlled-input/`DataContext`/virtualization wording now reflects the current retained WinUI runtime more honestly.
