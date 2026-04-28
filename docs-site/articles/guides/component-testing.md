---
title: Component Testing
description: How to test CSXAML components through hostless logical-tree rendering.
---

# Component Testing

`Csxaml.Testing` renders components without a live WinUI window and exposes semantic query and interaction helpers.

Basic shape:

```csharp
using Csxaml.Testing;
using MyApp.Components;

using var render = CsxamlTestHost.Render<TodoBoardComponent>();

render.Click(render.FindByText("Select"));
render.EnterText(render.FindByAutomationId("SelectedTodoTitle"), "Ship docs");
render.SetChecked(render.FindByAutomationId("SelectedTodoDone"), true);
```

Preferred query surface:

- `FindByAutomationId`
- `FindByAutomationName`
- `FindByText`
- `TryFindByAutomationId`
- `TryFindByAutomationName`
- `TryFindByText`

Interactions:

- `Click(node)`
- `EnterText(node, value)`
- `SetChecked(node, value)`

When the interaction invokes a component event, the render result updates automatically through the coordinator. If a test changes external state without an interaction callback, call `Rerender()`.
