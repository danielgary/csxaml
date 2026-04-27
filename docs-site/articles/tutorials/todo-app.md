---
title: Build a Todo App
description: Build a CSXAML Todo board with props, state, events, external controls, and tests.
---

# Build a Todo App

This tutorial builds a small Todo board using the supported CSXAML v1 surface:

- components
- typed props
- component-local state
- native WinUI controls
- events
- keyed repeated children
- controlled `TextBox` and `CheckBox` inputs
- hostless component tests

The repo demo under `Csxaml.Demo` is the reference implementation for this tutorial.

## 1. Start with a WinUI project

Use a WinUI app or class library that meets the [prerequisites](../getting-started/prerequisites.md). Add the `Csxaml` package and keep `Microsoft.WindowsAppSDK` referenced by the app project.

## 2. Create a model

Use an immutable model so UI updates are explicit:

```csharp
namespace MyApp;

public sealed record TodoItemModel(
    string Id,
    string Title,
    string Notes,
    bool IsDone);
```

## 3. Add a card component

Create `TodoCard.csxaml`:

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element TodoCard(
    string Title,
    bool IsDone,
    bool IsSelected,
    Action OnSelect,
    Action OnToggle) {
    render <Border BorderThickness={1} Padding={8} Margin={new Thickness(0, 0, 0, 8)}>
        <StackPanel Spacing={6}>
            <TextBlock Text={Title} />
            <TextBlock Text={IsDone ? "Done" : "Open"} />
            <Button Content={IsSelected ? "Selected" : "Select"} OnClick={OnSelect} />
            <Button Content="Toggle" OnClick={OnToggle} />
        </StackPanel>
    </Border>;
}
```

Typed props are part of the generated component constructor surface. Event props are ordinary C# delegates.

## 4. Add an editor component

Create `TodoEditor.csxaml`:

```csharp
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element TodoEditor(
    string ItemId,
    string Title,
    string Notes,
    bool IsDone,
    Action<string, string> OnTitleChanged,
    Action<string, string> OnNotesChanged,
    Action<string, bool> OnDoneChanged) {
    render <StackPanel Spacing={8}>
        <TextBlock Text="Selected task" />
        <TextBox
            Text={Title}
            OnTextChanged={value => OnTitleChanged(ItemId, value)} />
        <TextBox
            Text={Notes}
            OnTextChanged={value => OnNotesChanged(ItemId, value)} />
        <CheckBox
            Content="Done"
            IsChecked={IsDone}
            OnCheckedChanged={value => OnDoneChanged(ItemId, value)} />
    </StackPanel>;
}
```

Controlled input is explicit: component code owns the value, and input events write the new value back.

## 5. Add the board state

Create `TodoBoard.csxaml`:

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element TodoBoard {
    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(new()
    {
        new TodoItemModel("first", "Write docs", "Create the public docs site.", false),
        new TodoItemModel("second", "Build sample", "Keep the tutorial honest.", false)
    });

    State<string> SelectedItemId = new State<string>("first");

    TodoItemModel SelectedItem()
    {
        return Items.Value.Single(item => item.Id == SelectedItemId.Value);
    }

    void UpdateItem(string itemId, Func<TodoItemModel, TodoItemModel> update)
    {
        Items.Value = Items.Value
            .Select(item => item.Id == itemId ? update(item) : item)
            .ToList();
    }

    void SelectItem(string itemId)
    {
        SelectedItemId.Value = itemId;
    }

    void ToggleItem(string itemId)
    {
        UpdateItem(itemId, item => item with { IsDone = !item.IsDone });
    }

    var selected = SelectedItem();

    render <Grid RowDefinitions="Auto,*" ColumnDefinitions="320,*" Margin={12}>
        <TextBlock Grid.Row={0} Grid.ColumnSpan={2} Text="Todo Board" FontSize={24} />
        <ScrollViewer Grid.Row={1} Grid.Column={0}>
            <StackPanel Spacing={8}>
                foreach (var item in Items.Value) {
                    <TodoCard
                        Key={item.Id}
                        Title={item.Title}
                        IsDone={item.IsDone}
                        IsSelected={item.Id == SelectedItemId.Value}
                        OnSelect={() => SelectItem(item.Id)}
                        OnToggle={() => ToggleItem(item.Id)} />
                }
            </StackPanel>
        </ScrollViewer>
        <TodoEditor
            Grid.Row={1}
            Grid.Column={1}
            ItemId={selected.Id}
            Title={selected.Title}
            Notes={selected.Notes}
            IsDone={selected.IsDone}
            OnTitleChanged={(itemId, value) => UpdateItem(itemId, item => item with { Title = value })}
            OnNotesChanged={(itemId, value) => UpdateItem(itemId, item => item with { Notes = value })}
            OnDoneChanged={(itemId, value) => UpdateItem(itemId, item => item with { IsDone = value })} />
    </Grid>;
}
```

`State<T>` invalidates the component when `Value` changes. In-place collection mutation does not automatically rerender; assign a new value or call `Touch()` after a deliberate in-place update.

## 6. Add keys to repeated children

The `Key` attribute tells the runtime which child identity should be retained across rerenders:

```csharp
<TodoCard Key={item.Id} ... />
```

Use stable keys for repeated component children when the order can change or item state should be preserved.

## 7. Test the component

`Csxaml.Testing` provides hostless rendering helpers for runtime tests. A test can render the component, query the logical tree, and trigger interactions without inspecting generated code.

Typical test shape:

```csharp
using var render = CsxamlTestHost.Render(new TodoBoard());

render.Click(render.FindByText("Select"));
render.EnterText(render.FindByAutomationId("SelectedTodoTitle"), "Updated title");

Assert.IsNotNull(render.TryFindByText("Updated title"));
```

Use semantic queries such as automation id, automation name, text, and content whenever possible.

## 8. What to read next

- [State and events](../language/state-and-events.md)
- [Native props and events](../guides/native-props-and-events.md)
- [Component testing](../guides/component-testing.md)
- [Runtime troubleshooting](../troubleshooting/runtime-behavior.md)
