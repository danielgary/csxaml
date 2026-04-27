---
title: Supported Features
description: The current supported, preview, experimental, and deferred CSXAML feature surface.
---

# Supported Features

CSXAML uses release-posture labels to keep expectations explicit:

| Label | Meaning |
| --- | --- |
| Supported in v1 | Implemented, documented, and inside the current compatibility promise. |
| Preview shipping path | Intended to ship, but still preview or local-validation oriented. |
| Experimental | Usable for exploration, but outside the current v1 promise. |
| Not in v1 | Explicitly deferred. |

Supported areas include:

- component parameters
- `State<T>` declarations
- explicit `inject` declarations
- conditional markup
- repeated markup with keyed identity
- default slots
- current attached-property slice
- built-in control property and event binding
- external controls from referenced assemblies in the documented shape
- root host rendering
- runtime/testing DI activation
- component testing helpers
- editor diagnostics, completion, hover, quick fixes, formatting, and semantic tokens for the supported slice

Deferred areas include:

- named slots and fallback slot content
- broader external attached-property owner discovery
- richer event-argument projection
- virtualization
- `DataContext`-heavy third-party interop
- dedicated source-level lifecycle syntax

The source feature matrix remains in `docs/supported-feature-matrix.md` while the DocFX site is being established.
