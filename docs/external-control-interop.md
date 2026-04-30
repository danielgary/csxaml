# External Control Interop

CSXAML v1 is intentionally trying to feel like ordinary C# plus ordinary WinUI, not like a second import system bolted on next to it.

That means external controls from project references and NuGet packages come in through normal file-level `using` directives:

```csxaml
using CommunityToolkit.WinUI.Controls;
using Widgets = MyCompany.Widgets;

namespace MyApp.Components;

component Element WarningBanner(bool IsOpen) {
    render <InfoBar
        Severity={InfoBarSeverity.Warning}
        IsOpen={IsOpen}
        Message="Unsaved changes" />;
}

component Element AvatarButton(Action OnClick) {
    render <Widgets:StatusButton OnClick={OnClick} />;
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
- writable CLR `UIElement` properties for experimental property-content syntax
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

External events are metadata-defined and intentionally limited to predictable
delegate shapes.

An external event is supported when:

- it is a public instance event
- its delegate returns `void`
- none of its delegate parameters are `ref` or `out`

Supported events are exposed to CSXAML as `On<EventName>`.

- zero-argument or inherited curated command events bind as `Action`
- ordinary two-argument `EventHandler<TEventArgs>` style events bind as
  senderless `Action<TEventArgs>`

The sender is omitted. If component code needs the element itself, use
`Ref={...}` with `ElementRef<T>` on the external control tag.

## Child Content Support

Child-content shape is metadata-driven. For external controls, discovery now
records the default content property name, the supported content kind, relevant
CLR type names, and where the metadata came from.

Supported today:

- `[ContentProperty(Name = "...")]` on the control or an inherited base type
  is preferred over naming conventions
- a default content property that accepts a single `UIElement` maps to
  single-child content
- a default content property that accepts `object` maps to single-child content
- `Panel`-like controls or controls with a public `Children : UIElementCollection` property map to multi-child content
- `ContentControl.Content`, `Border.Child`, `ScrollViewer.Content`, public
  `Child`, and public `Content` remain supported conventions
- everything else is treated as childless

Example:

```csharp
[ContentProperty(Name = nameof(Example))]
public sealed class ControlExample : Button
{
    public UIElement? Example { get; set; }
    public UIElement? Options { get; set; }
}
```

```csxaml
using Widgets = MyApp.Controls;

render <Widgets:ControlExample>
    <Button Content="Run" />
</Widgets:ControlExample>;
```

The child button is projected into `ControlExample.Example`.

Unsupported child-content shapes fail deterministically rather than being
guessed. For example, `[ContentProperty(Name = "UnsupportedContent")]` on an
`int` property is recorded as unsupported metadata and diagnostics name the
content property.

Experimental property-content syntax can target metadata-backed named
`UIElement` properties:

```csxaml
<Widgets:ControlExample>
    <Widgets:ControlExample.Example>
        <Button Content="Run" />
    </Widgets:ControlExample.Example>
    <Widgets:ControlExample.Options>
        <CheckBox Content="Enabled" />
    </Widgets:ControlExample.Options>
</Widgets:ControlExample>
```

Unsupported or template-heavy external APIs should remain in handwritten C# or
XAML resources until their shape is explicitly modeled. Keep `DataTemplate`,
`ControlTemplate`, theme resources, and `DataContext`-heavy interop in XAML
dictionaries when those native WinUI semantics matter.

## Attached Properties

The current attached-property slice is still intentionally built-in only.

Today that means:

- external controls can receive supported built-in attached properties such as
  `Grid.Row`, `Canvas.Left`, `ToolTipService.ToolTip`, or
  `AutomationProperties.Name`
- attached-property owners still resolve through ordinary visible type names or explicit type aliases, just like the rest of CSXAML
- discovery of external attached-property owners is not part of the current supported slice

This is one of the places where the roadmap is intentionally leaving room for future expansion without over-promising v1.

## What Is Not Promised Yet

These areas are still outside the supported v1 slice today:

- arbitrary external control types without public parameterless constructors
- generic or non-public control types
- open-ended event transformations beyond `Action` and senderless
  `Action<TEventArgs>`
- external attached-property owner discovery
- unconstrained reflection over every reachable assembly
- fully hardened project-system and design-time tooling behavior for every reference shape

If one of those limits becomes painful, the right next step is to extend the metadata model and generator/runtime contract deliberately, not to slip in hidden fallback behavior.
