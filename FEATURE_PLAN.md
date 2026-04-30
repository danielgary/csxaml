# Feature Plan: WinUI Interop and Authoring Breadth

## Status

Drafted on 2026-04-29. Updated the same day with the generated root strategy
for `Application`, `Window`, `Page`, and `ResourceDictionary`, then tightened
with an explicit documentation and status-update discipline. Updated on
2026-04-30 after Phase 05 property-content and named-slot implementation,
then again after the Phase 06 generated `Page` / `Window` root foundation,
and after Phase 07 generated application mode.

This plan covers a post-v1 feature track for the next layer of CSXAML WinUI
interop:

- typed event-argument projection for common WinUI events
- element refs for imperative WinUI interop
- external-control content-property awareness
- property-content syntax and named slots
- broader attached-property metadata
- generated `Application`, `Window`, `Page`, and `ResourceDictionary` roots
- template/resource interop guidance
- large-list virtualization guidance
- CSXAML highlighting in sample presenters and fixtures

The goal is not just more API coverage. The goal is a broader authoring model
that still feels like the same framework.

## Product Direction

CSXAML should stay component-first, C#-native, and WinUI-aligned.

That means:

- behavior is ordinary C# delegates, lambdas, methods, and typed objects
- WinUI controls, properties, events, and attached properties keep recognizable
  names
- richer object/property shapes use metadata, not runtime guessing
- syntax should teach one pattern once and then reuse it
- tooling support ships with the language surface, not after it
- docs should tell authors when CSXAML is the right tool and when native WinUI
  XAML/resource/template machinery is still the better tool

The consistent feel should be:

```csxaml
using Controls = Microsoft.UI.Xaml.Controls;

component Element SearchPage {
    State<string> Query = new State<string>(string.Empty);
    ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

    render <Grid>
        <TextBox
            Ref={SearchBox}
            Text={Query.Value}
            OnTextChanged={value => Query.Value = value}
            OnKeyDown={args => SubmitOnEnter(args)}
            OnLoaded={_ => SearchBox.Current?.Focus(FocusState.Programmatic)} />

        <Controls:AutoSuggestBox
            QueryText={Query.Value}
            OnQuerySubmitted={args => Submit(args.QueryText)}
            OnSuggestionChosen={args => Choose(args.SelectedItem)} />

        <Button Content="More">
            <Button.Flyout>
                <MenuFlyout>
                    <MenuFlyoutItem Text="Refresh" />
                </MenuFlyout>
            </Button.Flyout>
        </Button>
    </Grid>;
}
```

The example is aspirational for this feature track, but the important part is
the shape:

- events are `OnEventName`
- event arguments are typed C# values
- imperative handles are explicit `ElementRef<T>` values
- non-attribute object content uses XAML-familiar property elements
- no string handler names, hidden `DataContext` binding, or CSXAML-owned
  resource lookup language appears

## Current Baseline

The repo already has the right foundations:

- `EventMetadata` describes exposed event names, handler types, and binding
  kinds.
- `EventBindingKind` currently supports direct `Action`, text value changes,
  and bool value changes.
- built-in `TextBox` and `CheckBox` already prove normalized value events.
- external controls are discovered from referenced assemblies and registered
  through generated metadata.
- child content is currently `None`, `Single`, or `Multiple`.
- external child detection currently looks for `Panel`, `Children`, `Child`,
  `Content`, and a few built-in base shapes.
- default slots exist for CSXAML components; named slots are intentionally
  rejected today.
- attached properties are metadata-driven for the current `Grid` and
  `AutomationProperties.Name` / `AutomationProperties.AutomationId` slice.
- docs and VS Code already share one TextMate grammar for `.csxaml`.
- performance docs already state that `foreach` is not virtualization.

This track should extend those foundations instead of replacing them.

## Compatibility Rules

Keep these source rules stable while expanding the surface:

- Existing `OnClick={Save}` command-style handlers remain valid.
- Existing `OnTextChanged={text => ...}` and `OnCheckedChanged={value => ...}`
  remain value-normalized events, not raw platform event args.
- Existing default slots remain valid.
- Existing native child content remains valid.
- Existing attached-property owner lookup continues to use ordinary `using`
  visibility and type aliases.
- Existing XAML resource dictionaries remain first-class. CSXAML should not
  force apps to rewrite templates or resources.

When a feature introduces a richer form, it should fail closed with a precise
diagnostic rather than silently falling back to reflection or loose object
assignment.

## Documentation And Status Discipline

Every implementation change in this feature track must update the public and
project-facing truth in the same change. A feature is not done when the code
works; it is done when developers can discover the behavior, understand its
status, and see its limits without reading generated code.

Required status surfaces:

- `LANGUAGE-SPEC.md` for source syntax, semantics, compatibility rules, and
  intentional non-goals
- `ROADMAP.md` for milestone status, notes, blockers, and scope changes
- `docs/supported-feature-matrix.md` for supported, preview, experimental, and
  deferred feature status
- DocFX pages under `docs-site/articles/**` for author-facing guidance
- root `README.md` when the getting-started or product-positioning story
  changes
- samples and templates when the recommended app shape changes
- tests or docs-render checks that prove examples stay current

The minimum per-change checklist is:

- update the implementation status table below
- update the relevant roadmap milestone or notes log
- update the supported-feature matrix when status changes
- update the language spec before or with syntax/semantic changes
- update docs and samples before declaring the slice complete
- document any new limitation, deferred case, or migration concern

## Implementation Status Table

