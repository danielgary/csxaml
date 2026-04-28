---
title: Create a New App
description: Start from a blank WinUI app, verify it runs, then add CSXAML.
---

# Create a New App

Use this path when you do not already have a WinUI project.

CSXAML is added to a normal WinUI app. The documented public path starts with
Visual Studio's WinUI project templates, then adds the `Csxaml` package and a
first `.csxaml` component.

## 1. Create the WinUI app

In Visual Studio, create a WinUI 3 desktop app from a blank WinUI template. Use
the packaged or unpackaged project shape that matches the app you want to ship.

After the project is created, check that the app project has the normal WinUI
settings:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
  <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
  <UseWinUI>true</UseWinUI>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

Keep the Windows App SDK package reference in the project:

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
```

## 2. Prove the blank app works

Build and launch the app before adding CSXAML:

```powershell
dotnet build
```

Fix SDK, workload, Windows App SDK, or project-template problems at this point.
If the blank WinUI app does not build yet, CSXAML generator errors will make the
failure harder to read.

## 3. Add CSXAML

Add the author-facing package:

```xml
<PackageReference Include="Csxaml" Version="0.1.0-preview.1" />
```

Then continue with the [Quick Start](quick-start.md). It adds a first
`HelloCard.csxaml` component, mounts it from `MainWindow`, and shows where
generated C# appears under `obj`.

## Repository Starter Sample

If you are working inside this repository and want a small running example,
build the starter sample:

```powershell
dotnet restore .\samples\Csxaml.Starter\Csxaml.Starter.csproj
dotnet build .\samples\Csxaml.Starter\Csxaml.Starter.csproj --no-restore
```

The sample demonstrates one root component, one child component, state
invalidation, a button event, a controlled `TextBox`, and an attached property.
It is repo-local validation, not the public package install path.
