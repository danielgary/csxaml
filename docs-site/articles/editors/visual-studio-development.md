---
title: Develop Visual Studio Extension
description: Build, bootstrap, verify, and troubleshoot the CSXAML Visual Studio extension from source.
---

# Develop Visual Studio Extension

Use this page when you are changing the Visual Studio host or validating a
source-built VSIX.

## Requirements

- Visual Studio 2026 / version 18.
- Visual Studio extension development workload.
- Machine-wide .NET 10 runtime under `C:\Program Files\dotnet`.
- .NET SDK 8 for the current VSIX host project.

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

See [Editor Troubleshooting](../troubleshooting/editor-extensions.md) if the
extension starts without CSXAML language features.