| Feature slice | Status | Required status surfaces |
| --- | --- | --- |
| [Phase 02: Typed event-argument projection](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_02.md) | Experimental | Spec event section, roadmap, feature matrix, native events guide, tooling docs |
| [Phase 03: Element refs](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_03.md) | Experimental | Spec runtime/app interop section, roadmap, feature matrix, native interop guide, testing docs |
| [Phase 04: Content-property awareness](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_04.md) | Experimental | Spec external-control section, roadmap, feature matrix, external-control guide |
| [Phase 05: Property-content syntax and named slots](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_05.md) | Experimental | Spec grammar/slots sections, roadmap, feature matrix, syntax docs, component model docs |
| [Phase 06: Generated page/window root foundation](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_06.md) | Experimental | Spec source-file/app model sections, roadmap, feature matrix, getting-started docs, runtime API docs |
| [Phase 07: Full generated application mode](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_07.md) | Experimental | Spec app model sections, roadmap, feature matrix, getting-started docs, generated app fixture |
| [Phase 08: Broader attached-property metadata](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_08.md) | Experimental | Spec attached-property section, roadmap, feature matrix, native props/events guide |
| [Phase 09: Resource/template guidance](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_09.md) | Experimental | Spec WinUI interop/resource posture, roadmap, feature matrix, resources/templates guide |
| [Phase 10: List virtualization guidance](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_10.md) | Experimental | Spec scale posture, roadmap, feature matrix, performance guide |
| [Phase 11: CSXAML highlighting in sample presenters and fixtures](plans/FEATURE_PLAN/EXECUTION_PACKAGE_PHASE_11.md) | Experimental | Roadmap, feature matrix, editor docs, docs build checks, fixture tests |

## Execution Packages

Detailed phase instructions live under `plans/FEATURE_PLAN/`:

- `EXECUTION_PACKAGE_PHASE_01.md`: status alignment and feature contract
- `EXECUTION_PACKAGE_PHASE_02.md`: typed event-argument projection
- `EXECUTION_PACKAGE_PHASE_03.md`: element references
- `EXECUTION_PACKAGE_PHASE_04.md`: content-property metadata discovery
- `EXECUTION_PACKAGE_PHASE_05.md`: property-content syntax and named slots
- `EXECUTION_PACKAGE_PHASE_06.md`: generated page and window root foundation
- `EXECUTION_PACKAGE_PHASE_07.md`: full generated application mode
- `EXECUTION_PACKAGE_PHASE_08.md`: broader attached-property metadata
- `EXECUTION_PACKAGE_PHASE_09.md`: resource and template guidance
- `EXECUTION_PACKAGE_PHASE_10.md`: list virtualization guidance
- `EXECUTION_PACKAGE_PHASE_11.md`: CSXAML highlighting in sample presenters and fixtures

These packages are execution instructions. They do not mean the behavior has
shipped; the status table above remains authoritative for implementation state.

## Slice 1: Typed Event-Argument Projection

**Phase 02 status:** Experimental implementation landed for the documented
common WinUI event-args set and supported external `EventHandler<TEventArgs>`
events. Broader/open-ended event projection remains outside the v1 promise.

### User Experience

Expose common WinUI events as senderless, typed `Action<TEventArgs>` handlers:

```csxaml
<ListView OnSelectionChanged={args => Select(args.AddedItems)} />
<Slider OnValueChanged={args => Volume.Value = args.NewValue} />
<AutoSuggestBox OnQuerySubmitted={args => Search(args.QueryText)} />
<TextBox OnKeyDown={args => SubmitOnEnter(args)} />
<Border OnPointerPressed={args => StartDrag(args)} />
<Frame OnNavigated={args => TrackPage(args.SourcePageType)} />
```

Use the same event names a XAML author expects, prefixed with `On`.

Recommended delegate shape:

| CSXAML event | Delegate shape |
| --- | --- |
| `OnLoaded` | `Action<RoutedEventArgs>` |
| `OnSelectionChanged` | `Action<SelectionChangedEventArgs>` |
| `OnValueChanged` | `Action<RangeBaseValueChangedEventArgs>` |
| `OnItemClick` | `Action<ItemClickEventArgs>` |
| `OnQuerySubmitted` | `Action<AutoSuggestBoxQuerySubmittedEventArgs>` |
| `OnSuggestionChosen` | `Action<AutoSuggestBoxSuggestionChosenEventArgs>` |
| `OnKeyDown` | `Action<KeyRoutedEventArgs>` |
| pointer events | `Action<PointerRoutedEventArgs>` |
| frame navigation events | `Action<NavigationEventArgs>`, `Action<NavigatingCancelEventArgs>`, `Action<NavigationFailedEventArgs>`, or `Action<NavigationStoppedEventArgs>` as appropriate |

The handler receives event args only. Sender access should come through
`ElementRef<T>` when authors need the element. This keeps event delegates
ergonomic and avoids pushing authors toward `(sender, args)` boilerplate in
markup.

### Design

Add one explicit event projection shape for platform event args.

Recommended metadata direction:

- keep `EventMetadata.ExposedName`
- keep `EventMetadata.HandlerTypeName`
- add an event binding kind such as `EventArgs`
- keep normalized value events as explicit binding kinds
- keep raw/direct `Action` command events as explicit binding kinds

Do not infer projections from event names alone. The metadata generator should
decide that a WinUI event is supported and record the exact projection shape.

For external controls, support event-args projection only when the CLR event is
simple and public:

- return type is `void`
- parameters are not by-ref
- the final event-args parameter type is public
- the generated CSXAML handler type is deterministic

