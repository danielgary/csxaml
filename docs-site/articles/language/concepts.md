---
title: Concepts
description: "Core CSXAML concepts: components, props, state, native controls, events, and retained identity."
---

# Concepts

## Components

A CSXAML component is a generated C# component type with a declarative render body. Component parameters become typed props.

## Props

Props are public inputs. They are declared in the component parameter list:

```csharp
component Element TodoCard(string Title, bool IsDone) {
    render <TextBlock Text={Title} />;
}
```

## State

`State<T>` represents component-owned mutable UI state. Assigning `Value` invalidates the component when the value changes.

```csharp
State<int> Count = new State<int>(0);
```

## Native controls

Native tags such as `StackPanel`, `TextBlock`, and `Button` project to WinUI controls through runtime adapters.

## Events

Events are assigned with C# delegates:

```csharp
<Button Content="Save" OnClick={Save} />
```

## Retained identity

The runtime reconciles component and native trees between renders. Stable `Key` values preserve identity for repeated children.

## Tooling

The language service understands tags, component props, native properties and events, attached properties, formatting, semantic tokens, hover, and diagnostics for the supported language slice.
