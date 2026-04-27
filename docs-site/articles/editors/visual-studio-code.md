---
title: VS Code Extension
description: Install, configure, package, and troubleshoot the CSXAML VS Code extension.
---

# VS Code Extension

The VS Code extension contributes:

- `csxaml` language id
- `.csxaml` file association
- language configuration for comments and bracket pairs
- hybrid TextMate grammar
- snippets
- language-server client for richer editor behavior

## Runtime prerequisites

The packaged extension targets Windows-hosted CSXAML authoring and expects the .NET 10 Desktop Runtime for the bundled framework-dependent language server.

For best C# coloring inside helper code and expression islands, use the extension alongside a C# extension.

## Packaged path

From the repo root:

```powershell
powershell .\scripts\Package-VSCodeExtension.ps1
```

That packaging flow:

1. stages the extension under `artifacts/vscode-extension/staging`
2. runs `npm ci`
3. publishes `Csxaml.LanguageServer` into a bundled `LanguageServer\` folder
4. creates a `.vsix` under `artifacts/vscode\`

## Language server resolution

The extension resolves the language server in this order:

1. `csxaml.languageServer.path`, when configured
2. bundled `LanguageServer/Csxaml.LanguageServer.exe`
3. bundled `LanguageServer/Csxaml.LanguageServer.dll`, launched through `dotnet`
4. repo-local Debug language server during extension development
5. repo-local Release language server during extension development

Use `CSXAML: Restart Language Server` after rebuilding the server while iterating locally.

## Current limitations

- code actions are intentionally limited to single-symbol suggestion replacements
- broader refactoring and import-management fixes are not implemented yet

See [Language Service Features](language-service-features.md) for the shared feature list.
