---
title: Resources and Templates
description: How generated CSXAML apps use resource dictionaries, and when to keep XAML for templates and theme resources.
---

# Resources and Templates

CSXAML does not replace WinUI's resource and template system. It gives generated
apps an explicit place to own app resources, while keeping XAML dictionaries as
the right tool for deep WinUI styling, templates, theme resources, and
`DataContext`-driven controls.

## Status

Generated app resources are experimental. In generated application mode, one
`component Application` can name one `component ResourceDictionary` with the
`resources` declaration:

```csxaml
namespace MyApp;

component Application App {
    startup MainWindow;
    resources AppResources;
}
```

The build emits a hidden intermediate `App.xaml` for generated apps so WinUI
loads its default control resources through the normal XAML compiler path. This
does not require a source `App.xaml` in the project.

## Generated Dictionaries

The current generated dictionary surface is intentionally small. A
`component ResourceDictionary` must render a `ResourceDictionary` root and can
populate `ResourceDictionary.MergedDictionaries`:

```csxaml
namespace MyApp;

component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
            <MyApp.Resources.AppTheme />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>;
}
```

`XamlControlsResources` is recognized as the WinUI controls resource
dictionary. Generated apps load that default dictionary from the hidden
intermediate `App.xaml`; when `AppResources` contains only
`XamlControlsResources`, the generated app does not instantiate the
`ResourceDictionary` subclass at runtime. Other merged entries are constructed
as CLR dictionary types, so an existing XAML dictionary should have an `x:Class`
and a public parameterless constructor.

## When To Keep XAML

Keep XAML resource dictionaries for:

- `DataTemplate`
- `ControlTemplate`
- implicit styles
- theme dictionaries and `{ThemeResource ...}` invalidation
- `{StaticResource ...}`-heavy resource graphs
- controls that rely on ambient `DataContext`
- existing WinUI pages that already have mature resource dictionaries

Those systems already have WinUI semantics, designer support, and runtime
behavior. CSXAML should not provide partial lookalikes that behave differently.

## Hybrid Apps

Hybrid apps keep the ordinary WinUI shell and host CSXAML components with
`CsxamlHost`. In that model, keep using `App.xaml`, `MainWindow.xaml`, and XAML
resource dictionaries exactly where WinUI expects them:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
            <ResourceDictionary Source="Resources/AppTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

CSXAML components can still receive values from C# expressions:

```csxaml
render <Button Style={AppStyles.PrimaryButtonStyle} Content="Save" />;
```

That expression is a normal C# value. It does not become a XAML
`StaticResource` lookup or a `ThemeResource` subscription.

## Generated Apps

Generated apps should avoid source `App.xaml`. Put generated app shell
declarations in CSXAML:

```csxaml
namespace MyApp;

component Window MainWindow {
    Title = "MyApp";
    Width = 960;
    Height = 640;

    render <HomePage />;
}
```

Use the generated resource dictionary for app-owned merged dictionaries. Keep
deep templates and theme dictionaries in XAML dictionary classes and merge those
classes from `AppResources`.

## Property Content Is Not A Template Language

Property-content syntax is for metadata-backed property assignment:

```csxaml
<Border>
    <Border.Child>
        <TextBlock Text="Title" />
    </Border.Child>
</Border>
```

Do not use property-content syntax as a substitute for `DataTemplate` or
`ControlTemplate` authoring. When a control expects a template-heavy object,
prefer one of these shapes:

- keep the template in a XAML resource dictionary
- expose the object through a typed C# helper and assign it with an expression
- wrap the behavior in a native WinUI control that owns its template story

## Not Supported

The current generated dictionary feature does not author:

- keyed CSXAML resources
- `StaticResource` or `ThemeResource` markup extensions
- `Binding`
- `DataTemplate`
- `ControlTemplate`
- implicit styles
- theme dictionaries
- URI-based `ResourceDictionary.Source` entries from CSXAML

Those are deliberate boundaries, not accidental omissions.
