---
title: Syntax
description: Practical syntax guide for CSXAML component files.
---

# Syntax

## File shape

```csharp
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element HelloCard(string Title) {
    render <TextBlock Text={Title} />;
}
```

## Component declaration

```csharp
component Element Name(Type Prop, Type OtherProp) {
    render <Root />;
}
```

The root render statement must use `render <Root />;`. Returning markup with `return` is not valid CSXAML syntax.

## Attributes

String values can be written as literals:

```csharp
<TextBlock Text="Hello" />
```

C# expressions are wrapped in braces:

```csharp
<TextBlock Text={Title} />
```

## Children

Native and component tags can contain children:

```csharp
<StackPanel>
    <TextBlock Text="Title" />
    <Button Content="Save" />
</StackPanel>
```

## Control flow

`if` and `foreach` are supported inside markup children:

```csharp
if (Items.Value.Count == 0) {
    <TextBlock Text="Nothing yet" />
}

foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

## Aliased tags

File-level using aliases can qualify external controls:

```csharp
using WinUi = Microsoft.UI.Xaml.Controls;

render <WinUi:InfoBar IsOpen={true} Title="Ready" />;
```
