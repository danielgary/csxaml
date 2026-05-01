---
title: Concepts
description: "Core CSXAML concepts: components, props, state, native controls, events, and retained identity."
---

# Concepts

Use this page as a glossary while reading examples. For exact grammar and edge
cases, use the [syntax cheat sheet](syntax.md) and the
[language specification](specification.md).

## Component file

A `.csxaml` file contains ordinary C# imports, one namespace, one CSXAML
component declaration, optional component-local state or services, helper code,
and one final `render <Root />;` statement.

## Components

A CSXAML component is a generated C# component type with a declarative render
body. Component parameters become typed props.

## Component instance

A component instance is the retained runtime object for one mounted component in
the rendered tree. State and injected services belong to the instance and are
preserved while reconciliation keeps that instance alive.

## Generated component type

Each `.csxaml` component generates a component class named after the source
component plus `Component`, such as `TodoCardComponent`.

## Props

Props are public inputs. They are declared in the component parameter list:

```csxaml
component Element TodoCard(string Title, bool IsDone) {
    render <TextBlock Text={Title} />;
}
```

Generated props are strongly typed. For a component such as `TodoCard`, the
generated props record is named `TodoCardProps`.

## Render statement

`render <Root />;` is the required final markup statement in a component body.
It is not an ordinary C# `return` statement.

## Expression island

An expression island is C# code inside markup braces:

```csxaml
<TextBlock Text={$"Hello, {Name}"} />
```

The expression runs against the current component props, state, injected
services, and helper code.

## State

`State<T>` represents component-owned mutable UI state. Assigning `Value` invalidates the component when the value changes.

```csxaml
State<int> Count = new State<int>(0);
```

## State invalidation

Changing `State<T>.Value` marks the owning component dirty when the new value is
different under the current equality rule. The host may batch dirty components
into a later render pass.

## `Touch()`

`Touch()` marks a state value dirty without replacing the value. Use it after
intentional in-place mutation of a list or object stored inside `State<T>`.

## Native controls

Native tags such as `StackPanel`, `TextBlock`, and `Button` project to WinUI controls through runtime adapters.

## Native element

A native element is a markup tag that resolves to a WinUI control or framework
type instead of a CSXAML component.

## Component element

A component element is a markup tag that resolves to another CSXAML component:

```csxaml
<TodoCard Title={item.Title} IsDone={item.IsDone} />
```

## Attached property

An attached property is assigned with its owner-qualified name, such as
`AutomationProperties.Name` or `Grid.Row`.

## Events

Events are assigned with C# delegates:

```csxaml
<Button Content="Save" OnClick={Save} />
```

Projected event handlers should update state explicitly when they need the UI to
change.

## Child content

Child content is the markup nested inside a native or component tag. Native
controls use their supported child-content rule. Components can accept child
content only when they declare the supported slot shape.

## Property content

Property content is an experimental `<Owner.Property>` child block that assigns
markup to a metadata-backed native property or component named slot. The block
is an assignment, not a real native element.

## Key

`Key` is an identity hint for repeated children. Stable keys let the runtime
match the same logical child across rerenders.

## Element ref

`ElementRef<T>` is an experimental imperative handle for projected native
controls. Assign it with `Ref={...}` on a native tag when component code needs
to call focus, scrolling, animation, or other WinUI APIs directly.

## Retained identity

The runtime reconciles component and native trees between renders. Stable `Key` values preserve identity for repeated children.

## Reconciliation

Reconciliation compares the new rendered tree with the previous retained tree.
It preserves matching component/native instances and removes or creates the
instances that no longer match.

## Host

`CsxamlHost` mounts a root component into a WinUI panel and coordinates render
passes for that subtree.

## Runtime adapter

A runtime adapter projects a logical CSXAML native node to a real WinUI object,
sets supported properties, attaches event handlers, and manages children.

## External control

An external control is a WinUI control from a referenced assembly that is made
visible to CSXAML through generated control metadata.

## Automation metadata

Automation properties such as `AutomationProperties.Name` give the UI a semantic
name. They are useful for accessibility and for `Csxaml.Testing` queries.

## Source map

The generator writes source-map sidecars under `obj` so diagnostics and debugger
workflows can connect generated C# back to authored `.csxaml`.

## Diagnostic

A diagnostic is a parser, validation, tooling, build, or runtime message that
should identify the relevant `.csxaml` source whenever possible.

## Tooling

The language service understands tags, component props, native properties and events, attached properties, formatting, semantic tokens, hover, and diagnostics for the supported language slice.
