---
title: Language Service Features
description: Shared CSXAML editor features available through the language server.
---

# Language Service Features

The shared language-service stack currently provides:

| Feature | Coverage |
| --- | --- |
| Diagnostics | Parser, validation, and projected C# diagnostics for supported regions. |
| Tag completion | Built-in controls, workspace components, referenced components, and imported external controls. |
| Attribute completion | Native props, native events, component props, attached properties, and `Key`. |
| Projected C# completion | Helper-code regions and expression islands. |
| Hover | Markup tags, attributes, attached properties, component parameters, and projected C# helper symbols. |
| Go to definition | Component tags and projected C# symbols when resolvable. |
| Formatting | Baseline mixed markup/C# indentation. |
| Semantic tokens | Tags, props, events, parameters, attached properties, keywords, and embedded C# symbols. |
| Code actions | Single-symbol suggestion replacements for misspelled tags and attributes. |

The VS Code and Visual Studio extensions should expose the same core semantics because both consume `Csxaml.LanguageServer` and `Csxaml.Tooling.Core`.
