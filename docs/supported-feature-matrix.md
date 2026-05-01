# Supported Feature Matrix

## Release Posture

These labels are used consistently across the README, roadmap, packaging docs, getting-started guide, and release notes.

| Label | Meaning |
| --- | --- |
| `Supported in v1` | Implemented now, documented, and inside the current compatibility promise. |
| `Preview shipping path` | Exists and is intended to ship, but the current artifact is still a preview or local-validation path rather than part of a tagged v1 release. |
| `Experimental` | Usable for exploration, but outside the current v1 promise. |
| `Not in v1` | Explicitly deferred; do not build against it as an expected v1 capability. |

Feature-matrix status mapping:

- `Supported in v1`: covered by Milestone 13 behavior and regression tests
- `Experimental`: available, but not yet part of the stable Milestone 13 promise
- `Not in v1`: intentionally outside the current Milestone 13 and v1 contract

| Area | Status | Notes |
| --- | --- | --- |
| Component parameters | Supported in v1 | Public prop surface only. |
| `State<T>` declarations | Supported in v1 | Component-local mutable UI state. |
| `inject Type name;` | Supported in v1 | Required typed services resolved once per component instance. |
| Conditional markup (`if`) | Supported in v1 | Covered by generator/runtime regression tests. |
| Repeated markup (`foreach`) | Supported in v1 | Includes keyed child retention behavior. `foreach` renders repeated retained children and is not virtualization. |
| Slots | Supported in v1 | Default slots remain in the v1-compatible surface. Named slots are implemented experimentally through the separate property-content feature row and remain outside the v1 promise. Fallback content and fragment-root pass-through remain outside v1. |
| Attached properties | Supported in v1 | Supported for the current built-in owner slice, including semantic automation metadata plus owner resolution through ordinary imports and explicit type aliases. Broader owner discovery remains outside v1. |
| Built-in control property/event binding | Supported in v1 | Covered by generator/runtime tests. Text-heavy surfaces can opt into WinUI wrapping through `TextBlock.TextWrapping`, `TextBox.TextWrapping`, and explicit `ScrollViewer` horizontal scroll-mode settings. `ListView` keeps native scrolling state by leaving stable `ItemsSource`, item-click, and selection-mode values alone during rerenders. Generated windows subclass the top-level window and WinUI child HWNDs, plus install a UI-thread wheel hook, for DPI-aware mouse-wheel fallback routing to the nearest vertically scrollable `ScrollViewer` whose transformed bounds contain the pointer when native handling does not move it; hosted roots include a routed pointer fallback. Slider `ValueChanged` handlers are guarded against render churn during native pointer drags. |
| External controls from referenced assemblies | Supported in v1 | Covered by validation, emission, runtime, and demo interop tests for the documented supported shape only. |
| Solution-local external controls | Supported in v1 | Covered by external-control validation and runtime regression paths for the documented supported shape only. |
| Typed event-argument projection for common WinUI events | Experimental | Metadata-defined senderless `Action<TEventArgs>` handlers now exist for common WinUI event args such as loaded, key, pointer, selection, value, item-click, autosuggest, frame navigation, and supported external `EventHandler<TEventArgs>` events. This remains outside the v1 promise. |
| Element refs / imperative handles | Experimental | `ElementRef<T>` and native `Ref={...}` now work for built-in and registered external native controls, clear on removal/disposal, and remain outside the v1 promise. Component refs and `x:Name` symbolic lookup are not supported. |
| External-control content-property awareness | Experimental | Metadata records default content properties for supported external controls, including `[ContentProperty]`, inherited content attributes, WinUI content conventions, and `UIElementCollection` child collections. Named `UIElement` properties can be targeted by experimental property-content syntax when metadata exposes them. |
| Property-content syntax and named slots | Experimental | XAML-familiar `<Owner.Property>` child elements now work for metadata-backed native property content and component named slots, with validation, emission, runtime projection, completion, hover, formatting, semantic tokens, and local slot go-to-definition. This remains outside the v1 promise; unsupported native properties, fallback slot content, and template authoring are still deferred. |
| Broader attached-property metadata | Experimental | Built-in metadata, validation, runtime application/clearing, completion, hover, semantic tokens, and quick-fix suggestions now cover `Canvas`, `RelativePanel`, `ScrollViewer`, `ToolTipService`, `VariableSizedWrapGrid`, and expanded `AutomationProperties` entries. External attached-property owner discovery remains outside v1. |
| Generated `Page` and `Window` roots | Experimental | `component Page` and `component Window` generate real WinUI shell types backed by retained CSXAML body components. Page roots emit hidden XAML companions for native `Frame.Navigate(typeof(PageType))` activation. `Window` supports limited `Title`, `Width`, `Height`, and `Backdrop` declarations. This remains outside the v1 promise. |
| Generated application mode | Experimental | `CsxamlApplicationMode=Generated` emits a WinUI entry point from one `component Application`, rejects source `App.xaml` `ApplicationDefinition` conflicts, emits a hidden intermediate `App.xaml` so WinUI default control resources load normally, creates the configured startup window, and can remove the default WinUI app/window shell files from source. Advanced activation and packaging policy remain deferred. |
| Generated `ResourceDictionary` app resources | Experimental | `component ResourceDictionary` supports app-owned merged dictionaries for generated apps. Default `XamlControlsResources` are loaded through the hidden generated `App.xaml`; default-only generated dictionaries are recognized without runtime instantiation. Keyed resources, styles, templates, and theme dictionaries remain separate design work. |
| Resource and template interop guidance | Experimental | The resources/templates guide explains generated app resource dictionaries, XAML dictionary interop, and the explicit boundary around `StaticResource`, `ThemeResource`, `Binding`, `DataTemplate`, `ControlTemplate`, and `DataContext` semantics. |
| List virtualization guidance | Experimental | Performance docs, language docs, and tooling hovers now distinguish `foreach` retained rendering from native virtualization. First-class CSXAML virtualization remains outside v1. |
| Root instance host path | Supported in v1 | `CsxamlHost(panel, instance)` and hostless coordinator/test flows remain valid. |
| Root type activation with services | Supported in v1 | `CsxamlHost(panel, typeof(Component), services)` and hostless test rendering support this path. |
| `IServiceProvider` activation boundary | Supported in v1 | Public DI boundary for runtime/testing. |
| `ActivatorUtilities` constructor activation | Supported in v1 | Used by the default activator when creating component instances. |
| `OnMounted()` | Supported in v1 | Runtime hook for handwritten `ComponentInstance` types; `.csxaml` source does not yet have dedicated lifecycle syntax. |
| `IDisposable` cleanup | Supported in v1 | Removed components and disposed roots are cleaned up once; handwritten component types can implement cleanup directly. |
| `IAsyncDisposable` cleanup | Supported in v1 | Async root disposal is supported; sync removal paths block on async disposal when needed. |
| Post-unmount state invalidation no-op | Supported in v1 | Stale component instances do not rerender the tree after removal/disposal. |
| `Csxaml.Testing` root-instance render | Supported in v1 | Hostless logical-tree rendering. |
| `Csxaml.Testing` root-type render | Supported in v1 | Supports DI activation. |
| `Csxaml.Testing` service overrides | Supported in v1 | `Action<ServiceCollection>` overloads are available. |
| `Csxaml.Testing` query by automation id/name | Supported in v1 | Preferred semantic query surface. |
| `Csxaml.Testing` query by text/content | Supported in v1 | Works over logical tree properties. |
| `Csxaml.Testing` click/text/checked interactions | Supported in v1 | Designed for common component workflows. |
| Tag and helper-code hover | Supported in v1 | Covers component tags, control tags, native properties/events, attached properties, component parameters, and projected C# helper-code symbols that can be resolved from the current file/workspace. |
| Suggestion-based quick fixes | Supported in v1 | Covers single-symbol replacements for misspelled visible tags and attribute names. |
| CSXAML highlighting in sample presenters and fixtures | Experimental | VS Code TextMate grammar, DocFX Shiki highlighting, snippets, grammar tests, semantic-token fixtures, and `samples/Csxaml.FeatureGallery` app-hosted `SampleCodePresenter` coverage now exist for the post-v1 syntax surface. The app-hosted presenter uses a deliberately small fallback classifier rather than the full TextMate grammar. |
| Generator/runtime cross-version mixing | Not in v1 | Upgrade generator, runtime, control metadata, and testing together. |
| Keyed/named/optional DI syntax | Not in v1 | Outside Milestone 13. |
| Property injection or markup injection | Not in v1 | Use explicit `inject` declarations instead. |
| Per-component DI scopes | Not in v1 | Host services are authoritative. |
| Subtree service-provider overrides | Not in v1 | Outside Milestone 13. |
| `OnUpdated` / effect hooks / dependency arrays | Not in v1 | Lifecycle remains intentionally small. |
| Full visual/UI automation testing framework | Experimental | WinUI projection tests exist, but hostless logical-tree testing is the supported Milestone 13 story. |

