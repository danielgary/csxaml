---
title: Performance and Scale
description: Current scale expectations and performance boundaries for CSXAML apps.
---

# Performance and Scale

CSXAML v1 optimizes for predictable retained rendering, not for hidden virtualization.

Current performance posture:

- generated code should remain deterministic and easy to inspect
- keyed child identity is retained where explicitly requested
- repeated visible children still represent real render work
- `foreach` is not a virtualization mechanism
- state equality suppresses no-op rerenders
- `Touch()` exists for deliberate in-place mutation cases

Use stable keys for repeated children and keep rendered lists within the scale that the current runtime supports. For very large visible lists, use host-level WinUI virtualization patterns rather than assuming CSXAML adds virtualization automatically.

Benchmark automation lives in the repo scripts and artifact baselines. Treat benchmark results as environment-sensitive unless the gate explicitly marks them as blocking.

## Key repeated children

Use stable keys when child order can change:

```csxaml
foreach (var item in Items.Value) {
    <TodoCard
        Key={item.Id}
        Title={item.Title}
        IsDone={item.IsDone} />
}
```

Do not use the loop index as the key when items can be inserted, removed, or
reordered. The key should describe the item identity, not the current position.

## Mutating state in place

Replacing the state value is the clearest path:

```csharp
Items.Value = Items.Value.Append(newItem).ToArray();
```

If you intentionally mutate the existing object or list in place, call
`Touch()` so the rerender is explicit:

```csharp
Items.Value.Add(newItem);
Items.Touch();
```

## Large visible lists

This is fine for small or moderate visible lists:

```csxaml
foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} />
}
```

Do not use that shape as a substitute for virtualization with thousands of
visible rows. Put very large scrolling surfaces behind a WinUI control or
external control that owns virtualization, then pass the data into that control:

```csxaml
render <VirtualizedTodoList ItemsSource={Items.Value} />;
```

The current benchmark gate covers hostless 1000-item keyed rerender scenarios.
That is a retained-tree stress proof, not a promise that plain markup loops are
the right UI shape for unbounded datasets.
