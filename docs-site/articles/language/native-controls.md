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

Experimental property-content syntax assigns child elements to metadata-backed
native properties:

```csxaml
<Border>
    <Border.Child>
        <TextBlock Text="Title" />
    </Border.Child>
</Border>
```

The property-content block is an assignment to `Border.Child`; it does not
create a `Border.Child` native element. The property must be known metadata, a
single-value property can receive at most one child, and attribute assignment
to the same property is a diagnostic.

`Ref={...}` is a reserved experimental native attribute for assigning a
`Csxaml.Runtime.ElementRef<T>` imperative handle. It is not a normal WinUI
property and it is not supported on component tags.

## Events

Event names use the normalized CSXAML event shape, such as `OnClick`,
`OnTextChanged`, `OnCheckedChanged`, `OnKeyDown`, and `OnValueChanged`.
The experimental typed event-args set uses senderless `Action<TEventArgs>`
handlers where metadata documents that shape.

## Attached properties

Attached properties use dotted owner syntax:

```csxaml
<TextBlock Grid.Row={0} AutomationProperties.Name="Title" Text="Todo Board" />

<Canvas>
    <Button Canvas.Left={20} Canvas.Top={12} Content="Move" />
</Canvas>
```

The current supported attached-property surface is intentionally bounded. The
experimental expanded owner set covers `Grid`, `Canvas`, `RelativePanel`,
`ToolTipService`, `VariableSizedWrapGrid`, and a practical
`AutomationProperties` surface. See [supported features](supported-features.md)
for the exact release posture.