External controls should not get automatic value normalization. A `ValueChanged`
event on an external control can project event args, but it should not become
`Action<double>` unless metadata or an explicit adapter says so.

### Implementation Notes

Keep runtime work small and explicit:

- add a reusable native event binder for `(sender, args) => handler(args)`
- keep `Button.OnClick` as the no-argument command pattern
- add focused built-in adapter support for controls that need special handling
- extend external event binding beyond `Action` only after metadata can express
  the exact handler type
- keep event rebinding through `NativeEventBindingStore`

Initial built-in control/event set:

- `FrameworkElement.Loaded`
- `Selector.SelectionChanged` consumers such as `ListView`, `GridView`,
  `ComboBox`, and `NavigationView` where supported
- `RangeBase.ValueChanged` consumers such as `Slider` and `ProgressBar` where
  meaningful
- `ListViewBase.ItemClick`
- `AutoSuggestBox.QuerySubmitted`
- `AutoSuggestBox.SuggestionChosen`
- `UIElement.KeyDown`
- common `UIElement` pointer events:
  `PointerPressed`, `PointerReleased`, `PointerMoved`, `PointerEntered`,
  `PointerExited`, `PointerCanceled`, `PointerCaptureLost`, and
  `PointerWheelChanged`
- `Frame.Navigating`, `Navigated`, `NavigationFailed`, and
  `NavigationStopped`

### Tests And Tooling

Add tests in the same layer that owns the behavior:

- metadata generator tests for every curated event
- generator emission tests proving handler casts use `Action<TEventArgs>`
- runtime adapter tests proving event args flow and handlers rebind cleanly
- external-control tests for a sample typed event
- tooling completion and hover tests showing delegate shapes
- source-mapped compile diagnostics for invalid handler lambdas

## Slice 2: Element References

**Phase 03 status:** Experimental implementation landed for `ElementRef<T>` and
`Ref={...}` on native elements, including built-in and registered external
native controls. Component refs and `x:Name` symbolic lookup remain out of
scope.

### User Experience

Add a small runtime type and a reserved native attribute:

```csxaml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();
ElementRef<ScrollViewer> ScrollHost = new ElementRef<ScrollViewer>();

render <StackPanel>
    <TextBox Ref={SearchBox} />
    <ScrollViewer Ref={ScrollHost}>
        <StackPanel />
    </ScrollViewer>
    <Button
        Content="Focus"
        OnClick={() => SearchBox.Current?.Focus(FocusState.Programmatic)} />
</StackPanel>;
```

Recommend `Ref`, not `x:Name`, as the first-class CSXAML feature.

Reasons:

- `Ref` is a value, so lifetime is visible in normal C# code.
- It avoids generated partial fields with surprising component scope.
- It works for external controls and retained native controls.
- It pairs naturally with typed event args where sender is omitted.
- It makes unmount behavior explicit: the ref can be cleared.

Native `Name` can still become an ordinary supported property later, but it
should not be the main imperative interop mechanism.

### Design

Add `ElementRef<TElement>` to `Csxaml.Runtime`.

Suggested API:

```csharp
public sealed class ElementRef<TElement>
    where TElement : class
{
    public TElement? Current { get; }
    public bool TryGet([NotNullWhen(true)] out TElement? element);
}
```

Runtime-owned methods can set and clear the value internally.

Semantics:

- `Ref` is valid only on native elements in the first slice.
- `Ref` does not cause rerender.
- `Ref` is assigned after native element creation.
- `Ref` remains stable across retained native rerenders.
- `Ref` clears when the native element is removed or the root is disposed.
- If the runtime creates an element that is not assignable to `TElement`, it
  throws a source-contextual runtime exception.

### Implementation Notes

Keep the compiler model explicit:

- parse `Ref` as a reserved native attribute, not a normal property
- emit it separately from `NativePropertyValue`
- store it on `NativeElementNode` as a dedicated value
- let `WinUiNodeRenderer` update refs when native projections are created,
  retained, replaced, or disposed
- add no support for component refs until a separate design decides whether
  they refer to component instances or rendered native roots

### Tests And Tooling

Required tests:

- parser/emitter tests for `Ref={SearchBox}`
- validation tests for string refs and invalid attribute placement
- runtime tests for assignment, retained rerender, replacement, removal, and
  root disposal
- external-control ref tests
- tooling completion for `Ref`
- hover text that explains lifetime and type expectations

## Slice 3: Content-Property Awareness For External Controls

**Phase 04 status:** experimental. Metadata now records default child-content
properties, including `[ContentProperty]` names, inherited content attributes,
built-in content conventions, and supported collection properties. Runtime
external-control child projection uses that metadata for default child content.
Property-content syntax such as `<ControlExample.Options>` remains Phase 05.

### User Experience

External controls with a `[ContentProperty]` should accept natural child
content even when the property is not named `Content`.

Examples:

```csxaml
<Gallery:ControlExample>
    <Gallery:ControlExample.Example>
        <Button Content="Run" />
    </Gallery:ControlExample.Example>
    <Gallery:ControlExample.Options>
        <StackPanel />
    </Gallery:ControlExample.Options>
</Gallery:ControlExample>
```

For default child content, a control that declares `[ContentProperty(Name =
"Example")]` can accept:

```csxaml
<Gallery:ControlExample>
    <Button Content="Run" />
</Gallery:ControlExample>
```

when metadata says `Example` is the default content property.

### Design

Replace the current `ControlChildKind`-only model with a richer but still small
content model.

Recommended metadata:

