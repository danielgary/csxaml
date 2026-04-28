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

Use the generated API reference for exact type and member documentation.
