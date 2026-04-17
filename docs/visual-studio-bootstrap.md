# Visual Studio Integration and Bootstrap

This document describes the landed Milestone 11 Visual Studio path for CSXAML.

Milestone 11 is now complete for the current Visual Studio 18 / 2026 host story:

- `Csxaml.Tooling.Core` owns shared CSXAML editor semantics
- `Csxaml.LanguageServer` exposes those semantics through one language-server boundary
- `Csxaml.VisualStudio` packages the server into a real `.vsix`
- the experimental-instance bootstrap loop stages and launches that extension reliably
- Visual Studio can provide semantic coloring, completion, diagnostics, formatting, and go-to-definition for `.csxaml`

The important architectural point is that Visual Studio is using the shared language stack rather than a Visual-Studio-only semantic fork.

## Delivered authoring slice

The current extension delivers:

- semantic coloring for CSXAML keywords, tags, props, events, component parameters, and attached properties
- tag completion for native controls, workspace components, referenced components, and imported external controls
- attribute completion for props, events, component parameters, attached properties, and `Key`
- projected C# completion inside helper-code regions and expression islands
- live editor diagnostics
- baseline document formatting and indentation
- go-to-definition for component usages that resolve to source files in the workspace

## Build

From the repo root:

```powershell
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj
```

That produces the VSIX container under:

```text
Csxaml.VisualStudio\bin\Debug\net8.0-windows8.0\Csxaml.VisualStudio.vsix
```

The VSIX includes the packaged `LanguageServer\` payload, including `Csxaml.LanguageServer.exe`, its runtime config, and shared tooling assemblies.

## Visual Studio requirement

The current host path targets the Visual Studio 2026 / version 18 VisualStudio.Extensibility model.

Expected local environment:

- Visual Studio 2026 / version 18, either Insiders or regular
- the `Visual Studio extension development` workload
- a machine-wide `.NET 10` runtime under `C:\Program Files\dotnet`

The extension itself declares `net8.0;net10.0` in `DotnetTargetVersions` so Visual Studio 18 does not warn that the package only supports the older host runtime lane.

The language server is still a framework-dependent `net10.0` executable. Visual Studio may inject its own private `.NET 8` runtime path during activation, so the host resolves a compatible `DOTNET_ROOT` from the machine-wide install when necessary.

## Install into the experimental instance

Use the helper script:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run-CsxamlVisualStudioBootstrap.ps1
```

That script:

- builds the extension with Visual Studio `MSBuild.exe`
- replaces packaged `Microsoft.VisualStudio.Extensibility*.dll` files with the versions from the selected Visual Studio 18 instance
- rewrites the generated manifest and packaged VSIX manifest for the targeted Visual Studio SKU/runtime lane
- clears stale experimental-instance caches
- deploys through Visual Studio's own VSIX deployment targets
- runs `devenv /RootSuffix Exp /UpdateConfiguration`
- launches the experimental instance with `Csxaml.sln`

Before rerunning the bootstrap, close any existing experimental instance so cache refresh and deployment can apply cleanly.

## Verify

In the experimental instance:

1. Open `Csxaml.Demo/Components/TodoCard.csxaml` or `Csxaml.Demo/Components/TodoBoard.csxaml`.
2. Confirm semantic coloring is active for tags and attributes.
3. Type `<But` and confirm completion offers `Button`.
4. Inside `<Button ...>`, type `OnC` and confirm completion offers `OnClick`.
5. On a component usage such as `<TodoCard ...>`, confirm component props like `Title` complete.
6. Introduce an invalid attribute such as `<Button Text="Save" />` and confirm the editor surfaces a diagnostic.
7. Use go-to-definition on `TodoCard` and confirm navigation opens the component source file.
8. Run format document and confirm mixed markup/C# indentation remains stable.

If those steps work, the Milestone 11 authoring bar is met for the current Visual Studio host path.

## Automated proof in the repo

The repo now carries focused automated proof for the milestone:

- tooling-core tests for tag completion, component-prop completion, native event completion, semantic tokens, formatting, definitions, and projected C# completion/diagnostics
- language-server protocol tests that exercise initialize, open, completion, definition, semantic tokens, formatting, and diagnostics notifications
- Visual Studio packaging tests that verify the generated manifest runtime targets and the packaged VSIX language-server payload

## Troubleshooting

If `.csxaml` still opens without CSXAML editor features:

1. Close the experimental instance.
2. Run the bootstrap script again.
3. Check the Visual Studio activity log under `%LOCALAPPDATA%\Microsoft\VisualStudio\<version>_<instanceId>Exp\ActivityLog.xml`.
4. Check `%TEMP%\csxaml-visualstudio.log` for extension and language-server startup breadcrumbs.
5. If the log says the language server needs `Microsoft.NETCore.App` `10.0.x`, confirm `dotnet --list-runtimes` includes a `Microsoft.NETCore.App 10.0.x` entry under `C:\Program Files\dotnet`.
6. If the extension appears in the experimental hive but is disabled, re-enable it in Extensions and restart the instance.
7. If the Extension Manager warns that the package only supports `net8.0`, redeploy the current VSIX; the generated manifest should now declare `net8.0;net10.0`.
8. If restore/design-time errors mention only `VisualStudioOfflinePackages`, reload the solution so Visual Studio picks up the repo `NuGet.Config` that includes `NuGet.org`.
9. If `%TEMP%\csxaml-visualstudio.log` never appears, inspect the deployed extension folder and confirm the packaged `LanguageServer\` payload is present.

`VSIXInstaller` failures that mention `RequiresInstallerException` are expected for this extension type. Use the bootstrap script instead of double-clicking the `.vsix`.
