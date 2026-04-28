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

## Recognizable diagnostics

Unknown tag:

```text
TodoBoard.csxaml(12,13): unsupported tag name 'Textblock'
```

Fix the tag spelling or import the namespace that makes the control/component
visible:

```csharp
<TextBlock Text={Title} />
```

Unknown attribute:

```text
TodoBoard.csxaml(18,21): unknown attribute 'OnClicked' on native control 'Button'
```

Use the supported event name:

```csharp
<Button Content="Save" OnClick={Save} />
```

Mapped C# error from helper code:

```text
TodoBoard.csxaml(24,28): error CS0103: The name 'MissingSymbol' does not exist in the current context
```

Start at the `.csxaml` line first. Open generated code only if the mapped source
location does not explain the failure.

## Source-map files

Generated maps are written next to generated C#:

```text
obj\Debug\net10.0-windows10.0.19041.0\Csxaml\Maps\TodoBoard.map.json
```

A map identifies the source file, generated file, component, and regions such
as tag, property, helper-code, and control-flow blocks. Use it when a compiler
or runtime message still names generated C#.
