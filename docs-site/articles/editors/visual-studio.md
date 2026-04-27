---
title: Visual Studio Extension
description: Install, bootstrap, verify, and troubleshoot the CSXAML Visual Studio extension.
---

# Visual Studio Extension

The Visual Studio extension uses the shared CSXAML language stack rather than a Visual-Studio-only semantic fork.

Current extension shape:

- `Csxaml.Tooling.Core` owns shared editor semantics
- `Csxaml.LanguageServer` exposes those semantics over LSP
- `Csxaml.VisualStudio` packages the server into a `.vsix`
- the VSIX includes the `LanguageServer\` payload

## Visual Studio requirement

The current host path targets the Visual Studio 2026 / version 18 VisualStudio.Extensibility model.

Expected local environment:

- Visual Studio 2026 / version 18
- Visual Studio extension development workload
- machine-wide .NET 10 runtime under `C:\Program Files\dotnet`

## Build

```powershell
dotnet build .\Csxaml.VisualStudio\Csxaml.VisualStudio.csproj
```

The VSIX is produced under:

```text
Csxaml.VisualStudio\bin\Debug\net8.0-windows8.0\
```

## Experimental instance bootstrap

For contributor testing:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run-CsxamlVisualStudioBootstrap.ps1
```

Close any existing experimental instance before rerunning the bootstrap script.

## Verify

In the experimental instance:

1. Open `Csxaml.Demo/Components/TodoCard.csxaml`.
2. Confirm semantic coloring is active.
3. Type `<But` and confirm `Button` completion.
4. Inside a button tag, type `OnC` and confirm `OnClick` completion.
5. Introduce an invalid attribute and confirm diagnostics appear.
6. Use go-to-definition on a component tag.
7. Run format document.

See [Editor Troubleshooting](../troubleshooting/editor-extensions.md) if the extension starts without CSXAML language features.