- default content property name, when one exists
- child content kind: none, single element, collection, or object
- whether named property-content elements are supported for a property
- target CLR type for the property value
- collection item type when known

The metadata generator should discover:

- `Microsoft.UI.Xaml.Markup.ContentPropertyAttribute`
- inherited content-property attributes
- `ContentControl.Content`
- `Border.Child`
- `ScrollViewer.Content`
- `Panel.Children`
- collection-style properties such as `NavigationView.MenuItems`

The runtime should set the named property directly through generated or cached
accessors. It should not assume every single-child external control uses
`Child` or `Content`.

### Tests

Add a solution-local external control fixture with:

- `[ContentProperty(Name = "Example")]`
- a second named UIElement property
- a collection property
- one inherited content property

Cover metadata discovery, validation, emitted nodes, runtime projection, and
tooling completion.

## Slice 4: Property-Content Syntax And Named Slots

**Phase 05 status:** Experimental implementation landed. CSXAML now parses,
validates, emits, renders, and tools XAML-familiar `<Owner.Property>` child
elements for metadata-backed native property content and component named slots.
Default slots remain compatible. Root-level property-content nodes, fallback
slot content, template authoring, and unsupported native property targets remain
out of scope.

### User Experience

Use one syntax for both native property content and component named slots:

```csxaml
<Button Content="Open">
    <Button.Flyout>
        <Flyout>
            <TextBlock Text="Details" />
        </Flyout>
    </Button.Flyout>
</Button>
```

```csxaml
component Element ControlExample {
    render <StackPanel>
        <Slot Name="Example" />
        <Slot Name="Options" />
    </StackPanel>;
}

component Element Demo {
    render <ControlExample>
        <ControlExample.Example>
            <Button Content="Run" />
        </ControlExample.Example>
        <ControlExample.Options>
            <CheckBox Content="Enabled" />
        </ControlExample.Options>
    </ControlExample>;
}
```

This intentionally mirrors XAML property elements. Authors should not need one
syntax for `Button.Flyout`, another for `NavigationView.MenuItems`, and a third
for component slots.

### Source Rules

Recommended rules:

- A child element named `ParentTag.Property` directly under `ParentTag` is a
  property-content element.
- The owner part must match the containing element's source tag name or the
  target component tag name.
- Property-content elements cannot appear as the root render node.
- Property-content elements cannot have `Key`, events, or attached properties.
- Property-content elements do not create native elements themselves.
- Attribute assignment and property-content assignment to the same property is
  a diagnostic unless metadata explicitly defines merge behavior.
- Single-value property content accepts at most one child node.
- Collection property content accepts multiple child nodes.
- Named component slot content preserves the caller's child order within that
  slot.
- Default slot content remains ordinary child content.

### Implementation Notes

Keep this split across small files:

- add `PropertyContentNode` to the AST
- add a parser helper that recognizes dotted child element names in child
  position
- add native validation for metadata-backed property content
- add component validation for named slot declarations and usage
- add an emitter path that keeps property content separate from normal children
- add runtime node support for native property content
- keep default-slot transport separate from named-slot transport

Do not make templates work by smuggling arbitrary markup through property
content. If a property expects a `DataTemplate`, this slice should either
require a C# expression or use the resource/template guidance below.

### Tests And Tooling

Tests should include:

- parsing nested property-content elements
- diagnostics for wrong owner, unknown property, duplicate single property,
  property content plus attribute collision, and root property content
- component named slot declaration and usage
- runtime projection for `Button.Flyout` or a controlled fixture equivalent
- `NavigationView.MenuItems` or fixture collection-property content
- completion for property-content names after typing `<Button.`
- semantic tokens for property-content owner and property names
- formatting that indents property-content blocks predictably

## Slice 5: Broader Attached-Property Metadata

**Phase 08 status:** Experimental broader attached-property metadata has
landed for `Canvas`, `RelativePanel`, `ToolTipService`,
`VariableSizedWrapGrid`, and the practical Gallery-facing
`AutomationProperties` slice. The matching layout controls are now part of the
built-in renderable control set, runtime application stays split by owner, and
tooling surfaces completion, hover, semantic tokens, and suggestion fixes for
the expanded metadata. External attached-property owner discovery remains
deferred.

### User Experience

Authors should be able to use the attached properties needed by normal WinUI
layout, accessibility, and Gallery samples:

```csxaml
<Canvas>
    <Button Canvas.Left={20} Canvas.Top={12} Content="Move" />
</Canvas>

<TextBlock
    AutomationProperties.Name="Total"
    AutomationProperties.HelpText="Shows the current total"
    Text={Total.Value.ToString()} />

<Border ToolTipService.ToolTip="Saved" />
```

### Metadata Scope

Initial owners:

- `Canvas`
- `RelativePanel`
- `ToolTipService`
- `VariableSizedWrapGrid`
- full practical `AutomationProperties` surface used by Gallery samples
- keep existing `Grid`

The owner list should be curated at first, but discovery should be systematic.

Metadata generation should recognize public attached properties from:

- public static `GetX` / `SetX` method pairs
- public static `XProperty` dependency-property fields or properties
- supported value kinds already used by normal properties

The model should keep:

- source owner name
- CLR owner type
- property name
- CLR value type
- value-kind hint
- optional required parent/control context
- dependency-property field name when available

### Runtime Design

Avoid one enormous attached-property switch.

Recommended file shape:

