---
title: Install Visual Studio Extension
description: Install and verify the CSXAML Visual Studio extension.
---

# Install Visual Studio Extension

The Visual Studio extension uses the shared CSXAML language stack rather than a Visual-Studio-only semantic fork.

Current extension shape:

- `Csxaml.Tooling.Core` owns shared editor semantics
- `Csxaml.LanguageServer` exposes those semantics over LSP
- `Csxaml.VisualStudio` packages the server into a `.vsix`
- the VSIX includes the `LanguageServer\` payload

## Visual Studio requirement

The current host path targets the Visual Studio 2026 / version 18 VisualStudio.Extensibility model.

Expected environment:

- Visual Studio 2026 / version 18
- machine-wide .NET 10 runtime under `C:\Program Files\dotnet`

## Install

Use the current preview VSIX from the project release or artifact you are
validating. Install it into a compatible Visual Studio 2026 / version 18
instance.

The source-build bootstrap path is for contributors and extension validation.
See [Develop the Visual Studio Extension](visual-studio-development.md) for that
workflow.

## Verify installation

In Visual Studio:

1. Open `Csxaml.Demo/Components/TodoCard.csxaml`.
2. Confirm semantic coloring is active.
3. Type `<But` and confirm `Button` completion.
4. Inside a button tag, type `OnC` and confirm `OnClick` completion.
5. Introduce an invalid attribute and confirm diagnostics appear.
6. Use go-to-definition on a component tag.
7. Run format document.

See [Editor Troubleshooting](../troubleshooting/editor-extensions.md) if the extension starts without CSXAML language features.
