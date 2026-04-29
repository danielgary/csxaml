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
- property, event, and child-content binding where metadata describes the control

Current boundaries:

- no broad `DataContext` binding model
- broader attached-property owner discovery is deferred
- unsupported control APIs should fail with source-facing diagnostics

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

Event arguments not projected:

```csxaml
render <Widgets:StatusButton OnClick={(sender, args) => Save(args)} />;
```

Expected C# diagnostic shape after generation:

```text
error CS1593: Delegate 'Action' does not take 2 arguments
```

Fix: bind a no-argument handler and keep event-payload-specific work inside the
external control or a C# adapter until richer event-argument projection is
supported.
