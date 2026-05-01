# Native Props And Events

CSXAML treats native WinUI props and events as metadata-driven surface, not as open-ended late-bound strings.

That gives the language a few important properties:

- native control/property/event names stay recognizable to WinUI authors
- invalid props and events fail at compile-time validation
- generated code stays boring and explicit

## Two value forms

Attribute values use one of two forms:

- string literal: `"Hello"`
- C# expression island: `{Title}`, `{18}`, `{IsDone ? DoneBrush : PendingBrush}`

Examples:

```xml
<TextBlock Text="Hello" />
<TextBlock Text={Title} />
<TextBlock FontSize={18} />
<TextBlock Text={Description} TextWrapping={TextWrapping.Wrap} />
<Button OnClick={Save} />
```

Quoted text is always literal text. If you mean a variable, property, or method result, use `{ ... }`.

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

The property-content block is an assignment, not a native element. Single-value
properties accept at most one child, collection properties accept multiple
children when metadata describes the collection, and assigning the same property
through both an attribute and property content is a diagnostic.

## Native property validation

Native controls are validated against shared control metadata.

That means CSXAML can answer questions like:

- is `Button` a supported native control?
- does `Button` expose `Content`?
- does `TextBlock` allow children?
- does `StackPanel` expose `Spacing`?

Unknown native props and duplicate native props are invalid.

## Event posture

Native events are distinct from native properties.

Event attributes use normalized CSXAML names such as:

- `OnClick`
- `OnTextChanged`
- `OnCheckedChanged`
- `OnKeyDown`
- `OnSelectionChanged`

Event values are ordinary C# delegate-valued expressions:

```xml
<Button Content="Save" OnClick={Save} />
<Button Content="Refresh" OnClick={() => Reload()} />
<TextBox Text={Name.Value} OnTextChanged={value => Name.Value = value} />
<Slider OnValueChanged={args => Volume.Value = args.NewValue} />
```

String handler names are not valid.

## Element refs

`Ref` is a reserved native attribute for imperative interop with the projected
WinUI element:

```xml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

render <TextBox Ref={SearchBox} />;
```

The value must be an expression whose type derives from `ElementRef`. The
runtime sets `Current` after projection, keeps it across retained rerenders,
updates it on replacement, and clears it when the element is removed or the root
renderer is disposed.

`Ref` is experimental and native-only. It is not a component-ref feature and it
does not implement `x:Name` symbolic lookup.

## Controlled input

For supported interactive controls, the v1 model is controlled input:

```csharp
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={text => Name.Value = text} />;
```

The current built-in normalized input events are:

- `TextBox.Text` with `OnTextChanged={Action<string>}`
- `CheckBox.IsChecked` with `OnCheckedChanged={Action<bool>}`

The experimental typed event-args set uses senderless delegates such as:

- `OnLoaded={Action<RoutedEventArgs>}`
- `OnKeyDown={Action<KeyRoutedEventArgs>}`
- pointer events with `Action<PointerRoutedEventArgs>`
- `ListView.OnSelectionChanged={Action<SelectionChangedEventArgs>}`
- `Slider.OnValueChanged={Action<RangeBaseValueChangedEventArgs>}`
- `AutoSuggestBox` query/suggestion events with their WinUI event args
- `Frame` navigation events with their WinUI event args

## Attached properties

Attached properties use owner-qualified attribute names:

```xml
<TextBlock
    Grid.Row={0}
    Grid.Column={1}
    AutomationProperties.Name="Task Title" />
```

The owner type must be visible through the file's ordinary C# namespace context, such as:

- file namespace
- `using Namespace;`
- type alias: `using AutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;`

Namespace aliases are valid for tags, but not as attached-property owners by themselves.

The current expanded built-in owner set is intentionally explicit:

| Owner | Supported properties |
| --- | --- |
| `AutomationProperties` | `AutomationId`, `Name`, `HelpText`, `ItemStatus`, `ItemType`, `LabeledBy` |
| `Canvas` | `Left`, `Top`, `ZIndex` |
| `Grid` | `Column`, `ColumnSpan`, `Row`, `RowSpan` |
| `RelativePanel` | `AlignLeftWithPanel`, `AlignTopWithPanel`, `Below`, `RightOf` |
| `ToolTipService` | `ToolTip` |
| `VariableSizedWrapGrid` | `ColumnSpan`, `RowSpan` |

Layout attached properties that describe parent-specific placement are only
valid under their matching parent control. `ToolTipService` and automation
metadata can be applied to any supported projected element.

## Component props versus native props

The same markup surface is used for both, but the validation source differs:

- component props validate against the callee's generated prop surface
- native props validate against control metadata

Examples:

```xml
<TodoCard Title={item.Title} IsDone={item.IsDone} OnToggle={() => Toggle(item.Id)} />
<Button Content="Save" OnClick={Save} />
```

Both surfaces are strongly validated; neither falls back to an untyped bag.