- one applicator per owner:
  - `CanvasAttachedPropertyApplicator`
  - `RelativePanelAttachedPropertyApplicator`
  - `ToolTipServiceAttachedPropertyApplicator`
  - `VariableSizedWrapGridAttachedPropertyApplicator`
  - expanded `AutomationPropertiesAttachedPropertyApplicator`
- a small shared conversion reader
- a small owner dispatcher

Clearing should use dependency-property defaults where possible:

- prefer `ClearValue(Owner.PropertyNameProperty)` when the dependency-property
  field is available
- use explicit owner defaults only when WinUI does not expose a clearable
  dependency property in the supported shape

### Tests And Tooling

Required tests:

- generated metadata contains expected owners and value types
- owner lookup still follows ordinary `using` and type aliases
- namespace aliases remain invalid attached-property owners
- validation enforces required parent context where metadata defines one
- runtime applies and clears each owner group
- completion and hover include the broader surface
- docs list supported owners without implying every WinUI attached property is
  supported forever

## Slice 6: Application, Window, Page, And ResourceDictionary Roots

**Phase 06 status:** Experimental generated `Page` and `Window` roots have
landed. They generate real WinUI shell types backed by retained CSXAML body
components, support limited `Window` title/size/backdrop declarations,
participate in component manifests, and can be launched from a handwritten app
shell. Generated
`Application`, generated entry point ownership, generated `ResourceDictionary`,
and `CsxamlApplicationMode=Generated` remain Phase 07 work.

**Phase 07 status:** Experimental generated application mode has landed.
`CsxamlApplicationMode=Generated` requires one `component Application`, rejects
`App.xaml` `ApplicationDefinition` conflicts, emits the WinUI entry point,
creates and activates the startup window, passes optional services into
generated windows/pages/components, and supports limited
`component ResourceDictionary` merged dictionaries. Advanced activation,
packaging, keyed resource authoring, templates, and broad lifecycle DSL remain
deferred.

### Problem

Today CSXAML can generate renderable element components, but a normal WinUI app
still needs XAML or handwritten C# roots around those components:

- `App.xaml` / `App.xaml.cs`
- `MainWindow.xaml` / `MainWindow.xaml.cs`
- optional `Page` XAML for `Frame` navigation
- XAML `ResourceDictionary` files for app resources and templates

That is a reasonable v1 boundary, but it makes the developer experience feel
split. A small app can author most of its UI in CSXAML, then immediately falls
back to XAML/code-behind for the app shell.

The solution should let a new CSXAML app delete the default WinUI shell files:

- no `App.xaml`
- no `App.xaml.cs`
- no `MainWindow.xaml`
- no `MainWindow.xaml.cs`

The project still needs ordinary WinUI project settings, package references,
and an app manifest when the Windows App SDK requires them. CSXAML should own
the source-level app shell, not replace the .NET project system or Windows
packaging model.

### User Experience

A new app should be able to start from a small set of `.csxaml` files:

```text
MyApp.csproj
app.manifest
App.csxaml
MainWindow.csxaml
AppResources.csxaml
Pages/HomePage.csxaml
```

`App.csxaml`:

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

`MainWindow.csxaml`:

```csxaml
component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;

    render <Frame SourcePageType={typeof(HomePage)} />;
}
```

`Pages/HomePage.csxaml`:

```csxaml
component Page HomePage {
    render <Grid>
        <TextBlock Text="Hello from a generated page." />
    </Grid>;
}
```

`AppResources.csxaml`:

```csxaml
component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>;
}
```

A project file should make the mode obvious but not noisy:

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
  <UseWinUI>true</UseWinUI>
  <EnableCsxaml>true</EnableCsxaml>
  <CsxamlApplicationMode>Generated</CsxamlApplicationMode>
</PropertyGroup>
```

For new starter templates, `Generated` should be the default. For existing
WinUI projects, `Hybrid` should remain the safe default so adopting CSXAML does
not delete or fight existing XAML roots.

### Root Kinds

Extend the existing declaration family with explicit root kinds:

```csxaml
component Element TodoCard(string Title) {
    render <TextBlock Text={Title} />;
}

component Page HomePage {
    render <TodoCard Title="Home" />;
}

component Window MainWindow {
    render <HomePage />;
}

component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <!-- Supported resources go here. -->
    </ResourceDictionary>;
}

