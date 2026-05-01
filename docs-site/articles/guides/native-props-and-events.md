---
title: Native Props and Events
description: How CSXAML binds native WinUI properties, events, attached properties, and controlled inputs.
---

# Native Props and Events

Native property values can be literals or C# expressions:

```csxaml
<TextBlock Text="Static" />
<TextBlock Text={Title} />
<TextBlock Text={Description} TextWrapping={TextWrapping.Wrap} />
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

Single-value property content accepts at most one child. Collection property
content accepts multiple children when metadata describes the property as a
collection. Attribute assignment and property-content assignment to the same
property are intentionally not merged.

## Supported native controls

The current built-in metadata covers the controls used by the starter and Todo
flows plus an experimental common event-args slice:

| Tag | Common supported props |
| --- | --- |
| `AutoSuggestBox` | `Text`, `PlaceholderText`, `Margin`, `Width`, `Height`, `Style` |
| `Border` | `Background`, `BorderBrush`, `BorderThickness`, `Padding`, `Margin`, `Width`, `Height`, `Style` |
| `Button` | `Content`, `Background`, `Foreground`, `FontSize`, `Margin`, `Width`, `Height`, `Style` |
| `Canvas` | `Background`, `Margin`, `Width`, `Height`, `Style` |
| `CheckBox` | `Content`, `IsChecked`, `Margin`, `Width`, `Height`, `Style` |
| `Frame` | `Margin`, `Width`, `Height`, `Style` |
| `Grid` | `RowDefinitions`, `ColumnDefinitions`, `Background`, `Margin`, `Width`, `Height`, `Style` |
| `ListView` | `ItemsSource`, `IsItemClickEnabled`, `SelectionMode`, `Margin`, `Width`, `Height`, `Style` |
| `RelativePanel` | `Background`, `Margin`, `Width`, `Height`, `Style` |
| `ScrollViewer` | `Margin`, `Width`, `Height`, `Style`, `HorizontalScrollBarVisibility`, `HorizontalScrollMode`, `VerticalScrollBarVisibility`, `VerticalScrollMode` |
| `Slider` | `Minimum`, `Maximum`, `Value`, `Margin`, `Width`, `Height`, `Style` |
| `StackPanel` | `Spacing`, `Orientation`, `Background`, `Margin`, `Width`, `Height`, `Style` |
| `TextBlock` | `Text`, `Foreground`, `FontSize`, `TextWrapping`, `Margin`, `Width`, `Height`, `Style` |
| `TextBox` | `Text`, `PlaceholderText`, `AcceptsReturn`, `TextWrapping`, `MinHeight`, `Margin`, `Width`, `Height`, `Style` |
| `VariableSizedWrapGrid` | `Background`, `Margin`, `Width`, `Height`, `Style` |

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

`ListView` remains the recommended native virtualization surface for larger
collections. Keep lists out of parent `ScrollViewer` surfaces when the list
itself owns vertical scrolling. When a list needs explicit native scroll
configuration, use WinUI's `ScrollViewer.*` attached properties on the
`ListView`:

```csxaml
<ListView
    Height={220}
    ScrollViewer.HorizontalScrollMode={ScrollMode.Disabled}
    ScrollViewer.VerticalScrollBarVisibility={ScrollBarVisibility.Auto}
    ScrollViewer.VerticalScrollMode={ScrollMode.Enabled}
    ItemsSource={Rows} />
