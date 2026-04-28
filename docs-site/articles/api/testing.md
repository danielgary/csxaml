---
title: Testing API
description: Overview of CSXAML hostless component test APIs.
---

# Testing API

`Csxaml.Testing` provides hostless component rendering for unit and integration tests.

Primary entry point:

```csharp
using Csxaml.Testing;

using var render = CsxamlTestHost.Render(new TodoBoard());
```

Primary capabilities:

- render a root component instance
- render a root component type with services
- query by automation id, automation name, text, or content
- invoke click, text, and checked-state interactions
- rerender explicitly when external state changes outside an interaction callback

Prefer semantic queries over structural child indexes.
