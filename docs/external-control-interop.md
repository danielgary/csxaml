# External Control Interop

CSXAML v1 is intentionally trying to feel like ordinary C# plus ordinary WinUI, not like a second import system bolted on next to it.

That means external controls from project references and NuGet packages come in through normal file-level `using` directives:

```csharp
using CommunityToolkit.WinUI.Controls;
using Widgets = MyCompany.Widgets;

namespace MyApp.Components;

component Element WarningBanner(bool IsOpen) {
    return <InfoBar
        Severity={InfoBarSeverity.Warning}
        IsOpen={IsOpen}
        Message="Unsaved changes" />;
}

component Element AvatarButton(Action OnClick) {
    return <Widgets:StatusButton OnClick={OnClick} />;
}
```

## What The Current Prototype Supports

The current supported slice is narrow on purpose.

- file-level `using Namespace;` imports
- file-level `using Alias = Namespace;` aliases
- bare external control tags when a single imported match exists
- alias-qualified tags such as `<Widgets:StatusButton />`
- deterministic ambiguity diagnostics when a bare name resolves to more than one supported imported control
- deterministic compile-time diagnostics when an imported control is unknown or unsupported

## Discovery And Generation Path

External-control support is not based on broad runtime scanning.

The current pipeline is:

1. the host build passes an explicit reference list to the generator
2. the generator loads referenced managed assemblies in a dedicated load context
3. supported external controls are reflected into the same metadata model used for built-in controls
4. generation emits a registration file that registers external-control descriptors with the runtime
5. the runtime creates and patches those controls through `ExternalControlRegistry` and `ExternalControlAdapter`

That keeps the path deterministic, testable, and aligned with the roadmap direction for long-term interop.

## Supported Control Types

A reflected external control is currently supported only when it is:

- public and non-abstract
- non-generic
- `FrameworkElement`-derived
- constructible through a public parameterless constructor

Controls that fail those checks are rejected during metadata discovery instead of falling through to runtime guesswork.

## Supported Properties

The generator currently reflects public writable instance properties and keeps only the shapes the runtime can validate and apply predictably.

Supported today:

- writable dependency properties with supported value kinds
- writable dependency properties whose type is `object`
- writable CLR properties with non-`Object` supported value kinds
- inherited built-in control properties that already exist in CSXAML metadata

Supported value kinds currently include:

- `string`
- `bool`
- `int`
- `double`
- enums
- `Thickness`
- `Brush`
- `Style`

What this means in practice:

- common visual and layout properties can flow through the shared metadata model
- explicit style helpers can be passed to external controls through ordinary `Style={...}` expressions
- unsupported property shapes fail at compile time instead of waiting for a runtime exception

## Supported Events

External events are intentionally simple in the current slice.

An external event is supported when:

- it is a public instance event
- its delegate returns `void`
- none of its delegate parameters are `ref` or `out`

Supported events are exposed to CSXAML as `On<EventName>` and currently bind as plain `Action` handlers. Event arguments are not surfaced into component code yet; the runtime adapter invokes the supplied action and discards the original event payload.

That is enough to support common button-like and notification-like interactions without committing the language to a richer event-argument story too early.

## Child Content Support

Child-content shape is inferred conservatively from the control type.

Supported today:

- `Panel`-like controls or controls with a public `Children : UIElementCollection` property map to multi-child content
- `ContentControl`-like controls, `Border`, `ScrollViewer`, or controls with a public `Child` or `Content` property map to single-child content
- everything else is treated as childless

Unsupported child-content shapes fail deterministically rather than being guessed.

## Attached Properties

The current attached-property slice is still intentionally built-in only.

Today that means:

- external controls can receive supported built-in attached properties such as `Grid.Row` or `AutomationProperties.Name`
- discovery of external attached-property owners is not part of the current supported slice

This is one of the places where the roadmap is intentionally leaving room for future expansion without over-promising v1.

## What Is Not Promised Yet

These areas are still outside the supported v1 slice today:

- arbitrary external control types without public parameterless constructors
- generic or non-public control types
- event handlers that need surfaced event arguments inside CSXAML
- external attached-property owner discovery
- unconstrained reflection over every reachable assembly
- fully hardened project-system and design-time tooling behavior for every reference shape

If one of those limits becomes painful, the right next step is to extend the metadata model and generator/runtime contract deliberately, not to slip in hidden fallback behavior.
