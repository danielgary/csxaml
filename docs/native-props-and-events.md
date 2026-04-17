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
<Button OnClick={Save} />
```

Quoted text is always literal text. If you mean a variable, property, or method result, use `{ ... }`.

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

Event values are ordinary C# delegate-valued expressions:

```xml
<Button Content="Save" OnClick={Save} />
<Button Content="Refresh" OnClick={() => Reload()} />
<TextBox Text={Name.Value} OnTextChanged={value => Name.Value = value} />
```

String handler names are not valid.

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
