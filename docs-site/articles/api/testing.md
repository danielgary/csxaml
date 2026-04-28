---
title: Testing API
description: Overview of CSXAML hostless component test APIs.
---

# Testing API

`Csxaml.Testing` provides hostless component rendering for unit and integration tests.

Primary entry point:

```csharp
using Csxaml.Testing;
using MyApp.Components;

using var render = CsxamlTestHost.Render<TodoBoardComponent>();
```

Primary capabilities:

- render a root component instance
- render a root component type with services
- query by automation id, automation name, text, or content
- invoke click, text, and checked-state interactions
- rerender explicitly when external state changes outside an interaction callback

Prefer semantic queries over structural child indexes.

Use this package from test projects. App runtime code should not depend on
`Csxaml.Testing`; it renders logical trees for verification rather than mounting
WinUI UI.

## Most useful types

| Type | Use it for |
| --- | --- |
| [CsxamlTestHost](xref:Csxaml.Testing.CsxamlTestHost) | Rendering a component without a live WinUI window. |
| [CsxamlRenderResult](xref:Csxaml.Testing.CsxamlRenderResult) | Querying the rendered logical tree and invoking interactions. |

Use <xref:Csxaml.Testing> for exact type and member documentation.
