---
title: External Control Interop
description: How package-provided and solution-local WinUI controls are discovered and used from CSXAML.
---

# External Control Interop

CSXAML discovers supported external controls from referenced assemblies and uses normal C# namespace imports.

Package-provided WinUI control example:

```csxaml
using WinUi = Microsoft.UI.Xaml.Controls;

component Element Notice {
    render <WinUi:InfoBar
        IsOpen={true}
        Title="Interop"
        Message="External WinUI controls resolve through using aliases." />;
}
```

Solution-local control example:

```csxaml
using Widgets = MyApp.Controls;

component Element AvatarButton(Action OnClick) {
    render <Widgets:StatusButton OnClick={OnClick} />;
}
```

Current supported shape:

- normal `using` imports
- using aliases for qualified tags
- referenced-assembly metadata discovery
- generated runtime registration for supported external controls
- property, event, ref, and child-content binding where metadata describes the
  control
- experimental default content-property metadata for controls that use
  `[ContentProperty]`
- experimental property-content syntax for metadata-backed named `UIElement`
  properties

Current boundaries:

- no broad `DataContext` binding model
- broader attached-property owner discovery is deferred
- unsupported control APIs should fail with source-facing diagnostics
- external `EventHandler<TEventArgs>` events can project senderless
  `Action<TEventArgs>` handlers, but value-normalized events remain explicit
  adapter features
- external native controls can be targeted with `Ref={...}` and
  `ElementRef<T>` for imperative interop

## Child Content

External child content is metadata-driven. CSXAML records the default content
property name, whether it accepts one child or a `UIElementCollection`, and
whether the source was `[ContentProperty]`, inherited metadata, built-in
metadata, or a convention.

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

component Element ExampleHost {
    render <Widgets:ControlExample>
        <Button Content="Run" />
    </Widgets:ControlExample>;
}
```

The child button is assigned to `ControlExample.Example`. Unsupported content
property types fail with diagnostics that name the property.

Use experimental property-content syntax when the named property should be
visible at the call site:

```csxaml
using Widgets = MyApp.Controls;

component Element ExampleHost {
    render <Widgets:ControlExample>
        <Widgets:ControlExample.Example>
            <Button Content="Run" />
        </Widgets:ControlExample.Example>
        <Widgets:ControlExample.Options>
            <CheckBox Content="Enabled" />
        </Widgets:ControlExample.Options>
    </Widgets:ControlExample>;
}
```

The owner part must match the containing tag, and the target property must be
present in generated metadata. Unsupported or template-heavy external APIs
should stay in handwritten C# or XAML resources until their shape is explicitly
modeled. For `DataTemplate`, `ControlTemplate`, theme resources, or
`DataContext`-heavy controls, keep the resource or template in XAML and merge it
through the app's resource dictionary. See
[Resources and Templates](resources-and-templates.md).

## Failure examples

Unknown external tag:

```csxaml
using Widgets = MyApp.Controls;

render <Widgets:PrivateStatusButton />;
```

Expected diagnostic shape:

```text
unsupported tag name 'Widgets:PrivateStatusButton'
```

Fix: make the control public, non-abstract, non-generic, `FrameworkElement`-
derived, and constructible with a public parameterless constructor, or use a
supported control.

Unsupported property:

```csxaml
render <Widgets:StatusButton BadgeCount={Count.Value} />;
```

Expected diagnostic shape:

```text
unknown attribute 'BadgeCount' on native control 'Widgets:StatusButton'
```

Fix: expose a supported writable property shape on the control, or keep that
state inside the control instead of assigning it from CSXAML.

Wrong event delegate shape:

```csxaml
render <Widgets:StatusButton OnClick={(sender, args) => Save(args)} />;
```

Expected C# diagnostic shape after generation:

```text
error CS1593: Delegate 'Action' does not take 2 arguments
```

Fix: match the event metadata. Direct command-style events such as `OnClick`
use `Action`; supported external `EventHandler<TEventArgs>` events use
senderless `Action<TEventArgs>`.
