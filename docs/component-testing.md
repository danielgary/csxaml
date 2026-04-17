# Component Testing

Milestone 13 adds a supported hostless logical-tree testing surface in `Csxaml.Testing`.

Today that surface is implemented and used throughout this repo, but the package is still being treated as repo-internal while the public API is reviewed. The supported testing model exists now; the public package boundary is still being finalized.

The goal is simple:

- render components from ordinary C#
- query semantically
- drive common interactions
- avoid generated-code spelunking and brittle child-index tests where practical

## Basic render flows

Render a root type:

```csharp
using Csxaml.Testing;

using var render = CsxamlTestHost.Render<TodoBoardComponent>();
Assert.AreEqual("Grid", render.Root.TagName);
```

Render a prebuilt root instance:

```csharp
var card = new TodoCardComponent();
card.SetProps(new TodoCardProps("Ship milestone", true, false, () => { }, () => { }));

using var render = CsxamlTestHost.Render(card);
```

## Service overrides

Tests can provide services without turning them into props:

```csharp
using var render = CsxamlTestHost.Render<TodoBoardComponent>(
    services => services.AddSingleton<ITodoRepository>(new FakeTodoRepository()));
```

This uses the same `IServiceProvider` boundary as the runtime.

## Semantic queries

Preferred queries:

- `FindByAutomationId`
- `FindByAutomationName`
- `FindByText`

Examples:

```csharp
var titleEditor = render.FindByAutomationId("SelectedTodoTitle");
var statusButton = render.FindByAutomationName("Selection Status Button");
var saveButton = render.FindByText("Save");
```

`TryFindBy...` variants are available when absence is part of the test.

## Common interactions

The built-in interaction helpers cover the common logical-tree cases:

- `Click(node)`
- `EnterText(node, value)`
- `SetChecked(node, value)`

Example:

```csharp
render.EnterText(render.FindByAutomationId("SelectedTodoTitle"), "Ship Milestone 13");
render.SetChecked(render.FindByAutomationId("SelectedTodoDone"), true);
render.Click(render.FindByText("Increment"));
```

When the interaction triggers state invalidation, the render result updates automatically through the coordinator.

If a test changes external state without triggering an interaction callback, call `Rerender()` explicitly.

## When child-index assertions are still reasonable

Semantic queries are preferred for user-visible workflows.

Direct tree traversal is still reasonable when the test is specifically about:

- layout/container shape
- keyed retention details
- exact child ordering behavior

Milestone 13 does not ban structural assertions. It just stops making them the default.

## What this testing layer is and is not

`Csxaml.Testing` is:

- hostless
- logical-tree based
- good for component behavior tests

`Csxaml.Testing` is not:

- a visual diff system
- a replacement for WinUI projection tests
- a general-purpose UI automation framework

Keep WinUI projection tests when the behavior depends on native control behavior, focus, selection restoration, or host-specific projection details.