```

The built-in `ListView` adapter preserves native list state across rerenders
by leaving stable `ItemsSource`, item-click, and selection-mode values alone.

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
| Any supported control | `OnLoaded` | `Action<RoutedEventArgs>` | Experimental senderless native event args. |
| Any supported control | `OnKeyDown` | `Action<KeyRoutedEventArgs>` | Experimental senderless native event args. |
| Any supported control | Pointer events | `Action<PointerRoutedEventArgs>` | Covers pressed, released, moved, entered, exited, canceled, capture lost, and wheel changed. |
| `ListView` | `OnSelectionChanged` | `Action<SelectionChangedEventArgs>` | Experimental senderless native event args. |
| `ListView` | `OnItemClick` | `Action<ItemClickEventArgs>` | Experimental senderless native event args. |
| `Slider` | `OnValueChanged` | `Action<RangeBaseValueChangedEventArgs>` | Experimental senderless native event args. |
| `AutoSuggestBox` | `OnQuerySubmitted` | `Action<AutoSuggestBoxQuerySubmittedEventArgs>` | Experimental senderless native event args. |
| `AutoSuggestBox` | `OnSuggestionChosen` | `Action<AutoSuggestBoxSuggestionChosenEventArgs>` | Experimental senderless native event args. |
| `Frame` | Navigation events | `Action<TEventArgs>` | Covers navigating, navigated, navigation failed, and navigation stopped. |

## Element refs

`Ref` is a reserved native attribute for imperative WinUI interop:

```csxaml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

render <TextBox Ref={SearchBox} />;
```

The value must be an `ElementRef<T>` expression. The runtime sets it after
projection, keeps it across retained rerenders, updates it when the element is
replaced, and clears it when the element leaves the tree or the renderer is
disposed.

`Ref` is experimental, native-only behavior. Component refs and `x:Name`
symbolic lookup are not supported.

## Resources and object values

Native properties such as `Style`, `Background`, `Flyout`, or other object
valued APIs can be assigned from ordinary C# expressions when metadata exposes
the property:

```csxaml
render <Button Style={AppStyles.PrimaryButtonStyle} Content="Save" />;
```

That expression is a normal C# value. It is not a `StaticResource` lookup, a
`ThemeResource` subscription, or a XAML binding. Keep resource lookup,
template-heavy objects, and theme dictionaries in XAML resources when those
WinUI semantics matter. See
[Resources and Templates](resources-and-templates.md).

Attached properties use dotted syntax:

```csxaml
<TextBlock Grid.Row={0} AutomationProperties.Name="Title" Text="Todo Board" />
```

Supported attached properties:

| Owner | Properties |
| --- | --- |
| `AutomationProperties` | `AutomationId`, `Name`, `HelpText`, `ItemStatus`, `ItemType`, `LabeledBy` |
| `Canvas` | `Left`, `Top`, `ZIndex` |
| `Grid` | `Column`, `ColumnSpan`, `Row`, `RowSpan` |
| `RelativePanel` | `AlignLeftWithPanel`, `AlignTopWithPanel`, `Below`, `RightOf` |
| `ScrollViewer` | `HorizontalScrollBarVisibility`, `HorizontalScrollMode`, `VerticalScrollBarVisibility`, `VerticalScrollMode` |
| `ToolTipService` | `ToolTip` |
| `VariableSizedWrapGrid` | `ColumnSpan`, `RowSpan` |

## Common validation failures

Event values must be C# expressions, not string method names:

```csxaml
// Invalid
<Button Content="Save" OnClick="Save" />

// Valid
<Button Content="Save" OnClick={Save} />
```

Layout attached properties with parent requirements are only valid where the
current supported metadata can resolve the expected parent relationship:

```csxaml
// Invalid
render <StackPanel>
    <TextBlock Grid.Row={0} Text="Title" />
</StackPanel>;

// Valid
render <Grid>
    <TextBlock Grid.Row={0} Text="Title" />
</Grid>;

// Valid
render <Canvas>
    <Button Canvas.Left={20} Canvas.Top={12} Content="Move" />
</Canvas>;
```

If a property or event fails validation, check
[`ControlMetadataRegistry`](xref:Csxaml.ControlMetadata.ControlMetadataRegistry),
the [metadata API](../api/metadata.md), and the
[supported features](../language/supported-features.md) page before assuming the
WinUI API shape is available through CSXAML.