component Application App {
    startup MainWindow;
    resources AppResources;
}
```

The exact `startup` and `resources` spelling should be finalized in the
language spec before implementation. The important direction is:

- use `component <Kind> Name` so root files feel related to `component Element`
- make the root kind explicit in source
- keep visual roots on `Window` and `Page` using familiar `render <Root />;`
- keep `Application` declarative and small
- keep `ResourceDictionary` constrained until resource/template semantics are
  deliberately designed
- make generated-app mode a first-class project mode, not a workaround hidden
  in samples

### Design

Add a `ComponentKind` model with these meanings:

| Kind | Generated public type | Body semantics |
| --- | --- | --- |
| `Element` | existing `ComponentInstance` pair | retained component render tree |
| `Page` | `Page` subclass | generated shell hosts a retained CSXAML body as page content |
| `Window` | `Window` subclass | generated shell hosts a retained CSXAML body as window content |
| `Application` | `Application` subclass plus generated entry point in generated-app mode | app startup, resources, services, first window creation |
| `ResourceDictionary` | `ResourceDictionary` subclass | merged dictionaries now; keyed resources remain future work |

Do not make `Window` or `Page` inherit from `ComponentInstance`. Generate a
real WinUI root type and a private retained body component behind it. That keeps
navigation, activation, and WinUI APIs normal while reusing CSXAML state,
rendering, diagnostics, and disposal behavior for the visual content.

Generate a real `Application` subclass from `component Application`. In
generated-app mode, also generate the startup entry point that `App.xaml`
normally causes the WinUI XAML compiler to produce. This generated entry point
must initialize WinRT/WinUI in the same shape as the current Windows App SDK
expects, then start the generated `App`.

Treat the WinUI-generated `App.g.i.cs` entry point as the compatibility model,
not as code to copy blindly forever. The implementation should have a focused
verification fixture that compares the generated CSXAML startup behavior
against the current WinUI app template behavior.

Recommended runtime additions:

- a content-host path that can mount a component into `Window.Content` or
  `Page.Content`, not only into a `Panel`
- root disposal that tears down the private body component when a generated
  window closes or page unloads
- optional service-provider handoff from generated `Application` to generated
  windows and pages
- source-contextual runtime exceptions when a generated shell cannot host its
  body
- a generated-root host abstraction with the same disposal guarantees as
  `CsxamlHost`

Recommended generator additions:

- `ComponentKind` in the AST rather than boolean flags
- one small emitter per root kind:
  - `ElementComponentEmitter`
  - `PageRootEmitter`
  - `WindowRootEmitter`
  - `ApplicationRootEmitter`
  - `ResourceDictionaryRootEmitter`
- a separate `ApplicationEntryPointEmitter` for generated-app mode
- a small `RootKindValidator` that owns app/window/page/dictionary rules
- root-kind validation separated from element-component validation
- deterministic generated names for private body components
- source maps for generated shell constructors, startup statements, and render
  bodies

### Generated Application Mode

Generated application mode is the end-to-end target for this feature. It should
replace `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, and
`MainWindow.xaml.cs`.

Required behavior:

- generated `App : Application`
- generated WinUI startup entry point
- deterministic startup window type
- generated startup window creation and activation
- generated or referenced application resource dictionary hookup
- service-provider creation through ordinary C# helper code
- service-provider handoff into generated windows, pages, and element
  components
- predictable app shutdown and root disposal
- source-facing diagnostics for missing startup window, duplicate app roots,
  missing generated-app project settings, or conflicts with XAML roots

Generated `Application` source should stay small and readable. Prefer explicit
members over a large DSL:

- `startup MainWindow;`
- `resources AppResources;`
- optional `IServiceProvider ConfigureServices()`
- optional `void OnStarted(LaunchActivatedEventArgs args)` or similar hook if
  the spec decides a hook is needed

Do not add app lifecycle mini-syntax for every WinUI activation scenario.
Advanced apps can still use handwritten C# partial methods or a hybrid shell,
but the starter and normal small-app story should be fully generated.

Deferred behavior:

- multiple launch activation modes
- deep protocol/file activation policy
- background tasks
- packaging manifest authoring
- installer/package identity authoring
- broad app lifecycle DSL

### Entry Point Generation

Generated-app mode must provide the entry point that disappears when
`App.xaml` is removed.

Implementation requirements:

- generate a single `Program` / entry point file only when
  `CsxamlApplicationMode` is `Generated`
- fail if another generated or handwritten entry point would collide
- fail if an `ApplicationDefinition` item such as `App.xaml` is present in
  generated mode
- emit the WinUI startup boilerplate in one small generated file
- keep the user's app logic in `component Application`, not in project-file
  properties
- add `FileWrites` entries so clean/rebuild removes generated entry files
- keep source maps for user-authored startup hooks and service configuration
  code

Project-system support should happen in MSBuild targets, not in user
instructions. A generated-root starter should build after `dotnet new` without
asking the author to hand-edit `App.xaml` item metadata.

### Window And Page Root Scope

`Window` and `Page` roots remove the most visible boilerplate:

- no empty `MainWindow.xaml`
- no named host panel solely for `CsxamlHost`
- normal `Frame.Navigate(typeof(GeneratedPage))` support through hidden
  generated Page XAML companions
- normal direct use of generated `Window` and `Page` types from C#

Generated roots should support a small set of common shell properties directly:

- `Title`
- `Width`
- `Height`
- startup size and position if WinUI exposes them cleanly

Anything more specialized should be ordinary C# helper code in the root file:

```csxaml
component Window MainWindow {
    void ConfigureWindow() {
        AppWindow.Title = "CSXAML Starter";
    }

    render <HomePage />;
}
```

The generated constructor should call the configuration hook at a predictable
point, mount the retained body, then leave the root as a normal WinUI object.

### ResourceDictionary Root Scope

`ResourceDictionary` roots should be part of the generated-app experience.

Start with a limited, honest surface:

- merged dictionaries
- keyed simple object resources
- style/resource objects that are already representable through supported
  metadata and property-content syntax
- resource dictionaries referenced by `component Application`

Do not include `DataTemplate`, `ControlTemplate`, implicit styles, or theme
dictionary authoring in the first slice unless the template/resource design is
explicitly expanded. XAML dictionaries remain the safest answer for deep WinUI
resource systems, but they should not be required for a normal CSXAML starter
app.

### Project-System Rules

Root generation must make the app mode obvious and fail clearly when it
conflicts with existing WinUI roots.

Rules:

- `CsxamlApplicationMode=Hybrid` allows CSXAML components and generated
  `Window` / `Page` types beside existing XAML roots
- `CsxamlApplicationMode=Generated` owns the app entry point and forbids
  `App.xaml` / `ApplicationDefinition`
- a project can have at most one generated `Application` root
- generated mode requires exactly one generated `Application` root
- hybrid mode may contain generated `Window` and `Page` roots but does not
  generate an entry point
