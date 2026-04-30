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

Assert.IsNotNull(render.FindByText("Write docs"));
Assert.IsNull(render.TryFindByText("Updated title"));

render.EnterText(render.FindByAutomationId("SelectedTodoTitle"), "Ship docs");
render.SetChecked(render.FindByAutomationId("SelectedTodoDone"), true);

Assert.IsNotNull(render.TryFindByText("Ship docs"));
Assert.IsNull(render.TryFindByText("Write docs"));
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

Hostless component tests should prefer semantic queries over native
`ElementRef<T>` handles. Refs are for projected WinUI interop such as focus,
scrolling, and animation targets; behaviors that depend on live native controls
still belong in WinUI projection tests.

Automation attached properties are the preferred way to make component tests
read semantically. `AutomationProperties.AutomationId` and
`AutomationProperties.Name` are queryable through `Csxaml.Testing`; expanded
properties such as `HelpText`, `ItemStatus`, `ItemType`, and `LabeledBy` are
projected for WinUI accessibility interop and should be covered by projection
tests when a workflow depends on the live native value.

The important pattern is to assert the initial tree first, perform one
interaction, then assert the new tree. That keeps tests tied to user-visible
behavior instead of generated implementation details.
