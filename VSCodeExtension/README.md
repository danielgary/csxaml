# CSXAML VS Code Extension

This folder contains a standalone VS Code extension that adds syntax highlighting for `.csxaml` files.

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
- a lightweight semantic token provider

The grammar intentionally splits the file into two broad regions:

- XAML-shaped markup uses XML/XAML-style scopes for tags, attributes, strings, and punctuation
- C# islands use embedded C# scopes in these contexts:
  - component parameter lists
  - `State<T>` declarations and initializers
  - `if (...)` headers
  - `foreach (...)` headers
  - attribute expressions such as `Text={Title}` or `OnClick={() => Save()}`

The semantic token provider adds a second layer on top of the grammar:

- native WinUI tags are marked as default-library classes
- component tags are marked as component classes
- native props are marked as properties
- native events are marked as events
- component props are marked as parameters
- `Key` is marked as a reserved readonly framework attribute

## Why This Approach

This matches the language spec:

- structure should feel like XAML
- expressions should stay ordinary C#
- parseability matters
- the language should remain easy to reason about and easy to tool

The TextMate grammar is a good first phase because it gives immediate value with low complexity.

## Current Limitations

This is a syntax-highlighting-first extension, not a full language server.

That means:

- it does not validate native props or component props
- it does not provide completion, hover, go-to-definition, or diagnostics
- deeply complex nested C# may still benefit from a future parser-backed semantic token layer
- semantic tokens currently use a lightweight workspace scan rather than the full compiler parser

The grammar is designed so a later semantic layer can sit on top of it cleanly.

## Best Results

For the best embedded C# coloring, use this extension alongside a C# extension in VS Code.

The markup side does not depend on an external XAML extension because the grammar already emits XML/XAML-style scopes for tags and attributes.

## Folder Layout

- `package.json`: VS Code extension manifest
- `extension.js`: activation and provider registration
- `language-configuration.json`: comments and pair behavior
- `syntaxes/csxaml.tmLanguage.json`: hybrid TextMate grammar
- `syntaxes/csxaml-embedded-csharp.tmLanguage.json`: recursive embedded C# regions for expressions and control-flow headers
- `src/`: lightweight semantic-token and workspace-catalog logic
- `snippets/csxaml.code-snippets`: common CSXAML snippets

## Development

Typical workflow:

1. Open `VSCodeExtension` as the workspace folder in VS Code.
2. Press `F5` to launch an Extension Development Host.
3. Open a `.csxaml` file in the extension host and inspect tokenization.
4. Refine scopes based on the language spec and real sample files.

## Next Steps

Good follow-up work for this extension:

- surface native tag, prop, and event metadata from the compiler metadata model
- add component-prop completion from generated component catalogs
- replace the lightweight semantic token parser with a parser-backed provider
