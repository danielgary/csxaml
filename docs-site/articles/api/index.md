---
title: API Overview
description: How CSXAML packages and namespaces are organized for app authors, tests, tooling, and metadata integrations.
---

# API Overview

The API reference is generated from XML documentation comments during the docs build. Do not edit generated API pages by hand.

Start here to choose the right surface:

- [Packages and Namespaces](packages-and-namespaces.md)
- [Runtime API](runtime.md)
- [Testing API](testing.md)
- [Tooling API](tooling.md)
- [Metadata API](metadata.md)

Generated reference entry points:

- <xref:Csxaml.Runtime>
- <xref:Csxaml.Testing>
- <xref:Csxaml.ControlMetadata>
- <xref:Csxaml.VisualStudio>
- <xref:Csxaml.Tooling.Core.Completion>
- <xref:Csxaml.Tooling.Core.Diagnostics>
- <xref:Csxaml.Tooling.Core.Projects>

## Most app authors need

- [CsxamlHost](xref:Csxaml.Runtime.CsxamlHost)
- [State&lt;T&gt;](xref:Csxaml.Runtime.State`1)
- [ComponentInstance](xref:Csxaml.Runtime.ComponentInstance)
- [CsxamlTestHost](xref:Csxaml.Testing.CsxamlTestHost)
- [CsxamlRenderResult](xref:Csxaml.Testing.CsxamlRenderResult)

## Most tooling authors need

- [CsxamlMarkupScanner](xref:Csxaml.Tooling.Core.Markup.CsxamlMarkupScanner)
- [CsxamlDiagnosticService](xref:Csxaml.Tooling.Core.Diagnostics.CsxamlDiagnosticService)
- [CsxamlCompletionService](xref:Csxaml.Tooling.Core.Completion.CsxamlCompletionService)
- [CsxamlDefinitionService](xref:Csxaml.Tooling.Core.Definitions.CsxamlDefinitionService)
- [CsxamlFormattingService](xref:Csxaml.Tooling.Core.Formatting.CsxamlFormattingService)
- [CsxamlWorkspaceLoader](xref:Csxaml.Tooling.Core.Projects.CsxamlWorkspaceLoader)

Generated reference pages are produced from these projects:

- `Csxaml.ControlMetadata`
- `Csxaml.Runtime`
- `Csxaml.Testing`
- `Csxaml.Tooling.Core`
- `Csxaml.VisualStudio`

The generated output is written under `obj/docfx` at build time and published
under `api/` in the static site.
