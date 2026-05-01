---
title: Component Model
description: How CSXAML components expose props, render native nodes, and compose child components.
---

# Component Model

CSXAML components are generated C# component classes.

The component parameter list defines the public prop surface:

```csxaml
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    render <Button Content={Title} OnClick={OnToggle} />;
}
```

Components can render:

- native WinUI controls
- other CSXAML components
- conditional children
- repeated children
- slotted child content in the supported default-slot shape
- experimental named slot content through property-content syntax

Component helper code runs as ordinary generated C#. Keep helper methods small and explicit so the rendered UI remains easy to trace.

## Generated Root Components

Experimental root components generate real WinUI shell types and retained
CSXAML bodies:

```csxaml
component Page HomePage {
    render <Grid>
        <TextBlock Text="Home" />
    </Grid>;
}

component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;

    render <HomePage />;
}
```

`Window` supports only `Title`, `Width`, `Height`, and experimental `Backdrop`
root declarations in this slice. `Backdrop = "Mica";` maps to WinUI's Mica
system backdrop. `Page` roots emit hidden XAML companions so native
`Frame.Navigate(typeof(PageType))` can activate generated pages through the
normal WinUI XAML metadata path. `Page` and `Window` roots do not accept
component parameters or slots.

Generated app mode adds one application root and optional app resources:

```csxaml
component Application App {
    startup MainWindow;
    resources AppResources;
}
```

`CsxamlApplicationMode=Generated` emits the WinUI entry point, creates the
startup window, and rejects projects that still include a source `App.xaml` as
an `ApplicationDefinition`. The build also emits a hidden intermediate
`App.xaml` so WinUI default control resources load through the normal XAML
compiler path. `component ResourceDictionary` is limited to merged dictionaries
for now. See
[Resources and Templates](../guides/resources-and-templates.md) for the
generated-app resource path and the XAML template boundary.

## Child components

Use component tags by importing the namespace or placing the components in the same namespace:

```csxaml
<TodoCard
    Title={item.Title}
    IsDone={item.IsDone}
    OnToggle={() => ToggleItem(item.Id)} />
```

## Keys

Use `Key` for repeated component children when identity matters:

```csxaml
foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} />
}
```

Keys should be stable, unique among siblings, and derived from model identity
rather than display text. Keys preserve identity for rendered children; they do
not virtualize large lists. Use native virtualization-aware controls for large
scrolling item surfaces.

## Slots

A bare `<Slot />` outlet receives default child content:

```csxaml
component Element PanelShell {
    render <Border>
        <Slot />
    </Border>;
}
```

Experimental named slots use `Name` on the outlet and `<Component.SlotName>` at
the call site:

```csxaml
component Element PanelShell {
    render <StackPanel>
        <Slot Name="Header" />
        <Slot />
    </StackPanel>;
}

component Element Dashboard {
    render <PanelShell>
        <PanelShell.Header>
            <TextBlock Text="Overview" />
        </PanelShell.Header>
        <TextBlock Text="Body" />
    </PanelShell>;
}
```

Named slot blocks preserve the caller's child order within that slot. Duplicate
named slot declarations, named slot outlets inside `foreach`, unknown named
slot content, and duplicate named slot content blocks are diagnostics. Fallback
slot content is not supported.
