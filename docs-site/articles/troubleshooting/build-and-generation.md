---
title: Build and Generation Troubleshooting
description: How to diagnose CSXAML package restore, generator, validation, and generated C# build failures.
---

# Build and Generation Troubleshooting

## Symptom: package restore fails

Likely causes:

1. The project does not reference `Csxaml`.
2. The project does not reference `Microsoft.WindowsAppSDK`.
3. NuGet.org or the intended local feed is unavailable.

How to verify:

```powershell
dotnet restore
dotnet list package
```

## Symptom: `dotnet build` succeeds but no generated file appears

Likely causes:

1. The file extension is not exactly `.csxaml`.
2. The `Csxaml` package is not referenced by this project.
3. The file is excluded by MSBuild item rules.

How to verify:

```powershell
dotnet build /bl
Get-ChildItem -Recurse obj -Filter *.g.cs | Where-Object FullName -like '*Csxaml*'
```

Generated output should appear under:

```text
obj\<configuration>\<tfm>\Csxaml\
```

Outside this repo, the package is responsible for importing the `buildTransitive`
targets that run the generator.

## Symptom: unknown tag or attribute diagnostic

Likely causes:

1. The `.csxaml` file is missing a namespace import.
2. The native or external control is outside the supported metadata slice.
3. The attribute name is not a supported property, event, attached property, or component prop.
4. An aliased external control tag should use `Alias:TypeName`.

How to verify:

```csharp
using WinUi = Microsoft.UI.Xaml.Controls;

render <WinUi:InfoBar IsOpen={true} Title="Ready" />;
```

Check the [native props and events guide](../guides/native-props-and-events.md)
and [supported feature matrix](../language/supported-features.md) before
assuming every WinUI property is exposed.

## Symptom: C# compiler error points into generated code

Use the `.csxaml` diagnostic first. If the compiler error comes from helper
code or an expression island, inspect the generated file and source-map sidecar:

```powershell
Get-ChildItem -Recurse obj -Include *.g.cs,*.map | Where-Object FullName -like '*Csxaml*'
```

Common causes include missing `using` directives, invalid C# in an expression
island, or passing a `State<T>` object where the control property expects
`State<T>.Value`.
