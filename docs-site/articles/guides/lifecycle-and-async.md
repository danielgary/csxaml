---
title: Lifecycle and Async
description: Lifecycle and async behavior for CSXAML runtime components.
---

# Lifecycle and Async

Lifecycle support is intentionally small and runtime-centered.

Supported behavior:

- retained children mount once while their identity is preserved
- removed children are disposed
- async root disposal is supported
- stale state invalidation after unmount does not rerender the tree
- render-phase state writes fail rather than recursively rerendering

Handwritten component types can use:

- `OnMounted()`
- `IDisposable`
- `IAsyncDisposable`

Generated `.csxaml` files do not currently have dedicated lifecycle syntax. Put async work behind explicit services, update state deliberately, and keep cancellation or disposal ownership visible in normal C# code.

If an async continuation runs after a component has unmounted, it must not resurrect the disposed component instance.
