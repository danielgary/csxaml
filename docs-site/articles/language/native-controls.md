---
title: Native Controls
description: How CSXAML projects native WinUI controls, properties, events, and attached properties.
---

# Native Controls

Native control tags project to WinUI controls through metadata-driven validation and runtime adapters.

> [!TIP]
> For the exact supported control, property, event, and attached-property table,
> see [Native Props and Events](../guides/native-props-and-events.md).

Example:

```csxaml
render <StackPanel Spacing={8}>
    <TextBlock Text="Title" />
    <Button Content="Save" OnClick={Save} />
</StackPanel>;
```

## Properties

Native properties can use literals or C# expressions:

```csxaml
<TextBlock Text={Title} FontSize={24} />
```

## Events

Event names use the normalized CSXAML event shape, such as `OnClick`, `OnTextChanged`, and `OnCheckedChanged`.

## Attached properties

Attached properties use dotted owner syntax:

```csxaml
<TextBlock Grid.Row={0} AutomationProperties.Name="Title" Text="Todo Board" />
```

The current supported attached-property surface is intentionally bounded. See [supported features](supported-features.md) for the exact release posture.
