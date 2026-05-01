---
title: State and Events
description: How state invalidation and event handlers work in CSXAML components.
---

# State and Events

## State

State is explicit:

```csxaml
State<string> Name = new State<string>("Draft");
```

Read and write the current value through `Value`:

```csxaml
<TextBlock Text={Name.Value} />
```

Assigning `Value` invalidates the component when the value changes:

```csharp
Name.Value = "Updated";
```

For value types, the runtime uses the type's normal equality check. For
reference types, assigning the exact same object reference is a no-op. In plain
terms: if the state container can tell the value changed, it rerenders; if you
mutate the same object in place, you must say so explicitly.

`Count.Value++` is valid for simple value state:

```csxaml
State<int> Count = new State<int>(0);

render <Button
    Content={$"Clicked {Count.Value} times"}
    OnClick={() => Count.Value++} />;
```

For collections and other reference types, prefer assigning a new value:

```csharp
// Avoid: mutates the same list reference and does not rerender by itself.
Items.Value.Add(newItem);

// Better: assigns a new list reference and rerenders.
Items.Value = Items.Value.Append(newItem).ToList();
```

If you deliberately mutate in place, call `Touch()`:

```csharp
Items.Value.Add(newItem);
Items.Touch();
```

`Touch()` is an escape hatch for controlled mutation. If a component appears to
keep old UI after a list or object update, see [runtime troubleshooting](../troubleshooting/runtime-behavior.md).

## Events

CSXAML events are C# delegates:

```csxaml
void Save()
{
    // Save work here.
}

render <Button Content="Save" OnClick={Save} />;
```

Inline lambdas are also supported:

```csxaml
<Button Content="Increment" OnClick={() => Count.Value++} />
```

Some experimental native events expose senderless WinUI event args:

```csxaml
<Slider OnValueChanged={args => Volume.Value = args.NewValue} />
<TextBox OnKeyDown={args => SubmitOnEnter(args)} />
```

The handler receives the event args value, not the sender. Use the delegate
shape listed in the native props and events guide for each event. If component
code needs the sender element, use a native `ElementRef<T>`:

```csxaml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

render <TextBox
    Ref={SearchBox}
    OnLoaded={_ => SearchBox.Current?.Focus(FocusState.Programmatic)} />;
```

## Controlled inputs

Controlled input keeps state ownership in the component:

```csxaml
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={value => Name.Value = value} />;
```

Supported projected input events include `OnTextChanged` for `TextBox` and `OnCheckedChanged` for `CheckBox`.