- generated `Window`, `Page`, and `ResourceDictionary` names must not collide
  with existing C# or XAML `x:Class` types
- generated root types should participate in normal manifests so other projects
  can reference generated pages and dictionaries where appropriate
- starter templates should default to generated mode and include a documented
  hybrid option for migration or advanced app shells

Good diagnostics matter more than permissive magic:

- "Generated CSXAML application mode cannot be used while App.xaml is included
  as ApplicationDefinition."
- "Generated CSXAML application mode requires exactly one component
  Application declaration."
- "Hybrid CSXAML mode found component Application App, but entry point
  generation is disabled. Set CsxamlApplicationMode=Generated or keep a
  handwritten App.xaml.cs."

### Tests And Tooling

Required coverage:

- parser tests for every root kind
- diagnostics for duplicate application roots and app/XAML conflicts
- MSBuild tests for `Hybrid` versus `Generated` mode
- entry point generation tests
- generator tests for shell constructors and body component names
- package-consumer build tests with no `App.xaml` or `MainWindow.xaml`
- starter-template test with no `App.xaml`, no `App.xaml.cs`, no
  `MainWindow.xaml`, and no `MainWindow.xaml.cs`
- WinUI smoke tests for generated window content
- page navigation tests for generated pages
- resource lookup tests for generated dictionaries once keyed resources land
- completion, hover, semantic tokens, formatting, and snippets for root kinds
- docs that show generated-root apps as the default CSXAML app experience and
  hybrid WinUI shells as an escape hatch

## Slice 7: Template And Resource Interop Guidance

**Phase 09 status:** Resource/template guidance has landed. The docs now treat
generated `component ResourceDictionary` roots as the experimental generated-app
resource path for merged dictionaries, while keeping XAML dictionaries as the
recommended surface for deep templates, theme resources, `StaticResource`,
`ThemeResource`, `Binding`, and `DataContext`-heavy controls.

### Recommended Direction

Keep XAML resource dictionaries as the primary deep resource/template authoring
surface, even after CSXAML can generate limited `ResourceDictionary` roots.

CSXAML should consume WinUI styles, resources, templates, generated
ResourceDictionary roots, and typed helper objects. It should not immediately
become a complete second resource-dictionary language.

Reasons:

- `ControlTemplate`, `DataTemplate`, theme resources, and merged dictionaries
  are already deep WinUI systems.
- A partial CSXAML template language would create surprising edge cases around
  `DataContext`, bindings, theme invalidation, and designer/tooling support.
- The current CSXAML model is strongest when UI state and behavior are explicit
  C# component code.

### Guidance To Add

Add a first-class docs page:

`docs-site/articles/guides/resources-and-templates.md`

It should explain:

- use generated `component ResourceDictionary` roots for app-owned resources in
  generated CSXAML apps
- keep XAML resource dictionaries available for deep styles, brushes,
  templates, theme resources, and migration from existing WinUI apps
- expose resources to CSXAML through typed C# helpers when that improves
  readability
- assign `Style`, `Flyout`, or template properties from C# expressions when
  property-content syntax is not enough
- use native WinUI controls for template-heavy, `DataContext`-driven surfaces
- do not expect CSXAML to synthesize `StaticResource`, `ThemeResource`,
  `Binding`, or `DataContext` semantics

### Limited Future Template Authoring Option

Only after generated dictionaries, property-content syntax, and
attached-property breadth are stable, consider limited CSXAML template authoring
with these constraints:

- no component state
- no events
- no `foreach`
- no implicit `DataContext`
- output is plain WinUI objects or resource dictionaries
- templates are explicit generated object factories, not hidden runtime parsing

That should be a separate design. It should not block this feature track.

## Slice 8: List Virtualization Guidance

**Phase 10 status:** Experimental guidance landed. Docs and tooling now state
that `foreach` renders retained child subtrees and is not virtualization; large
scrolling item surfaces should use native virtualized controls, external
controls, or handwritten adapters.

### User Experience

The docs should make this distinction unmistakable:

- `foreach` renders repeated visible children in the CSXAML retained tree.
- Native virtualized controls own large scrolling item surfaces.

Recommended guidance:

