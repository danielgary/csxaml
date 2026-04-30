---
title: Why CSXAML?
description: When CSXAML is useful compared with XAML/code-behind and handwritten C# UI trees.
---

# Why CSXAML?

CSXAML is for WinUI codebases that want UI structure, state, events, and helper
logic in one typed component file without giving up generated C# output.

## Compared with XAML and code-behind

XAML is excellent for declarative WinUI structure, but behavior often moves into
code-behind, converters, view models, or binding indirection. CSXAML keeps the
tree shape close to XAML while letting values and events be ordinary C#
expressions.

Use CSXAML when:

1. The UI is component-shaped.
2. Props and state should be easy to trace in code.
3. Event handlers should be explicit C# delegates.
4. Generated output should stay inspectable for debugging.

Stay with XAML when:

1. The view depends heavily on established XAML resource, template, or
   `DataContext` patterns.
2. Designers and existing tooling are centered on XAML files.
3. The page is already stable and the binding model is working clearly.

## Compared with handwritten C# UI trees

Handwritten C# UI trees keep everything in one language, but deeply nested UI
construction can become hard to scan. CSXAML keeps the structure readable while
still lowering to ordinary generated C#.

Use CSXAML when a component should read like markup but behave like typed C#.
Use handwritten C# when the UI is highly dynamic, generated from data in ways
that would make markup harder to understand, or when you need APIs outside the
current supported CSXAML surface.

## Concrete fit examples

Good fit: a task-board card editor with typed props, a few local state values,
explicit button/text events, and generated output you want to debug when a build
or runtime diagnostic points back to source.

```csxaml
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    render <StackPanel Spacing={8}>
        <TextBlock Text={Title} />
        <CheckBox IsChecked={IsDone} OnCheckedChanged={_ => OnToggle()} />
    </StackPanel>;
}
```

Bad fit: an existing page built around XAML `DataTemplate` resources,
`DataContext` inheritance, designer-owned layout, and mature binding patterns.
Keep that page in XAML unless there is a specific component slice that benefits
from typed CSXAML props and explicit event flow.
The [Resources and Templates](../guides/resources-and-templates.md) guide
spells out that boundary for generated app resources and XAML dictionaries.

## Current boundary

CSXAML is still preview technology. Before depending on a feature, check the
[supported feature matrix](../language/supported-features.md). The supported
path is intentionally narrow so behavior remains readable, testable, and low
surprise.
