---
title: Runtime Behavior Troubleshooting
description: How to diagnose retained rendering, state, event, and lifecycle behavior.
---

# Runtime Behavior Troubleshooting

## State did not rerender

Check whether the new value is actually different. `State<T>` suppresses no-op invalidation for equal values and same-reference assignments.

For in-place mutation:

```csharp
Items.Value.Add(item);
Items.Touch();
```

Prefer assigning a new collection when practical.

## Repeated child state moved to the wrong item

Use stable `Key` values for repeated component children:

```csharp
<TodoCard Key={item.Id} ... />
```

Keys must be unique among siblings.

## Event handler does not run

Check that the attribute is a supported event and the delegate type matches the expected event shape. For example, `OnClick` expects `Action`, while `OnTextChanged` expects `Action<string>`.

## Removed component still receives async work

Late async continuations can still execute ordinary C# code, but state invalidation after unmount must not resurrect or rerender the removed component.
