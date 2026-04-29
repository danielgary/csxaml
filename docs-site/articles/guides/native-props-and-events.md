---
title: Native Props and Events
description: How CSXAML binds native WinUI properties, events, attached properties, and controlled inputs.
---

# Native Props and Events

Native property values can be literals or C# expressions:

```csxaml
<TextBlock Text="Static" />
<TextBlock Text={Title} />
```

## Supported native controls

The current built-in metadata covers the controls used by the starter and Todo
flows:

| Tag | Common supported props |
| --- | --- |
| `Border` | `Background`, `BorderBrush`, `BorderThickness`, `Padding`, `Margin`, `Width`, `Height`, `Style` |
| `Button` | `Content`, `Background`, `Foreground`, `FontSize`, `Margin`, `Width`, `Height`, `Style` |
| `CheckBox` | `Content`, `IsChecked`, `Margin`, `Width`, `Height`, `Style` |
| `Grid` | `RowDefinitions`, `ColumnDefinitions`, `Background`, `Margin`, `Width`, `Height`, `Style` |
| `ScrollViewer` | `Margin`, `Width`, `Height`, `Style` |
| `StackPanel` | `Spacing`, `Orientation`, `Background`, `Margin`, `Width`, `Height`, `Style` |
| `TextBlock` | `Text`, `Foreground`, `FontSize`, `Margin`, `Width`, `Height`, `Style` |
| `TextBox` | `Text`, `PlaceholderText`, `AcceptsReturn`, `TextWrapping`, `MinHeight`, `Margin`, `Width`, `Height`, `Style` |

Events use normalized CSXAML names:

```csxaml
<Button Content="Save" OnClick={Save} />
<Button Content="Refresh" OnClick={() => Reload()} />
```

Controlled input is explicit:

```csxaml
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={text => Name.Value = text} />;
```

Supported projected events include:

| Control | Event | Delegate shape | Notes |
| --- | --- | --- | --- |
| `Button` | `OnClick` | `Action` | Direct command-style handler. |
| `TextBox` | `OnTextChanged` | `Action<string>` | Receives the current text value. |
| `CheckBox` | `OnCheckedChanged` | `Action<bool>` | Receives the current checked value. |

Attached properties use dotted syntax:

```csxaml
<TextBlock Grid.Row={0} AutomationProperties.Name="Title" Text="Todo Board" />
```

Supported attached properties:

| Owner | Properties |
| --- | --- |
| `AutomationProperties` | `AutomationId`, `Name` |
| `Grid` | `Column`, `ColumnSpan`, `Row`, `RowSpan` |

## Common validation failures

Event values must be C# expressions, not string method names:

```csxaml
// Invalid
<Button Content="Save" OnClick="Save" />

// Valid
<Button Content="Save" OnClick={Save} />
```

Grid row and column attached properties are only valid where the current
supported metadata can resolve the `Grid` parent relationship:

```csxaml
// Invalid
render <StackPanel>
    <TextBlock Grid.Row={0} Text="Title" />
</StackPanel>;

// Valid
render <Grid>
    <TextBlock Grid.Row={0} Text="Title" />
</Grid>;
```

If a property or event fails validation, check
[`ControlMetadataRegistry`](xref:Csxaml.ControlMetadata.ControlMetadataRegistry),
the [metadata API](../api/metadata.md), and the
[supported features](../language/supported-features.md) page before assuming the
WinUI API shape is available through CSXAML.
