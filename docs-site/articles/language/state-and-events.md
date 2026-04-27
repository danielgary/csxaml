---
title: State and Events
description: How state invalidation and event handlers work in CSXAML components.
---

# State and Events

## State

State is explicit:

```csharp
State<string> Name = new State<string>("Draft");
```

Read and write the current value through `Value`:

```csharp
<TextBlock Text={Name.Value} />
```

Assigning `Value` invalidates the component when the value changes:

```csharp
Name.Value = "Updated";
```

For reference types, assigning the same reference is a no-op. If you deliberately mutate in place, call `Touch()`:

```csharp
Items.Value.Add(newItem);
Items.Touch();
```

Prefer assigning a new collection when possible because it makes render causes easier to understand.

## Events

CSXAML events are C# delegates:

```csharp
void Save()
{
    // Save work here.
}

render <Button Content="Save" OnClick={Save} />;
```

Inline lambdas are also supported:

```csharp
<Button Content="Increment" OnClick={() => Count.Value++} />
```

## Controlled inputs

Controlled input keeps state ownership in the component:

```csharp
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={value => Name.Value = value} />;
```

Supported projected input events include `OnTextChanged` for `TextBox` and `OnCheckedChanged` for `CheckBox`.
