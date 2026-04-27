---
title: Compatibility Policy
description: What parts of CSXAML are stable enough to rely on and what remains subject to change.
---

# Compatibility Policy

CSXAML compatibility is scoped to the documented v1 surface.

Inside the promise:

- component declarations and typed props
- `State<T>` declarations and assignment-driven invalidation
- explicit `inject` declarations
- supported native controls, props, events, and attached properties
- keyed repeated child identity
- default slot behavior
- documented testing helpers
- documented package boundaries

Outside the promise:

- internal generator implementation details
- generated C# shape beyond documented source-map and diagnostics behavior
- unsupported WinUI control metadata
- broad `DataContext` interop
- named slots and fallback content
- unreleased editor experiments

If an implementation detail is useful enough for external developers, document it and add tests before treating it as supported behavior.