```csxaml
// Good for small and moderate visible lists.
foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

```csxaml
// Prefer a native or external virtualized control for large datasets.
<VirtualizedTodoList ItemsSource={Items.Value} />
```

### Docs And Samples

Update the performance guide with a decision table:

| Scenario | Recommended shape |
| --- | --- |
| 5 to 100 visible component rows | CSXAML `foreach` with stable keys |
| hundreds of simple retained rows | CSXAML `foreach` can be acceptable when measured |
| thousands of rows | native virtualized control |
| data-template-heavy item surface | XAML `DataTemplate` or native control interop |
| dynamic list with editing controls | measure retained identity and focus behavior |

Add a sample or docs snippet showing how to wrap a native virtualized control
behind an external control or small handwritten adapter.

### Tooling

Do not add noisy compiler warnings based on arbitrary item counts. The compiler
cannot know the runtime size of most collections.

Useful tooling can still help:

- hover on `foreach` can mention that it is not virtualization
- docs links from performance-related hovers can point to the list guidance
- benchmark docs should keep 1000-item stress scenarios framed as a bound, not
  an authoring recommendation

## Slice 9: CSXAML Highlighting In Sample Presenters And Fixtures

**Phase 11 status:** Experimental alignment landed for the current repo
surfaces. The VS Code TextMate grammar covers the new syntax, DocFX continues
to load that grammar through Shiki, snippets and grammar tests cover generated
roots, refs, property content, named slots, and semantic-token fixtures cover
the generated-root keywords. `samples/Csxaml.FeatureGallery` now contains an
app-hosted `SampleCodePresenter` that uses a tiny documented fallback
classifier; full grammar reuse remains a future hardening option.

### User Experience

Any in-app sample presenter that shows CSXAML should color it as CSXAML, not
as C# or XML.

The repo already uses one TextMate grammar for VS Code and DocFX. Reuse that
grammar as the source of truth wherever possible.

### Implementation Direction

For static docs:

- keep DocFX using the existing Shiki post-build path
- keep Markdown fences tagged as `csxaml`
- keep CI checking that CSXAML-looking snippets are not accidentally fenced as
  `csharp`

For app-hosted sample presenters:

- add a tiny syntax-highlighting abstraction rather than hardcoding colors in
  sample controls
- prefer consuming the existing TextMate grammar through a build-time or
  startup tokenizer if practical
- if a Gallery-style `SampleCodePresenter` cannot consume TextMate directly,
  add a deliberately small CSXAML token classifier:
  - component and render keywords
  - tags
  - attributes
  - string literals
  - C# expression islands
  - comments
- document that the TextMate grammar remains authoritative

For tests and fixtures:

- add representative `.csxaml` snippets that cover new event args, refs,
  property-content syntax, named slots, generated roots, attached properties,
  and resource interop examples
- keep grammar tests updated when syntax changes
- add docs-render checks for property-content and `Ref` examples

## Suggested Delivery Order

1. Update `LANGUAGE-SPEC.md`, `ROADMAP.md`,
   `docs/supported-feature-matrix.md`, the implementation status table above,
   and the relevant DocFX pages with the intended post-v1 direction before
   implementation starts.
2. Implement typed event-args projection for one built-in control family and
   one external-control fixture.
3. Add `ElementRef<T>` and `Ref`, because it complements typed events and
   unlocks focus, scrolling, and animation interop.
4. Add content-property metadata discovery for external controls without new
   syntax first.
5. Add property-content syntax for native controls, then reuse the same syntax
   for named component slots.
6. Add the generated-root foundation: `ComponentKind`, root-kind validation,
   `Page` / `Window` emitters, and a content-host runtime path backed by
   private retained body components.
7. Complete generated application mode: generated `Application`, generated
   WinUI entry point, generated `ResourceDictionary`, MSBuild conflict checks,
   and a starter template with no `App.xaml`, no `App.xaml.cs`, no
   `MainWindow.xaml`, and no `MainWindow.xaml.cs`.
8. Expand attached-property metadata owner by owner, keeping each runtime
   applicator small.
9. Publish resource/template guidance and update native/external interop docs.
10. Strengthen virtualization guidance and sample wrapping patterns.
11. Update syntax highlighting, fixtures, snippets, completion, hover,
   formatting, semantic tokens, and docs examples across the whole new surface.

At each step, keep the documentation and status surfaces in sync with the code
change. Do not batch docs cleanup as a final separate milestone; stale status
tables are product bugs.

This order keeps each step useful on its own while avoiding a giant syntax
change that has no runtime or tooling story yet.

## Definition Of Done

This feature track is done when:

- common WinUI event args are typed and documented
- external controls can expose typed event args in the supported shape
- `ElementRef<T>` works for focus, scrolling, animation targets, and external
  controls
- `[ContentProperty]` controls behave naturally
- property-content syntax supports native property content and component named
  slots
- generated `Window` and `Page` roots host retained CSXAML bodies without
  handwritten XAML shells
- generated application mode builds and runs a WinUI app with no `App.xaml`,
  no `App.xaml.cs`, no `MainWindow.xaml`, and no `MainWindow.xaml.cs`
- generated `Application` and `ResourceDictionary` roots own startup and app
  resources in generated mode and fail clearly when they conflict with XAML
  roots
- broader attached properties cover Gallery-like layout and automation needs
- resource/template guidance is explicit and visible in docs
- large-list guidance prevents authors from treating `foreach` as hidden
  virtualization
- CSXAML snippets are highlighted correctly in docs, sample presenters, and
  editor/test fixtures
- generator, runtime, metadata, tooling, docs, and samples all use the same
  vocabulary
- `LANGUAGE-SPEC.md`, `ROADMAP.md`, `docs/supported-feature-matrix.md`, DocFX
  docs, samples, templates, and the implementation status table agree on what
  is supported, preview, experimental, or deferred
- regression tests cover each behavior at the layer that owns it

## Risks To Watch

- Event projection can sprawl if every WinUI event becomes a special adapter.
  Keep raw typed event args generic and reserve value-normalized events for
  deliberate cases.
- `Ref` can become a back door for lifecycle bugs. Clear refs deterministically
  and document that refs do not trigger rerender.
- Property-content syntax can become a template language by accident. Keep
  template/resource authoring as a separate decision.
- Attached-property breadth can create huge files. Split by owner and generate
  or curate in small units.
- Generated root support can accidentally become a shadow WinUI project system.
  Keep root kinds explicit, detect conflicts with existing XAML roots, and leave
  advanced app lifecycle policy in ordinary C#.
- Virtualization guidance can be ignored if samples overuse `foreach`. Samples
  should model native virtualization interop where large lists are discussed.
- Highlighting can drift if sample presenters use a separate classifier. Treat
  the TextMate grammar as the authoritative source and keep tests around any
  fallback classifier.
