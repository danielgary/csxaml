# Native Props And Events

This guide covers the built-in WinUI control slice that CSXAML validates and renders directly.

For the feature status table, see [supported-feature-matrix.md](supported-feature-matrix.md). For project-reference or package-provided controls, see [external-control-interop.md](external-control-interop.md).

## Supported Built-In Controls

| Control | Child content | Properties | Events |
| --- | --- | --- | --- |
| `AutoSuggestBox` | None | `Height`, `HorizontalAlignment`, `Margin`, `PlaceholderText`, `Style`, `Text`, `VerticalAlignment`, `Width` | `OnQuerySubmitted`, `OnSuggestionChosen`, common event-args events |
| `Border` | Single child | `Background`, `BorderBrush`, `BorderThickness`, `Height`, `HorizontalAlignment`, `Margin`, `Padding`, `Style`, `VerticalAlignment`, `Width` | common event-args events |
| `Button` | None | `Background`, `Content`, `FontSize`, `Foreground`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | `OnClick`, common event-args events |
| `Canvas` | Multiple children | `Background`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | common event-args events |
| `CheckBox` | None | `Content`, `Height`, `HorizontalAlignment`, `IsChecked`, `Margin`, `Style`, `VerticalAlignment`, `Width` | `OnCheckedChanged`, common event-args events |
| `Frame` | None | `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | navigation events, common event-args events |
| `Grid` | Multiple children | `Background`, `ColumnDefinitions`, `Height`, `HorizontalAlignment`, `Margin`, `RowDefinitions`, `Style`, `VerticalAlignment`, `Width` | common event-args events |
| `ListView` | None | `Height`, `HorizontalAlignment`, `IsItemClickEnabled`, `ItemsSource`, `Margin`, `SelectionMode`, `Style`, `VerticalAlignment`, `Width` | `OnSelectionChanged`, `OnItemClick`, common event-args events |
| `RelativePanel` | Multiple children | `Background`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | common event-args events |
| `ScrollViewer` | Single child | `Height`, `HorizontalAlignment`, `HorizontalScrollBarVisibility`, `HorizontalScrollMode`, `Margin`, `Style`, `VerticalAlignment`, `VerticalScrollBarVisibility`, `VerticalScrollMode`, `Width` | common event-args events |
| `Slider` | None | `Height`, `HorizontalAlignment`, `Margin`, `Maximum`, `Minimum`, `Style`, `Value`, `VerticalAlignment`, `Width` | `OnValueChanged`, common event-args events |
| `StackPanel` | Multiple children | `Background`, `Height`, `HorizontalAlignment`, `Margin`, `Orientation`, `Spacing`, `Style`, `VerticalAlignment`, `Width` | common event-args events |
| `TextBlock` | None | `FontSize`, `Foreground`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `Text`, `TextWrapping`, `VerticalAlignment`, `Width` | common event-args events |
| `TextBox` | None | `AcceptsReturn`, `Height`, `HorizontalAlignment`, `Margin`, `MinHeight`, `PlaceholderText`, `Style`, `Text`, `TextWrapping`, `VerticalAlignment`, `Width` | `OnTextChanged`, common event-args events |
| `VariableSizedWrapGrid` | Multiple children | `Background`, `Height`, `HorizontalAlignment`, `Margin`, `Style`, `VerticalAlignment`, `Width` | common event-args events |

Unknown attributes fail during generation with source diagnostics. The supported set is intentionally explicit rather than a promise that every WinUI property is available.

Use `TextWrapping={TextWrapping.Wrap}` on `TextBlock` for long labels or
paragraph text. WinUI only wraps when the parent gives the text a finite width;
horizontal stacks and horizontally scrollable regions can still measure text
with more width than the viewport.

For text-heavy pages hosted in a `ScrollViewer`, disable horizontal scrolling
when you expect wrapping and native horizontal gestures to stay inside the
viewport:

```csxaml
<ScrollViewer
    HorizontalScrollBarVisibility={ScrollBarVisibility.Disabled}
    HorizontalScrollMode={ScrollMode.Disabled}
    VerticalScrollBarVisibility={ScrollBarVisibility.Auto}
    VerticalScrollMode={ScrollMode.Auto}>
    <TextBlock Text={LongText} TextWrapping={TextWrapping.Wrap} />
</ScrollViewer>
```

## Property Content

Experimental property-content syntax assigns child elements to a
metadata-backed native property:

```csxaml
<Border>
    <Border.Child>
        <TextBlock Text="Title" />
    </Border.Child>
