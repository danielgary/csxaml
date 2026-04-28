---
title: Language Overview
description: A reader-friendly overview of CSXAML syntax, components, state, rendering, and interop.
---

# Language Overview

CSXAML is a small component language that keeps UI structure close to XAML while keeping logic in ordinary C#.

A component file contains:

- normal C# `using` directives
- a normal C# namespace
- one or more CSXAML component declarations
- optional `inject` and `State<T>` declarations
- ordinary helper code
- one final `render <Root />;` statement

Example:

```csharp
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element CounterButton(string Label) {
    State<int> Count = new State<int>(0);

    render <Button
        Content={$"{Label}: {Count.Value}"}
        OnClick={() => Count.Value++} />;
}
```

Read the [concepts](concepts.md) page first, then use the [syntax](syntax.md) page as a practical guide. The [language specification](specification.md) remains the formal contract.
