---
title: Runtime Behavior Troubleshooting
description: How to diagnose retained rendering, state, event, and lifecycle behavior.
---

# Runtime Behavior Troubleshooting

## Symptom: state changed but the UI did not rerender

Likely causes:

1. The new value is equal to the old value.
2. A reference type was mutated in place.
3. External state changed outside a CSXAML event callback.

For in-place mutation:

```csharp
Items.Value.Add(item);
Items.Touch();
```

Prefer assigning a new collection when practical:

```csharp
Items.Value = Items.Value.Append(item).ToList();
```

In tests, call `Rerender()` when changing external state outside an interaction
callback.

## Symptom: repeated child state moved to the wrong item

Use stable `Key` values for repeated component children:

```csxaml
<TodoCard Key={item.Id} ... />
```

Keys must be unique among siblings.

## Symptom: event handler does not run

Likely causes:

1. The attribute is not a supported event for that control.
2. The event value is a string instead of a C# expression.
3. The delegate shape does not match the event.

Common expected shapes:

| Event | Delegate |
| --- | --- |
| `OnClick` | `Action` |
| `OnTextChanged` | `Action<string>` |
| `OnCheckedChanged` | `Action<bool>` |
| `OnKeyDown` | `Action<KeyRoutedEventArgs>` |
| `OnValueChanged` | `Action<RangeBaseValueChangedEventArgs>` |

For typed event-args handlers, CSXAML passes the event args value and omits the
sender.

## Symptom: an element ref is null

Likely causes:

1. The element has not been projected yet.
2. The element was removed by an `if` branch or repeated-child update.
3. The root renderer or host was disposed.
4. `Ref` was placed on a component tag instead of a native control tag.

`ElementRef<T>.Current` is intentionally nullable. Use `TryGet` or null checks
around focus, scrolling, and animation interop.

## Symptom: removed component still receives async work

Late async continuations can still execute ordinary C# code, but state invalidation after unmount must not resurrect or rerender the removed component.

If the continuation updates component state after unmount, check the lifecycle
and disposal flow around the component that started the work.
