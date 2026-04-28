---
title: Install VS Code Extension
description: Install and verify the CSXAML VS Code extension.
---

# Install VS Code Extension

The VS Code extension contributes:

- `csxaml` language id
- `.csxaml` file association
- language configuration for comments and bracket pairs
- hybrid TextMate grammar
- snippets
- language-server client for richer editor behavior

## Requirements

- Windows-hosted CSXAML authoring.
- Visual Studio Code.
- .NET 10 Desktop Runtime for the bundled framework-dependent language server.
- A C# extension if you want richer C# coloring inside helper code and expression islands.

## Install

Use the current preview VSIX from the project release or artifact you are
validating, then run **Extensions: Install from VSIX...** in VS Code.

For a repo-built VSIX, the packaged file is written under:

```text
artifacts\vscode\
```

See [Develop the VS Code Extension](visual-studio-code-development.md) when you
need to build that VSIX from source.

## Verify

1. Open a folder containing a `.csxaml` file.
2. Confirm the language mode is `CSXAML`.
3. Type `<But` inside a render body and confirm `Button` completion.
4. Run **CSXAML: Restart Language Server** if semantic features do not appear after install.

## Expected failure modes

If the .NET 10 Desktop Runtime is missing, syntax highlighting may still work
because it comes from the TextMate grammar, but completion, hover, diagnostics,
formatting, and go-to-definition will not start. The output channel should show
a language-server startup failure such as:

```text
Could not load Csxaml.LanguageServer
```

Install the runtime, restart VS Code, and leave `csxaml.languageServer.path`
empty while validating the bundled server.

## Configuration

The extension resolves the language server in this order:

1. `csxaml.languageServer.path`, when configured
2. bundled `LanguageServer/Csxaml.LanguageServer.exe`
3. bundled `LanguageServer/Csxaml.LanguageServer.dll`, launched through `dotnet`
4. repo-local Debug language server during extension development
5. repo-local Release language server during extension development

Most app authors should leave `csxaml.languageServer.path` empty so the bundled
server is used.

## Current limitations

- code actions are intentionally limited to single-symbol suggestion replacements
- broader refactoring and import-management fixes are not implemented yet

See [Language Service Features](language-service-features.md) for the shared
feature list and [Editor Extension Troubleshooting](../troubleshooting/editor-extensions.md)
for language-server startup issues.
