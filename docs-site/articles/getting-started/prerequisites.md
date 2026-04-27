---
title: Prerequisites
description: Required SDKs, platform assumptions, and project settings for CSXAML apps.
---

# Prerequisites

CSXAML targets WinUI app and library projects.

Minimum project expectations:

- Windows development environment.
- .NET SDK 10 for CSXAML runtime and generator projects.
- .NET SDK 8 when working on the Visual Studio extension.
- Windows App SDK referenced by the app project.
- `UseWinUI` enabled in the app or library project.
- Nullable reference types recommended.

Typical project shape:

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

Inside this repository, shared build targets are also imported for local development. Outside this repository, the supported authoring path is the `Csxaml` package.
