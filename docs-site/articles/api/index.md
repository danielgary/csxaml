---
title: API Overview
description: How CSXAML packages and namespaces are organized for app authors, tests, tooling, and metadata integrations.
---

# API Overview

The API reference is generated from XML documentation comments during the docs build. Do not edit generated API pages by hand.

Start here to choose the right surface:

- [Packages and Namespaces](packages-and-namespaces.md)
- [Runtime API](runtime.md)
- [Testing API](testing.md)
- [Tooling API](tooling.md)
- [Metadata API](metadata.md)

Generated reference pages are produced from these projects:

- `Csxaml.ControlMetadata`
- `Csxaml.Runtime`
- `Csxaml.Testing`
- `Csxaml.Tooling.Core`
- `Csxaml.VisualStudio`

The generated output is written to `obj/docfx/api` at build time and published under `api/` in the static site.
