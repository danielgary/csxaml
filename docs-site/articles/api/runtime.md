---
title: Runtime API
description: Overview of the CSXAML runtime APIs used by generated components and advanced hosts.
---

# Runtime API

`Csxaml.Runtime` contains the retained component runtime used by generated CSXAML components.

Important areas:

- component instances
- `State<T>`
- native and component node models
- reconciliation
- host integration
- WinUI projection
- runtime diagnostics

Generated components depend on this package. App authors usually interact with it through the host surface and generated components rather than directly constructing node trees.

## App-author boundary

Use these directly in normal apps:

- `CsxamlHost` to mount a root component
- `CsxamlRootHost` when working with experimental generated `Window` or `Page`
  roots
- generated component classes and generated props records
- `State<T>` inside `.csxaml` or handwritten component code
- `CsxamlRuntimeException` when reporting runtime failures

Do not build normal app behavior around these unless you are writing an
advanced host or runtime extension:

- `NativeElementNode` and `ComponentNode`
- reconciliation classes
- WinUI adapter classes
- generated node-construction details

## Most useful types

| Type | Use it for |
| --- | --- |
| [CsxamlHost](xref:Csxaml.Runtime.CsxamlHost) | Rendering a root component into a WinUI `Panel`. |
| [CsxamlRootHost](xref:Csxaml.Runtime.CsxamlRootHost) | Mounting the retained body behind an experimental generated `Window` or `Page` root. |
| [State&lt;T&gt;](xref:Csxaml.Runtime.State`1) | Component-local mutable state that can invalidate rendering. |
| [ComponentInstance](xref:Csxaml.Runtime.ComponentInstance) | Base type for generated and handwritten components. |
| [ComponentInstance&lt;TProps&gt;](xref:Csxaml.Runtime.ComponentInstance`1) | Base type for generated components with typed props. |
| [CsxamlRuntimeException](xref:Csxaml.Runtime.CsxamlRuntimeException) | Runtime failures with component/source context. |

Use <xref:Csxaml.Runtime> for exact type and member documentation.
