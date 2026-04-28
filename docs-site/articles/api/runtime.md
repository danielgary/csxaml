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

## Most useful types

| Type | Use it for |
| --- | --- |
| [CsxamlHost](xref:Csxaml.Runtime.CsxamlHost) | Rendering a root component into a WinUI `Panel`. |
| [State&lt;T&gt;](xref:Csxaml.Runtime.State`1) | Component-local mutable state that can invalidate rendering. |
| [ComponentInstance](xref:Csxaml.Runtime.ComponentInstance) | Base type for generated and handwritten components. |
| [ComponentInstance&lt;TProps&gt;](xref:Csxaml.Runtime.ComponentInstance`1) | Base type for generated components with typed props. |
| [CsxamlRuntimeException](xref:Csxaml.Runtime.CsxamlRuntimeException) | Runtime failures with component/source context. |

Use <xref:Csxaml.Runtime> for exact type and member documentation.
