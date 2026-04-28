---
title: Build and Generation Troubleshooting
description: How to diagnose CSXAML package restore, generator, validation, and generated C# build failures.
---

# Build and Generation Troubleshooting

## Package restore fails

Confirm the app references `Csxaml` and `Microsoft.WindowsAppSDK`, and that NuGet.org or the intended local feed is available.

## Generator does not run

Check that the project imports the `Csxaml` package. Outside this repo, the package is responsible for the `buildTransitive` targets.

Generated output should appear under:

```text
obj\<configuration>\<tfm>\Csxaml\
```

## Unknown tag or attribute

Check:

- namespace imports in the `.csxaml` file
- whether the native or external control is in the supported metadata slice
- whether the attribute is a property, event, attached property, or component prop
- whether an aliased tag should use `Alias:TypeName`

## C# compiler error points into generated code

Use the `.csxaml` diagnostic first. If the compiler error comes from helper code or an expression island, inspect the generated file and source-map sidecar under `obj\<configuration>\<tfm>\Csxaml\`.
