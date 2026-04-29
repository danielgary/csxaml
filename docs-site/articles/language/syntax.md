---
title: Syntax
description: Practical syntax guide for CSXAML component files.
---

# Syntax

## File shape

```csxaml
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element HelloCard(string Title) {
    render <TextBlock Text={Title} />;
}
```

## Component declaration

```csxaml
component Element Name(Type Prop, Type OtherProp) {
    render <Root />;
}
```

The root render statement must use `render <Root />;`. Returning markup with `return` is not valid CSXAML syntax.

## Common invalid syntax

Use `render`, not `return`, for the final markup statement:

```csxaml
// Invalid
return <StackPanel />;

// Valid
render <StackPanel />;
```

Read a state value through `.Value`:

```csxaml
State<int> Count = new State<int>(0);

// Invalid or unintended
<TextBlock Text={Count} />

// Usually intended
<TextBlock Text={Count.Value} />
```

Attached-property values that are not strings use C# expressions:

```csxaml
// Invalid
<TextBlock Grid.Row="1" Text="Title" />

// Valid
<TextBlock Grid.Row={1} Text="Title" />
```

## Attributes

String values can be written as literals:

```csxaml
<TextBlock Text="Hello" />
```

C# expressions are wrapped in braces:

```csxaml
<TextBlock Text={Title} />
```

## Children

Native and component tags can contain children:

```csxaml
<StackPanel>
    <TextBlock Text="Title" />
    <Button Content="Save" />
</StackPanel>
```

## Control flow

`if` and `foreach` are supported inside markup children:

```csxaml
if (Items.Value.Count == 0) {
    <TextBlock Text="Nothing yet" />
}

foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

## Aliased tags

File-level using aliases can qualify external controls:

```csxaml
using WinUi = Microsoft.UI.Xaml.Controls;

render <WinUi:InfoBar IsOpen={true} Title="Ready" />;
```
