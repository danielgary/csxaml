---
title: Prerequisites
description: Required SDKs, platform assumptions, and project settings for CSXAML apps.
---

# Prerequisites

CSXAML targets WinUI app and library projects on Windows.

## Required tools

- Windows development environment.
- .NET SDK 10 for CSXAML app, runtime, generator, and language-server builds.
- Windows App SDK referenced by the app project.
- Visual Studio or command-line build tools that can build WinUI projects.
- `UseWinUI` enabled in the app or library project.
- Nullable reference types recommended.

For Visual Studio app authoring, install the WinUI/Windows App SDK tooling and
the .NET desktop workload. For Visual Studio extension development, also install
the **Visual Studio extension development** workload and keep .NET SDK 8
available for the current VSIX host project.

Command-line only usage is supported after you have a WinUI project file. You
can restore, build, and test with `dotnet`; Visual Studio is not required just
to run the CSXAML generator.

## Project shape

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

`net10.0-windows10.0.19041.0` is the currently tested target framework for the
public package path. Treat other Windows TFMs as deliberate validation work
rather than the documented baseline.

You can create the WinUI project from Visual Studio templates, an existing app,
or a starter project. CSXAML does not require a special WinUI template; it only
requires the package reference and `.csxaml` files included in the project.

## Verify the environment

Check the installed SDKs:

```powershell
dotnet --list-sdks
```

If .NET 10 is missing, `dotnet build` commonly reports:

```text
NETSDK1045: The current .NET SDK does not support targeting .NET 10.0.
```

Check package versions in the app project:

```powershell
dotnet list package
```

Expected package lines include `Csxaml` at the current preview version and
`Microsoft.WindowsAppSDK` at the version used by the project.

Inside this repository, shared build targets are also imported for local development. Outside this repository, the supported authoring path is the `Csxaml` package.
