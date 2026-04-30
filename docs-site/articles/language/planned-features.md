---
title: Planned Features
description: Post-v1 CSXAML feature directions that are planned or experimental.
---

# Planned Features

The supported feature surface is intentionally smaller than the long-term CSXAML
direction. This page summarizes post-v1 work that is either planned or still
experimental.

For the current release posture, use [Supported Features](supported-features.md)
as the source of truth. The language specification also records planned
directions in a non-normative future-work section.

Planned post-v1 areas:

- broader event-argument projection beyond the current experimental common
  WinUI event-args set
- broader element-ref scenarios beyond the current experimental native
  `ElementRef<T>` and `Ref={...}` slice
- broader content interop beyond the current experimental default
  content-property metadata for `[ContentProperty]` controls
- broader property-content targets beyond the current experimental
  metadata-backed `<Owner.Property>` syntax for native property content and
  component named slots
- broader attached-property metadata beyond the current experimental built-in
  layout, tooltip, and automation owner set, especially external owner
  discovery
- generated `Application`, `Window`, `Page`, and `ResourceDictionary` roots,
  including a generated app mode that can remove the default WinUI shell files
- deeper resource/template authoring beyond the current guidance page and
  generated merged-dictionary slice
- first-class large-list virtualization abstractions beyond the current
  guidance and native-control interop boundary
- broader app-hosted sample presenter integration beyond the current
  `samples/Csxaml.FeatureGallery` fallback classifier and the
  grammar/snippet/DocFX/editor fixture alignment

Until these areas move out of `Not in v1` or `Experimental`, current apps
should keep using the documented WinUI shell, `CsxamlHost`, supported metadata,
and native virtualization-aware controls for large scrolling surfaces.
