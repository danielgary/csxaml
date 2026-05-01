---
title: Create a New App
description: Start from a blank WinUI app, verify it runs, then add CSXAML.
---

# Create a New App

Use this path when you do not already have a WinUI project.

CSXAML is added to a normal WinUI app. The documented public path starts with
Visual Studio's WinUI project templates, then adds the `Csxaml` package and a
first `.csxaml` component.

Experimental generated application mode now exists. It can build an app from
`component Application`, `component Window`, `component Page`, and limited
`component ResourceDictionary` files with no `App.xaml`, no `App.xaml.cs`, no
`MainWindow.xaml`, and no `MainWindow.xaml.cs`.

The v1-safe public path still starts with the normal WinUI shell. Use generated
mode when you are deliberately exploring post-v1 app-shell behavior.

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

## Experimental generated mode

Generated mode is selected in the project file:

```xml
<CsxamlApplicationMode>Generated</CsxamlApplicationMode>
```

In that mode, remove the template `App.xaml` and `MainWindow.xaml` files and
author the app shell in CSXAML:

```csxaml
component Application App {
    startup MainWindow;
    resources AppResources;
}

component ResourceDictionary AppResources {
    render <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>;
}

component Window MainWindow {
    Title = "CSXAML Starter";
    Width = 960;
    Height = 640;

    render <HomePage />;
}
```

The repository samples `samples/Csxaml.HelloWorld`, `samples/Csxaml.TodoApp`,
and `samples/Csxaml.FeatureGallery` are the current proof for this shape.
Generated mode is experimental and intentionally smaller than the normal WinUI
app lifecycle surface. Generated mode should use `component ResourceDictionary`
for its experimental app resource path rather than reintroducing source
`App.xaml`. The build still emits a hidden intermediate `App.xaml` so WinUI can
load default control resources normally. Keep deep templates, theme
dictionaries, and
`StaticResource`/`ThemeResource` graphs in XAML dictionaries; see
[Resources and Templates](../guides/resources-and-templates.md).

## Repository Samples

If you are working inside this repository and want running examples, use the
consolidated samples:

```powershell
dotnet build .\samples\Csxaml.ExistingWinUI\Csxaml.ExistingWinUI.csproj
dotnet build .\samples\Csxaml.HelloWorld\Csxaml.HelloWorld.csproj
dotnet build .\samples\Csxaml.TodoApp\Csxaml.TodoApp.csproj
dotnet build .\samples\Csxaml.FeatureGallery\Csxaml.FeatureGallery.csproj
```

`Csxaml.ExistingWinUI` demonstrates the supported existing-shell host path.
`Csxaml.HelloWorld` is the smallest generated app. `Csxaml.TodoApp` is the
advanced generated-app sample. `Csxaml.FeatureGallery` demonstrates the
experimental post-v1 feature surface in one app. These are repo-local
validation samples, not the public package install path.
