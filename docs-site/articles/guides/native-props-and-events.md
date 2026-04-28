---
title: Native Props and Events
description: How CSXAML binds native WinUI properties, events, attached properties, and controlled inputs.
---

# Native Props and Events

Native property values can be literals or C# expressions:

```csharp
<TextBlock Text="Static" />
<TextBlock Text={Title} />
```

Events use normalized CSXAML names:

```csharp
<Button Content="Save" OnClick={Save} />
<Button Content="Refresh" OnClick={() => Reload()} />
```

Controlled input is explicit:

```csharp
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={text => Name.Value = text} />;
```

Supported projected events include:

- `Button.OnClick`
- `TextBox.OnTextChanged`
- `CheckBox.OnCheckedChanged`

Attached properties use dotted syntax:

```csharp
<TextBlock Grid.Row={0} AutomationProperties.Name="Title" Text="Todo Board" />
```

If a property or event fails validation, check the control metadata and the [supported features](../language/supported-features.md) page before assuming the WinUI API shape is available through CSXAML.
