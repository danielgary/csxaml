# CSXAML VS Code Extension

This folder contains the VS Code extension for `.csxaml` files.

The public marketplace publisher id is `danielgarysoftware`, the extension is licensed under Apache-2.0, and the runtime entrypoint is the committed bundle at `dist/extension.js`. That means the repo-local extension can activate without a separate `npm install` just to satisfy `vscode-languageclient`.

## Goals

The extension is based on [LANGUAGE-SPEC.md](../LANGUAGE-SPEC.md), with three priorities:

- keep CSXAML visually familiar to C# and XAML developers
- use explicit syntax boundaries from the spec so highlighting stays maintainable
- treat embedded C# as real C# where the language says C# is allowed

## What It Does

The current extension contributes:

- a `csxaml` language id
- `.csxaml` file association
- a language configuration for comments and core bracket pairs
- a hybrid TextMate grammar
- snippets for common CSXAML constructs, generated roots, refs, property
  content, and named slots
- a shared language-server client for completion, hover, diagnostics, formatting, definitions, semantic tokens, and suggestion-based quick fixes

This extension also has a packaged install path:

- `scripts/Package-VSCodeExtension.ps1` stages the extension under `artifacts/vscode-extension/staging`
- that packaging flow publishes `Csxaml.LanguageServer` into a bundled `LanguageServer/` folder there
- the packaged `.vsix` is emitted under `artifacts/vscode/`
- the packaged extension currently targets Windows-hosted CSXAML authoring and expects the .NET 10 Desktop Runtime

The grammar intentionally splits the file into two broad regions:

- XAML-shaped markup uses XML/XAML-style scopes for tags, attributes, strings, and punctuation
- C# islands use embedded C# scopes in these contexts:
  - component parameter lists
  - `State<T>` declarations and initializers
  - `if (...)` headers
  - `foreach (...)` headers
  - attribute expressions such as `Text={Title}` or `OnClick={() => Save()}`
- generated root declarations such as `component Application App { ... }`,
  `component Window MainWindow { ... }`, and
  `component ResourceDictionary AppResources { ... }` are recognized directly
- application declarations such as `startup` and `resources` stay out of the
  helper-code fallback
- property-content tags such as `<Button.Flyout>` and native `Ref` attributes
  use the same markup grammar as the rest of CSXAML
- ordinary helper-code regions now fall back to `source.cs` scopes when the host has C# grammar support available

The language server adds the semantic/editor layer on top of the grammar:

- tag completion for built-in controls, imported external controls, and workspace components
- attribute completion for native props, events, component props, attached properties, and `Key`
- projected C# completion inside helper-code regions and expression islands
- hover for markup tags/attributes plus projected C# helper-code symbols
- editor diagnostics
- go-to-definition for component tags
- document formatting
- semantic tokens produced by the shared tooling stack

The same TextMate grammar is also loaded by the DocFX Shiki post-build
highlighter, so docs and VS Code should color `csxaml` examples consistently.
`samples/Csxaml.FeatureGallery` includes an app-hosted `SampleCodePresenter`
with a deliberately small fallback classifier. That presenter is a sample UI
control, not a replacement for the shared grammar used by VS Code and DocFX.

## Why This Approach

This matches the language spec:

- structure should feel like XAML
- expressions should stay ordinary C#
- parseability matters
- the language should remain easy to reason about and easy to tool
- editor semantics should come from the same shared stack used by the Visual Studio extension

The TextMate grammar still matters because it gives immediate structure-aware coloring, while the shared language server provides richer editor behavior.

## Current Limitations

This extension uses the shared `Csxaml.LanguageServer`, but the server still only exposes the features that exist in the current tooling stack.

That means:

- code actions are intentionally limited to single-symbol suggestion replacements
- broader refactoring and import-management fixes are not implemented yet

## Best Results

For the best embedded C# coloring, use this extension alongside a C# extension in VS Code.

The markup side does not depend on an external XAML extension because the grammar already emits XML/XAML-style scopes for tags and attributes.

## Folder Layout

- `package.json`: VS Code extension manifest
- `extension.js`: source entrypoint used to build the bundle
- `dist/extension.js`: committed runtime bundle used by VS Code
- `language-configuration.json`: comments and pair behavior
- `syntaxes/csxaml.tmLanguage.json`: hybrid TextMate grammar
- `syntaxes/csxaml-embedded-csharp.tmLanguage.json`: recursive embedded C# regions for expressions and control-flow headers
- `src/languageServerPathResolver.js`: resolves local and packaged language-server paths
- `snippets/csxaml.code-snippets`: common CSXAML snippets
- `scripts/bundle-extension.mjs`: bundles the extension entrypoint for packaging and local debug use

## Development

Typical workflow:

1. Open the repo root in VS Code.
2. Run `npm install` in `VSCodeExtension` when you need to change bundle dependencies or regenerate the extension bundle.
3. Press `F5` with the `VS Code Ext Host + CSXAML` launch profile.
4. That launch profile builds `Csxaml.LanguageServer` before opening the Extension Development Host.
5. Run `npm run bundle` in `VSCodeExtension` after changing `extension.js` or `src/**/*`.
6. Open a `.csxaml` file in the extension host and test completion, diagnostics, formatting, navigation, and hover.

By default, repo-local development keeps language-client trace off so hover and completion stay closer to normal editor speed. Turn on `csxaml.languageServer.trace` only when you need protocol-level debugging.

## Packaged Path

The packaged extension is aimed at Windows-hosted CSXAML authoring outside the repo.

Package it from the repo root:

```powershell
powershell .\scripts\Package-VSCodeExtension.ps1
```

That flow:

1. reads the shared repo release version from `build/Csxaml.props`
2. stages the extension into `artifacts/vscode-extension/staging`
3. runs `npm ci`
4. publishes `Csxaml.LanguageServer` into a bundled `LanguageServer/` folder
5. creates a `.vsix` under `artifacts/vscode/`

Packaging prerequisites:

- Node.js 20+ with `npm` and `npx` available on `PATH`
- .NET 10 SDK to publish `Csxaml.LanguageServer`

Runtime prerequisites for the packaged extension:

- Windows
- .NET 10 Desktop Runtime for the bundled framework-dependent language server

The extension resolves the language server in this order:

1. `csxaml.languageServer.path` if configured
2. `LanguageServer/Csxaml.LanguageServer.exe` under the extension folder
3. `LanguageServer/Csxaml.LanguageServer.dll` under the extension folder, launched through `dotnet`
4. `../Csxaml.LanguageServer/bin/Debug/net10.0/Csxaml.LanguageServer.exe` during extension development
5. `../Csxaml.LanguageServer/bin/Debug/net10.0/Csxaml.LanguageServer.dll` during extension development, launched through `dotnet`
6. `../Csxaml.LanguageServer/bin/Release/net10.0/Csxaml.LanguageServer.exe` during extension development
7. `../Csxaml.LanguageServer/bin/Release/net10.0/Csxaml.LanguageServer.dll` during extension development, launched through `dotnet`

Use the `CSXAML: Restart Language Server` command after rebuilding the server while iterating.

## Next Steps

Good follow-up work for this extension:

- broaden code-action coverage beyond suggestion-based replacements
- add VS Code-side smoke coverage for startup and core LSP flows
