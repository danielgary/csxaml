---
title: Debugging and Diagnostics
description: How CSXAML reports parser, validation, build, source-map, and runtime errors.
---

# Debugging and Diagnostics

CSXAML diagnostics should point back to `.csxaml` source instead of forcing users into generated C#.

Important diagnostics surfaces:

- parser diagnostics for invalid CSXAML syntax
- validation diagnostics for unknown tags, props, events, and attached properties
- generated C# `#line` mappings for source-authored helper code
- source-map sidecars under `obj`
- runtime exception context that names component, tag, member, and stage when available

Generated files are written under:

```text
obj\<configuration>\<tfm>\Csxaml\
```

When debugging:

1. Start with the `.csxaml` diagnostic.
2. Check generated C# only when a mapped C# compiler error needs inspection.
3. Use source maps to connect generated output back to authored source.
4. Keep runtime exceptions tied to component names and render stages when reporting issues.