## Productization Snapshot

| Area | Status | Notes |
| --- | --- | --- |
| NuGet package consumption | Preview shipping path | Clean validation exists from locally packed `0.1.0-preview.1` artifacts, matching the public package examples. |
| Existing WinUI sample app | Supported in v1 | `samples/Csxaml.ExistingWinUI` demonstrates the v1-safe WinUI shell plus `CsxamlHost` path. |
| Starter `dotnet new` template | Not in v1 | No public starter-template package is documented in the current preview docs. Use the blank WinUI app path or `samples/Csxaml.ExistingWinUI` instead. |
| Fully generated CSXAML starter shell | Experimental | `samples/Csxaml.HelloWorld` is the minimal generated app, `samples/Csxaml.TodoApp` is the advanced generated Todo app, and `samples/Csxaml.FeatureGallery` demonstrates the broader experimental feature surface through `App.csxaml`, generated `Window`/`Page` roots, generated resources, external controls, and no source `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, or `MainWindow.xaml.cs`. The public starter docs still keep this outside the v1 promise. |
| Visual Studio VSIX workflow | Supported in v1 | Documented bootstrap and packaging path exist for the current Visual Studio 18 authoring story. |
| VS Code extension workflow | Preview shipping path | The repo-local extension still supports local iteration, and `scripts/Package-VSCodeExtension.ps1` produces an installable VSIX aligned to the current `0.1.0-preview.1` package line with a bundled language server. |
| Benchmark runner and local perf gate | Supported in v1 | `scripts/Run-Benchmarks.ps1` writes timestamped benchmark snapshots, maintains `artifacts/benchmarks/baseline.json` and `baseline.md`, and gates the 1000-item hostless runtime rerender lanes on the audited runner while metadata, generator, and tooling remain report-only. |

## Current Release Watch Items

These are not blockers for the current preview line, but they remain important follow-up work:

- no CI trend lane exists yet for benchmark history
- broader editor code-action coverage remains intentionally small

## Not in V1

These areas are intentionally outside the current v1 contract:

- stable-v1 named slots, slot fallback content, and fragment-root slot pass-through
- fragment-root slot pass-through
- broader external attached-property owner discovery
- open-ended event-argument projection beyond the documented experimental typed event set
- stable-v1 property-content syntax guarantees
- property-content for unsupported or template-heavy targets such as broad `<Button.Flyout>` authoring without metadata/runtime support
- stable-v1 generated `Application`, `Window`, `Page`, and `ResourceDictionary` roots
- stable-v1 generated application mode without `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, or `MainWindow.xaml.cs`
- stable-v1 generated CSXAML resource dictionaries for app-owned resources
- first-class CSXAML virtualization abstractions and stable-v1 very large
  visible-list strategies
- stable app-hosted `SampleCodePresenter` integration beyond the current small
  fallback classifier
- `DataContext`-heavy third-party control interop
- dedicated source-level lifecycle or cancellation syntax