</Border>
```

The owner before the dot must match the containing tag. Single-value properties
accept at most one child, collection properties accept multiple children when
metadata describes the collection, and assigning the same property through both
an attribute and property content is a diagnostic.

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
- numeric or boolean attached properties such as `Grid.Row`, `Canvas.Left`,
  `Canvas.ZIndex`, `RelativePanel.AlignLeftWithPanel`, and
  `VariableSizedWrapGrid.ColumnSpan`

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
| `OnLoaded` | Any supported control | `Action<RoutedEventArgs>` | Experimental senderless native event args. |
| `OnKeyDown` | Any supported control | `Action<KeyRoutedEventArgs>` | Experimental senderless native event args. |
| Pointer events | Any supported control | `Action<PointerRoutedEventArgs>` | Covers pressed, released, moved, entered, exited, canceled, capture lost, and wheel changed. |
| `OnSelectionChanged` | `ListView` | `Action<SelectionChangedEventArgs>` | Experimental senderless native event args. |
| `OnValueChanged` | `Slider` | `Action<RangeBaseValueChangedEventArgs>` | Experimental senderless native event args. |
| Autosuggest events | `AutoSuggestBox` | `Action<TEventArgs>` | Query submitted and suggestion chosen. |
| Frame navigation events | `Frame` | `Action<TEventArgs>` | Navigating, navigated, navigation failed, and navigation stopped. |

Events require expression values:

```xml
<Button Content="Save" OnClick={Save} />
<TextBox Text={Title.Value} OnTextChanged={value => Title.Value = value} />
<CheckBox IsChecked={IsDone.Value} OnCheckedChanged={value => IsDone.Value = value} />
```

Typed event-args handlers receive the event args value and omit the sender. If a
handler needs the element itself, use `Ref={...}` with `ElementRef<T>`.

## Element Refs

`Ref` is a reserved native attribute for focus, scrolling, animation targets,
and other imperative WinUI interop:

```xml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

render <TextBox Ref={SearchBox} />;
```

The ref value must be an expression assignable to `Csxaml.Runtime.ElementRef`.
The runtime sets the ref after projection, preserves it across retained
rerenders, updates it when an element is replaced, and clears it when the
element is removed or the renderer is disposed.

This is experimental native-element behavior. Component refs and `x:Name`
symbolic lookup are not supported.

## Resources And Object Values

When metadata exposes an object-valued property, assign a normal C# expression:

```xml
<Button Style={AppStyles.PrimaryButtonStyle} Content="Save" />
```

That expression assigns the value returned by C#. It does not become
`StaticResource`, `ThemeResource`, or `{Binding}`. Keep XAML dictionaries for
deep resource graphs, templates, and theme invalidation behavior.

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
| `AutomationProperties.HelpText` | `string` literal or expression | Any supported projected element |
| `AutomationProperties.ItemStatus` | `string` literal or expression | Any supported projected element |
| `AutomationProperties.ItemType` | `string` literal or expression | Any supported projected element |
| `AutomationProperties.LabeledBy` | object expression | Any supported projected element |
| `Canvas.Left` | `double` expression | Direct child of `Canvas` |
| `Canvas.Top` | `double` expression | Direct child of `Canvas` |
| `Canvas.ZIndex` | `int` expression | Direct child of `Canvas` |
| `RelativePanel.AlignLeftWithPanel` | `bool` expression | Direct child of `RelativePanel` |
| `RelativePanel.AlignTopWithPanel` | `bool` expression | Direct child of `RelativePanel` |
| `RelativePanel.RightOf` | object literal or expression | Direct child of `RelativePanel` |
| `RelativePanel.Below` | object literal or expression | Direct child of `RelativePanel` |
| `ToolTipService.ToolTip` | string literal or expression | Any supported projected element |
| `VariableSizedWrapGrid.ColumnSpan` | `int` expression | Direct child of `VariableSizedWrapGrid` |
| `VariableSizedWrapGrid.RowSpan` | `int` expression | Direct child of `VariableSizedWrapGrid` |

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

Expanded layout owners are experimental but usable:

```xml
<Canvas>
    <Button Canvas.Left={20} Canvas.Top={12} Canvas.ZIndex={2} Content="Move" />
</Canvas>

<Border ToolTipService.ToolTip="Saved" />
```

External attached-property owner discovery is not part of the current v1 slice.

## Child Content Rules

Native child rules are validated before code is emitted:

- `Canvas`, `Grid`, `RelativePanel`, `StackPanel`, and
  `VariableSizedWrapGrid` allow multiple children.
- `Border` and `ScrollViewer` allow zero or one child.
- `AutoSuggestBox`, `Button`, `CheckBox`, `Frame`, `ListView`, `Slider`,
  `TextBlock`, and `TextBox` do not allow child elements.

Component child content is separate from native child content. A component must define a default `<Slot />` outlet before callers can pass children.

## Unsupported Areas

These are intentionally outside the current built-in surface:

- arbitrary WinUI controls without the external-control metadata path
- broad XAML syntax such as `{Binding}`, `{StaticResource}`, or markup extensions
- string parsing for enum, bool, thickness, brush, or style values
- open-ended event args beyond the documented experimental event set
- component refs or `x:Name` symbolic lookup
- command binding conventions
- external attached-property owner discovery
- child content on childless controls
- virtualization or item-control behavior from `foreach`

