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
