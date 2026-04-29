---
title: External Controls
description: How referenced WinUI and custom controls are consumed from CSXAML.
---

# External Controls

External controls are discovered from referenced assemblies and used through normal C# imports and aliases.

```csxaml
using WinUi = Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element Notice {
    render <WinUi:InfoBar
        IsOpen={true}
        Title="Interop"
        Message="This external WinUI control resolved through a using alias." />;
}
```

Solution-local controls and package-provided controls use the same model when metadata can be generated for the referenced assembly.

Current boundaries:

- normal using imports and aliases are supported
- deterministic referenced-assembly metadata discovery is supported
- richer `DataContext`-heavy interop is outside the v1 promise
- broader attached-property owner discovery remains intentionally limited

See [External Control Interop](../guides/external-control-interop.md) for the practical guide.
