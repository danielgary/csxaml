# Native Props And Events

This guide covers the built-in WinUI control slice that CSXAML validates and renders directly.

For the feature status table, see [supported-feature-matrix.md](supported-feature-matrix.md). For project-reference or package-provided controls, see [external-control-interop.md](external-control-interop.md).

## Supported Built-In Controls

| Control | Child content | Properties | Events |
| --- | --- | --- | --- |
| `Border` | Single child | `Background`, `BorderBrush`, `BorderThickness`, `Height`, `HorizontalAlignment`, `Margin`, `Padding`, `Style`, `VerticalAlignment`, `Width` | none |
| `Button` | None | `Background`, `Content`, `FontSize`, `Foreground`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | `OnClick` |
| `CheckBox` | None | `Content`, `Height`, `HorizontalAlignment`, `IsChecked`, `Margin`, `Style`, `VerticalAlignment`, `Width` | `OnCheckedChanged` |
| `Grid` | Multiple children | `Background`, `ColumnDefinitions`, `Height`, `HorizontalAlignment`, `Margin`, `RowDefinitions`, `Style`, `VerticalAlignment`, `Width` | none |
| `ScrollViewer` | Single child | `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | none |
| `StackPanel` | Multiple children | `Background`, `Height`, `HorizontalAlignment`, `Margin`, `Orientation`, `Spacing`, `Style`, `VerticalAlignment`, `Width` | none |
| `TextBlock` | None | `FontSize`, `Foreground`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `Text`, `VerticalAlignment`, `Width` | none |
| `TextBox` | None | `AcceptsReturn`, `Height`, `HorizontalAlignment`, `Margin`, `MinHeight`, `PlaceholderText`, `Style`, `Text`, `TextWrapping`, `VerticalAlignment`, `Width` | `OnTextChanged` |

Unknown attributes fail during generation with source diagnostics. The supported set is intentionally explicit rather than a promise that every WinUI property is available.

## Literal And Expression Values

Attribute values are either string literals or C# expressions:

```xml
<TextBlock Text="Ready" />
<TextBlock Text={Title.ToUpperInvariant()} />
<Border Padding={12} Background={TodoColors.DoneBackground} />
```

String literals are accepted only where the metadata says the property is `string` or `object`.

Expressions are accepted for every supported property and are required for:

- events
- `bool`, `int`, `double`, enum, `Thickness`, `Brush`, and `Style` values
- `Grid.Row`, `Grid.Column`, `Grid.RowSpan`, and `Grid.ColumnSpan`

CSXAML does not treat `{...}` as XAML binding or markup-extension syntax. The contents are emitted as C#.

## Value Kinds

The generator records a value-kind hint for each native property. Runtime adapters use that hint to apply a small, predictable set of conversions.

| Value kind | Accepted values |
| --- | --- |
| `string` | `string` literals or expressions. Null values are generally normalized to an empty control value by text-oriented adapters. |
| `bool` | `bool` expressions. `CheckBox.IsChecked` also accepts nullable bool through the WinUI property shape, with null normalized to `false` by the current controlled-input adapter. |
| `int` | Integral expressions that fit in `int`: `byte`, `short`, `int`, or in-range `long`. |
| `double` | Numeric expressions: `byte`, `short`, `int`, `long`, `float`, `double`, or `decimal`. |
| `enum` | An actual enum value, such as `Orientation.Horizontal` or `TextWrapping.Wrap`. String enum names are not parsed. |
| `Thickness` | A `Thickness` value or a numeric expression for uniform thickness. |
| `Brush` | A WinUI `Brush`, a CSXAML `ArgbColor`, or `null`. |
| `Style` | A WinUI `Style`, a CSXAML `DeferredStyle`, or `null`. |
| `object` | Any expression value. String literals are also accepted. `Grid.RowDefinitions` and `Grid.ColumnDefinitions` additionally accept strings such as `"Auto,*"` or C# sequences of `GridLength`. |

If a value cannot be converted to the target shape, the runtime wraps the failure with CSXAML component, tag, and member context. See [debugging-and-diagnostics.md](debugging-and-diagnostics.md).

## Events

Built-in event names are projected names, not raw WinUI event names.

| CSXAML event | Control | Handler type | Behavior |
| --- | --- | --- | --- |
| `OnClick` | `Button` | `Action` | Invoked when the WinUI `Click` event fires. |
| `OnTextChanged` | `TextBox` | `Action<string>` | Invoked with the current text. |
| `OnCheckedChanged` | `CheckBox` | `Action<bool>` | Invoked with the current checked state, normalized to `false` for null. |

Events require expression values:

```xml
<Button Content="Save" OnClick={Save} />
<TextBox Text={Title.Value} OnTextChanged={value => Title.Value = value} />
<CheckBox IsChecked={IsDone.Value} OnCheckedChanged={value => IsDone.Value = value} />
```

The current built-in event slice does not expose routed event args. If a control or event is outside the built-in slice, use the external-control path described in [external-control-interop.md](external-control-interop.md).

## Controlled Inputs

`TextBox` and `CheckBox` are controlled controls in the current model:

- the rendered property (`Text` or `IsChecked`) is the source of truth for the native control value
- the projected event reports user changes back to component code
- component code should update `State<T>` or another backing value in the event handler
- rerendering patches the retained native control instead of replacing it when identity is stable

Typical text input:

```xml
<TextBox
    Text={Title.Value}
    PlaceholderText="Title"
    OnTextChanged={value => Title.Value = value} />
```

Typical checkbox input:

```xml
<CheckBox
    Content="Done"
    IsChecked={IsDone.Value}
    OnCheckedChanged={value => IsDone.Value = value} />
```

The runtime avoids writing identical controlled values back into retained inputs and restores `TextBox` selection when a controlled text patch changes the value.

## Attached Properties

Supported attached properties are built into metadata:

| Attached property | Value kind | Placement |
| --- | --- | --- |
| `Grid.Row` | `int` expression | Direct child of `Grid` |
| `Grid.Column` | `int` expression | Direct child of `Grid` |
| `Grid.RowSpan` | `int` expression | Direct child of `Grid` |
| `Grid.ColumnSpan` | `int` expression | Direct child of `Grid` |
| `AutomationProperties.Name` | `string` literal or expression | Any supported projected element |
| `AutomationProperties.AutomationId` | `string` literal or expression | Any supported projected element |

Example:

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="260,*">
    <TextBlock Grid.Row={0} Grid.ColumnSpan={2} Text="Tasks" />
    <TextBox
        Grid.Row={1}
        Grid.Column={1}
        AutomationProperties.AutomationId="SelectedTodoTitle"
        Text={SelectedTitle.Value}
        OnTextChanged={value => SelectedTitle.Value = value} />
</Grid>
```

External attached-property owner discovery is not part of the current v1 slice.

## Child Content Rules

Native child rules are validated before code is emitted:

- `Grid` and `StackPanel` allow multiple children.
- `Border` and `ScrollViewer` allow zero or one child.
- `Button`, `CheckBox`, `TextBlock`, and `TextBox` do not allow child elements.

Component child content is separate from native child content. A component must define a default `<Slot />` outlet before callers can pass children.

## Unsupported Areas

These are intentionally outside the current built-in surface:

- arbitrary WinUI controls without the external-control metadata path
- broad XAML syntax such as `{Binding}`, `{StaticResource}`, or markup extensions
- string parsing for enum, bool, thickness, brush, or style values
- routed event args in built-in event handlers
- command binding conventions
- external attached-property owner discovery
- child content on childless controls
- virtualization or item-control behavior from `foreach`

