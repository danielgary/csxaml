---
title: Packages and Namespaces
description: Which CSXAML package and namespace to use for each developer role.
---

# Packages and Namespaces

| Package or project | Primary audience | Notes |
| --- | --- | --- |
| `Csxaml` | App authors | Author-facing package with build targets and generator payload. |
| `Csxaml.Runtime` | Generated components and advanced runtime hosts | Runtime nodes, components, state, reconciliation, hosting, and rendering APIs. |
| `Csxaml.Testing` | Test authors | Hostless component rendering, queries, and interactions. |
| `Csxaml.Tooling.Core` | Editor/tooling authors | Completion, diagnostics, definitions, formatting, hover, semantic tokens, and workspace loading. |
| `Csxaml.ControlMetadata` | Metadata/tooling authors | Shared control metadata model. |
| `Csxaml.VisualStudio` | Visual Studio extension contributors | VS-specific host and contribution types. |

Normal app authors should start with the `Csxaml` package and generally should not depend directly on generator, language-server, or control-metadata internals.
