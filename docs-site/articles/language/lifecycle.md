---
title: Lifecycle
description: Runtime lifecycle behavior for mounting, disposal, async work, and post-unmount state updates.
---

# Lifecycle

CSXAML keeps lifecycle behavior intentionally small.

Supported runtime hooks for handwritten component types:

- `OnMounted()`
- `IDisposable`
- `IAsyncDisposable`

Generated `.csxaml` source does not currently introduce dedicated lifecycle syntax. Use ordinary C# services and explicit state updates for async work.

Important rules:

- mounted components should mount once while retained
- removed components are disposed
- async disposal is honored by root disposal
- state writes after unmount do not resurrect a component
- render-phase state writes are invalid
- native `ElementRef<T>` handles are cleared when their element leaves the tree
  or the root renderer is disposed

See [Lifecycle and Async](../guides/lifecycle-and-async.md) for examples and test expectations.
