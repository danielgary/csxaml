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

## Stable source example

This source shape is inside the documented compatibility promise:

```csharp
component Element CounterButton(string Label) {
    State<int> Count = new State<int>(0);

    render <Button
        Content={$"{Label}: {Count.Value}"}
        OnClick={() => Count.Value++} />;
}
```

The component declaration, typed prop, `State<T>` declaration, native `Button`
tag, `Content` property, and `OnClick` event are the supported surface.

## Unstable generated-detail example

Generated files under `obj` are useful for debugging, but normal app code
should not depend on their private layout:

```text
obj\Debug\net10.0-windows10.0.19041.0\Csxaml\Generated\CounterButton.g.cs
```

Do not build app behavior around generated private method names, temporary
locals, or node-construction ordering unless that behavior is documented as a
public contract.

## Package alignment example

Keep author-facing and test-facing packages on the same release line:

```xml
<PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
<PackageReference Include="Csxaml.Testing" Version="0.1.0-preview.1" />
```

Mixing a newer generator package with an older runtime or testing package is not
part of the compatibility promise.
