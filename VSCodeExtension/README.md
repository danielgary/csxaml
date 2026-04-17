# CSXAML VS Code Extension

This folder contains a VS Code extension for `.csxaml` files.

The public marketplace publisher id is `danielgarysoftware`, and the extension is licensed under Apache-2.0.

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
- snippets for common CSXAML constructs
- a shared language-server client for completion, diagnostics, formatting, definitions, and semantic tokens

The grammar intentionally splits the file into two broad regions:

- XAML-shaped markup uses XML/XAML-style scopes for tags, attributes, strings, and punctuation
- C# islands use embedded C# scopes in these contexts:
  - component parameter lists
  - `State<T>` declarations and initializers
  - `if (...)` headers
  - `foreach (...)` headers
  - attribute expressions such as `Text={Title}` or `OnClick={() => Save()}`

The language server adds the semantic/editor layer on top of the grammar:

- tag completion for built-in controls, imported external controls, and workspace components
- attribute completion for native props, events, component props, attached properties, and `Key`
- projected C# completion inside helper-code regions and expression islands
- editor diagnostics
- go-to-definition for component tags
- document formatting
- semantic tokens produced by the shared tooling stack

## Why This Approach

This matches the language spec:

- structure should feel like XAML
- expressions should stay ordinary C#
- parseability matters
- the language should remain easy to reason about and easy to tool
- editor semantics should come from the same shared stack used by the Visual Studio extension

The TextMate grammar still matters because it gives immediate structure-aware coloring, while the shared language server provides richer editor behavior.

## Current Limitations

This extension now uses the shared `Csxaml.LanguageServer`, but the server still only exposes the features that exist in the current tooling stack.

That means:

- hover is not implemented yet
- the local development loop still assumes you can build `Csxaml.LanguageServer`

## Best Results

For the best embedded C# coloring, use this extension alongside a C# extension in VS Code.

The markup side does not depend on an external XAML extension because the grammar already emits XML/XAML-style scopes for tags and attributes.

## Folder Layout

- `package.json`: VS Code extension manifest
- `extension.js`: source entrypoint used for local bundling
- `dist/`: bundled runtime entrypoint used by the packaged extension
- `language-configuration.json`: comments and pair behavior
- `syntaxes/csxaml.tmLanguage.json`: hybrid TextMate grammar
- `syntaxes/csxaml-embedded-csharp.tmLanguage.json`: recursive embedded C# regions for expressions and control-flow headers
- `src/languageServerPathResolver.js`: resolves local and packaged language-server paths
- `snippets/csxaml.code-snippets`: common CSXAML snippets
- `scripts/bundle-extension.mjs`: bundles the extension entrypoint for packaging and local debug use

## Development

Typical workflow:

1. Open `VSCodeExtension` as the workspace folder in VS Code.
2. Run `npm install`.
3. Press `F5` to launch an Extension Development Host.
4. The launch profile bundles the extension entrypoint and builds `Csxaml.LanguageServer` before starting the extension host.
5. Open a `.csxaml` file in the extension host and test completion, diagnostics, formatting, and navigation.

The extension resolves the language server in this order:

1. `csxaml.languageServer.path` if configured
2. `LanguageServer/Csxaml.LanguageServer.exe` under the extension folder
3. `../Csxaml.LanguageServer/bin/Debug/net10.0/Csxaml.LanguageServer.exe`
4. `../Csxaml.LanguageServer/bin/Release/net10.0/Csxaml.LanguageServer.exe`

Use the `CSXAML: Restart Language Server` command after rebuilding the server while iterating.

## Next Steps

Good follow-up work for this extension:

- add hover and richer code actions once the shared language server grows them
- add VS Code-side smoke coverage for startup and core LSP flows
