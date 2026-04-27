---
title: Editor Extensions
description: CSXAML editor support for VS Code and Visual Studio.
---

# Editor Extensions

CSXAML editor support is built around a shared tooling stack:

- `Csxaml.Tooling.Core` owns language semantics.
- `Csxaml.LanguageServer` exposes those semantics through LSP.
- The VS Code extension hosts the language server.
- The Visual Studio extension packages the language server into a VSIX.

Start with the editor you use:

- [VS Code Extension](visual-studio-code.md)
- [Visual Studio Extension](visual-studio.md)
- [Language Service Features](language-service-features.md)

Both editor hosts should stay aligned because they share the same language-service implementation.
