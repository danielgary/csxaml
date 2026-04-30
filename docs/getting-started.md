# Getting Started

This guide shows the shortest path to authoring one CSXAML component in the current prototype.

Status: preview NuGet packaging works for local validation. The repo-local path remains useful for development, and the package path is the intended v1 authoring shape. A `dotnet new` template package is not implemented yet.

## Prerequisites

- Windows
- .NET 10 SDK
- a WinUI-capable build environment
- this repo checked out for repo-local development, or locally packed CSXAML packages for package validation

The demo currently targets `net10.0-windows10.0.19041.0` and uses `Microsoft.WindowsAppSDK`.

## Preview Package Setup

Pack the current preview packages from this repo:

```powershell
New-Item -ItemType Directory -Force .\artifacts\packages
dotnet pack .\Csxaml.ControlMetadata\Csxaml.ControlMetadata.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Runtime\Csxaml.Runtime.csproj -c Release -o .\artifacts\packages
dotnet pack .\Csxaml.Generator\Csxaml.Generator.csproj -c Release -o .\artifacts\packages
```

Then reference the packages from a WinUI app:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableCsxaml>true</EnableCsxaml>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Csxaml.Generator" Version="0.1.0-preview.1" PrivateAssets="all" />
    <PackageReference Include="Csxaml.Runtime" Version="0.1.0-preview.1" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
  </ItemGroup>
</Project>
```

`Csxaml.Generator` contributes `buildTransitive` MSBuild assets and runs the packaged generator from its `tools` payload. Generated files and maps land under `obj\...\Csxaml`.

## Repo-Local Project Setup

For a repo-local WinUI project, opt in to CSXAML and reference the runtime:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableCsxaml>true</EnableCsxaml>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260209005" />
    <ProjectReference Include="..\Csxaml.Runtime\Csxaml.Runtime.csproj" />
  </ItemGroup>
</Project>
```

The repo root imports `build/Csxaml.props` and `build/Csxaml.targets` through `Directory.Build.props` and `Directory.Build.targets`. Projects outside this repo should use the preview package path above instead of importing repo-local build files.

## First Component

Add a `.csxaml` file, for example `Components/Counter.csxaml`:

```csxaml
namespace MyApp;

component Element Counter {
    State<int> Count = new State<int>(0);

    render <StackPanel Spacing={8}>
        <TextBlock Text={$"Count: {Count.Value}"} />
        <Button Content="Increment" OnClick={() => Count.Value++} />
    </StackPanel>;
}
```

The generated component type is named `CounterComponent`.

## Host The Component

Create a normal WinUI host surface, such as a named panel in `MainWindow.xaml`:

```xml
<StackPanel x:Name="ComponentHost" />
```

This is the current supported host shape. Generated CSXAML app roots that can
replace source `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, and
`MainWindow.xaml.cs` now exist experimentally. In generated mode, use
`component Application` plus `component ResourceDictionary` for the app shell
and resources instead of editing source `App.xaml`; the build emits a hidden
intermediate `App.xaml` for WinUI default resources. Keep deep templates and
theme resources in XAML dictionaries.

Then render the generated component from code-behind:

```csharp
using System;
using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace MyApp;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow(IServiceProvider services)
    {
        InitializeComponent();

        _host = new CsxamlHost(ComponentHost, typeof(CounterComponent), services);
        _host.Render();
    }
}
```

If the component has no injected services, the service provider can be `null`.

## Build

Build the app project:

```powershell
dotnet build .\MyApp\MyApp.csproj
```

The repo samples are the current working reference apps:

```powershell
dotnet build .\samples\Csxaml.ExistingWinUI\Csxaml.ExistingWinUI.csproj
dotnet build .\samples\Csxaml.HelloWorld\Csxaml.HelloWorld.csproj
dotnet build .\samples\Csxaml.TodoApp\Csxaml.TodoApp.csproj
dotnet build .\samples\Csxaml.FeatureGallery\Csxaml.FeatureGallery.csproj
```

## Generated Output

Generated files are under the project intermediate output:

```text
obj\...\Csxaml\Generated\
```

Source-map sidecars are under:

```text
obj\...\Csxaml\Maps\
```

The build also writes CSXAML input/reference/context files under `obj\...\Csxaml\`. These are implementation details, but they are useful when diagnosing generation inputs.

Generated code is intentionally readable, but start with `.csxaml` diagnostics and source maps before opening `.g.cs` files.

## Testing

Use `Csxaml.Testing` for hostless logical-tree component tests.

Current repo-local test projects use project references. A simple test shape is:

```csharp
using Csxaml.Testing;

using var render = CsxamlTestHost.Render<CounterComponent>();
render.Click(render.FindByText("Increment"));
```

Prefer semantic queries such as `FindByAutomationId`, `FindByAutomationName`, and `FindByText` over child-index assertions.

See `docs/component-testing.md` for the full testing guide.

## Troubleshooting

If no generated component type appears:

- confirm `<EnableCsxaml>true</EnableCsxaml>` is set
- confirm the `.csxaml` file is included by the default `**\*.csxaml` glob
- build the project once before using the generated type
- check the generated namespace, which defaults to the file namespace, then `RootNamespace`, then `AssemblyName`

If the generator project cannot be found:

- confirm the repo-local `Directory.Build.props` and `Directory.Build.targets` imports are active
- for an outside-repo experiment, set `CsxamlGeneratorProject` to the local `Csxaml.Generator.csproj`
- for package consumption, confirm the project references `Csxaml.Generator` and restore imported its `buildTransitive` assets

If a native property or event fails validation:

- check `docs/supported-feature-matrix.md`
- check `docs/external-control-interop.md` for external controls
- remember that unsupported controls should fail deterministically instead of falling through to runtime guessing

If an injected service fails:

- pass an `IServiceProvider` to `CsxamlHost`
- register every service declared with `inject Type name;`
- keep services separate from component props

If a compiler error points at generated code:

- check the `.csxaml` diagnostic first
- inspect the matching file under `obj\...\Csxaml\Maps\`
- use `docs/debugging-and-diagnostics.md` for the source-map workflow

If solution tests hit VSIX packaging file locks, use the reliable sequential command:

```powershell
dotnet test .\Csxaml.sln --no-restore -m:1
```
