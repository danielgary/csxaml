---
title: Lifecycle and Async
description: Lifecycle and async behavior for CSXAML runtime components.
---

# Lifecycle and Async

Lifecycle support is intentionally small and runtime-centered.

Supported behavior:

- retained children mount once while their identity is preserved
- removed children are disposed
- async root disposal is supported
- stale state invalidation after unmount does not rerender the tree
- render-phase state writes fail rather than recursively rerendering

Handwritten component types can use:

- `OnMounted()`
- `IDisposable`
- `IAsyncDisposable`

Generated `.csxaml` files do not currently have dedicated lifecycle syntax. Put async work behind explicit services, update state deliberately, and keep cancellation or disposal ownership visible in normal C# code.

If an async continuation runs after a component has unmounted, it must not resurrect the disposed component instance.

## Async loading in `.csxaml`

Use ordinary async methods and explicit state. Keep stale-result handling visible
in the component instead of assuming the runtime cancels the work for you:

```csxaml
component Element TodoLoader {
    inject ITodoService TodoService;

    State<bool> IsLoading = new State<bool>(false);
    State<int> LoadVersion = new State<int>(0);
    State<IReadOnlyList<TodoItem>> Items = new State<IReadOnlyList<TodoItem>>([]);

    async Task RefreshAsync()
    {
        var version = LoadVersion.Value + 1;
        LoadVersion.Value = version;
        IsLoading.Value = true;

        var items = await TodoService.LoadAsync();
        if (LoadVersion.Value != version) {
            return;
        }

        Items.Value = items;
        IsLoading.Value = false;
    }

    render <StackPanel Spacing={8}>
        <Button Content="Refresh" OnClick={async () => await RefreshAsync()} />
        if (IsLoading.Value) {
            <TextBlock Text="Loading..." />
        }
        foreach (var item in Items.Value) {
            <TextBlock Key={item.Id} Text={item.Title} />
        }
    </StackPanel>;
}
```

The version check prevents an older request from overwriting newer state. If the
component is removed before the request finishes, later state writes still must
not remount the disposed instance.

## Handwritten lifecycle hook

Use `OnMounted()` only from handwritten runtime components:

```csharp
internal sealed class MountedProbeComponent : ComponentInstance
{
    private int _mountCount;

    protected override void OnMounted()
    {
        _mountCount++;
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            key: null,
            properties: [new NativePropertyValue("Text", $"Mounted:{_mountCount}")],
            events: [],
            children: []);
    }
}
```

For cleanup, implement `IDisposable` or `IAsyncDisposable` on the handwritten
component type. The runtime calls cleanup when reconciliation removes the
component or when the host/coordinator is disposed.
