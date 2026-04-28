---
title: Component Model
description: How CSXAML components expose props, render native nodes, and compose child components.
---

# Component Model

CSXAML components are generated C# component classes.

The component parameter list defines the public prop surface:

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    render <Button Content={Title} OnClick={OnToggle} />;
}
```

Components can render:

- native WinUI controls
- other CSXAML components
- conditional children
- repeated children
- slotted child content in the supported default-slot shape

Component helper code runs as ordinary generated C#. Keep helper methods small and explicit so the rendered UI remains easy to trace.

## Child components

Use component tags by importing the namespace or placing the components in the same namespace:

```csharp
<TodoCard
    Title={item.Title}
    IsDone={item.IsDone}
    OnToggle={() => ToggleItem(item.Id)} />
```

## Keys

Use `Key` for repeated component children when identity matters:

```csharp
foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

Keys should be stable, unique among siblings, and derived from model identity rather than display text.
