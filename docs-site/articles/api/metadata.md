---
title: Metadata API
description: Overview of CSXAML control metadata APIs used by generator, runtime, and tooling code.
---

# Metadata API

`Csxaml.ControlMetadata` defines the shared model for native and external controls.

The metadata model describes:

- control types
- properties
- events
- child-content rules
- value-kind hints
- event binding kinds

Generator, runtime, and tooling code use this shared model so validation, emission, editor completions, hover, and runtime projection stay consistent.

Normal app authors should not construct or mutate metadata registries. Use
ordinary package/project references and `using` directives; let the generator
produce the metadata and runtime registration needed for supported native and
external controls.

## Most useful types

| Type | Use it for |
| --- | --- |
| [ControlMetadataRegistry](xref:Csxaml.ControlMetadata.ControlMetadataRegistry) | Looking up built-in control metadata by tag name. |
| [ControlMetadata](xref:Csxaml.ControlMetadata.ControlMetadata) | Describing a supported native or external control. |
| [PropertyMetadata](xref:Csxaml.ControlMetadata.PropertyMetadata) | Describing a supported property. |
| [EventMetadata](xref:Csxaml.ControlMetadata.EventMetadata) | Describing a supported event and delegate shape. |
| [AttachedPropertyMetadataRegistry](xref:Csxaml.ControlMetadata.AttachedPropertyMetadataRegistry) | Looking up supported attached properties. |
| [CompiledComponentManifest](xref:Csxaml.ControlMetadata.CompiledComponentManifest) | Passing generated component metadata across assembly boundaries. |

Use <xref:Csxaml.ControlMetadata> for exact type and member documentation.
