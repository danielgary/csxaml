---
title: Language Overview
description: A reader-friendly overview of CSXAML syntax, components, state, rendering, and interop.
---

# Language Overview

CSXAML is a small component language that keeps UI structure close to XAML while keeping logic in ordinary C#.

Use this page for the mental model. Use [concepts](concepts.md) as a glossary
of the major moving parts and [syntax](syntax.md) as the copyable cheat sheet.

A component file contains:

- normal C# `using` directives
- a normal C# namespace
- one CSXAML component declaration
- optional `inject` and `State<T>` declarations
- ordinary helper code
- one final `render <Root />;` statement

Example:

```csxaml
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element CounterButton(string Label) {
    State<int> Count = new State<int>(0);

    render <Button
        Content={$"{Label}: {Count.Value}"}
        OnClick={() => Count.Value++} />;
}
```

The [language specification](specification.md) remains the formal contract.
