---
title: Develop VS Code Extension
description: Build, package, and iterate on the CSXAML VS Code extension from source.
---

# Develop VS Code Extension

Use this page when you are changing the VS Code extension or bundled language
server.

## Build the package

From the repo root:

```powershell
powershell .\scripts\Package-VSCodeExtension.ps1
```

That packaging flow:

1. stages the extension under `artifacts/vscode-extension/staging`
2. runs `npm ci`
3. publishes `Csxaml.LanguageServer` into a bundled `LanguageServer\` folder
4. creates a `.vsix` under `artifacts\vscode\`

## Iterate locally

Use `csxaml.languageServer.path` when you need VS Code to launch a specific
language-server build. Leave it unset when validating the packaged bundled
server.

Run **CSXAML: Restart Language Server** after rebuilding the server while
iterating locally.

## Verify the package

Install the generated `.vsix` into an isolated VS Code profile or extensions
directory, then verify:

1. `.csxaml` files use the `CSXAML` language mode.
2. TextMate highlighting works before the server starts.
3. Completion, diagnostics, hover, formatting, and go-to-definition work after
   the server starts.
4. The CSXAML output channel shows the bundled server path.

See [Editor Extension Troubleshooting](../troubleshooting/editor-extensions.md)
for language-server startup failures.
