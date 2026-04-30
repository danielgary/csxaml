---
title: Syntax
description: Practical syntax guide for CSXAML component files.
---

# Syntax

## File shape

```csxaml
using Microsoft.UI.Xaml.Controls;

namespace MyApp.Components;

component Element HelloCard(string Title) {
    render <TextBlock Text={Title} />;
}
```

## Component declaration

```csxaml
component Element Name(Type Prop, Type OtherProp) {
    render <Root />;
}
```

The root render statement must use `render <Root />;`. Returning markup with `return` is not valid CSXAML syntax.

Experimental generated root declarations use the same family:

```csxaml
component Page HomePage {
    render <Grid />;
}

component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;
    Backdrop = "Mica";

    render <HomePage />;
}
```

`component Page` and `component Window` generate WinUI shell types backed by a
retained CSXAML body. Page roots also emit hidden generated XAML companions so
native `Frame.Navigate(typeof(PageType))` can activate them.

Generated application mode adds:

```csxaml
component Application App {
    startup MainWindow;
    resources AppResources;
}

component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>;
}
```

Generated mode is enabled with `CsxamlApplicationMode=Generated`. It is
experimental, emits a hidden intermediate `App.xaml` for WinUI default control
resources, and rejects source `App.xaml` files in generated apps.
`ResourceDictionary` roots currently support merged dictionaries only.
Default-only `XamlControlsResources` dictionaries are recognized without
runtime instantiation. Resource lookup, templates, and theme dictionaries remain
covered by [Resources and Templates](../guides/resources-and-templates.md).

## Common invalid syntax

Use `render`, not `return`, for the final markup statement:

```csxaml
// Invalid
return <StackPanel />;

// Valid
render <StackPanel />;
```

Read a state value through `.Value`:

```csxaml
State<int> Count = new State<int>(0);

// Invalid or unintended
<TextBlock Text={Count} />

// Usually intended
<TextBlock Text={Count.Value} />
```

Attached-property values that are not strings use C# expressions:

```csxaml
// Invalid
<TextBlock Grid.Row="1" Text="Title" />

// Valid
<TextBlock Grid.Row={1} Text="Title" />
```

## Attributes

String values can be written as literals:

```csxaml
<TextBlock Text="Hello" />
```

C# expressions are wrapped in braces:

```csxaml
<TextBlock Text={Title} />
```

Reserved framework attributes also use normal expression rules. `Ref` must be an
expression, not a string name:

```csxaml
ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

render <TextBox Ref={SearchBox} />;
```

## Children

Native and component tags can contain children:

```csxaml
<StackPanel>
    <TextBlock Text="Title" />
    <Button Content="Save" />
</StackPanel>
```

## Property Content

Experimental property-content syntax assigns children to a named native
property or component slot without treating the property element as a real
control:

```csxaml
<Border>
    <Border.Child>
        <TextBlock Text="Title" />
    </Border.Child>
</Border>
```

The owner before the dot must match the containing tag. Property-content
elements cannot be render roots, cannot have attributes such as `Key`, events,
attached properties, or `Ref`, and cannot be assigned alongside an attribute of
the same name.

Component named slots use the same source shape:

```csxaml
<TodoCard>
    <TodoCard.Header>
        <TextBlock Text="Today" />
    </TodoCard.Header>
</TodoCard>
```

## Control flow

`if` and `foreach` are supported inside markup children:

```csxaml
if (Items.Value.Count == 0) {
    <TextBlock Text="Nothing yet" />
}

foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

`foreach` creates repeated retained child nodes. It is useful for small and
moderate visible lists, but it is not virtualization; use the
[Performance and Scale](../guides/performance-and-scale.md) guide when choosing
between markup loops and native virtualized controls.

## Aliased tags

File-level using aliases can qualify external controls:

```csxaml
using WinUi = Microsoft.UI.Xaml.Controls;

render <WinUi:InfoBar IsOpen={true} Title="Ready" />;
```
