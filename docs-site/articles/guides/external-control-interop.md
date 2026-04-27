---
title: External Control Interop
description: How package-provided and solution-local WinUI controls are discovered and used from CSXAML.
---

# External Control Interop

CSXAML discovers supported external controls from referenced assemblies and uses normal C# namespace imports.

Package-provided WinUI control example:

```csharp
using WinUi = Microsoft.UI.Xaml.Controls;

component Element Notice {
    render <WinUi:InfoBar
        IsOpen={true}
        Title="Interop"
        Message="External WinUI controls resolve through using aliases." />;
}
```

Solution-local control example:

```csharp
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
