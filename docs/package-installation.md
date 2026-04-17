# Package Installation

CSXAML currently exposes two public package identities:

- `Csxaml`
- `Csxaml.Runtime`

For normal app authors, install only `Csxaml`.

`Csxaml` is the author-facing package. It carries:

- the `buildTransitive` MSBuild assets that wire `.csxaml` generation into your project
- the packaged generator payload used at build time
- a dependency on `Csxaml.Runtime`

You do not need to reference `Csxaml.Generator`, `Csxaml.ControlMetadata`, or repo-local build targets directly.

## Minimum project shape

Typical WinUI app/library setup:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
  </ItemGroup>
</Project>
```

## First component

Add a `.csxaml` file:

```csharp
component Element HelloCard(string Title) {
    render <StackPanel Spacing={8}>
        <TextBlock Text={Title} />
        <Button Content="Tap" />
    </StackPanel>;
}
```

Build the project. The package-provided targets should:

- discover `.csxaml` files automatically
- run the packaged generator
- emit generated C# under `obj\<configuration>\<tfm>\Csxaml\Generated\`

## Repo-local versus packaged use

Inside this repo, projects also import `build/Csxaml.props` through the root `Directory.Build.props`.

Outside this repo, the `Csxaml` package is the supported install path. You should not need the repo folder layout or repo-local generator project paths.

## Current package posture

- `Csxaml` and `Csxaml.Runtime` are the public package surface
- `Csxaml.Testing` remains repo-internal for now while its public API is still being reviewed
- `Csxaml.ControlMetadata` and `Csxaml.Generator` are implementation details, not public install targets
