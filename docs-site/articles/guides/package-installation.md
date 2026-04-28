---
title: Package Installation
description: How app authors install CSXAML packages and what each public package is for.
---

# Package Installation

Normal app authors install `Csxaml`.

`Csxaml` is the author-facing package. It carries:

- `buildTransitive` MSBuild assets
- the packaged generator payload
- a dependency on `Csxaml.Runtime`

Do not reference `Csxaml.Generator`, `Csxaml.ControlMetadata`, or repo-local build targets directly from an app.

```xml
<ItemGroup>
  <PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
  <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
</ItemGroup>
```

The generated C# lands under:

```text
obj\<configuration>\<tfm>\Csxaml\Generated\
```

For package-only validation, see the sample under `samples/PackageHello`.

## Package roles

| Package | Audience | Purpose |
| --- | --- | --- |
| `Csxaml` | App authors | Build integration and generator payload. |
| `Csxaml.Runtime` | Generated components | Retained component runtime and WinUI projection support. |
| `Csxaml.Testing` | Test authors | Hostless component test helpers, once public API review is complete. |
| `Csxaml.ControlMetadata` | Tooling and advanced integration | Shared metadata model for native and external controls. |
