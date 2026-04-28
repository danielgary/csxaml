---
title: Tooling API
description: Overview of CSXAML APIs for editor and language-service integrations.
---

# Tooling API

`Csxaml.Tooling.Core` is the shared language-service library used by the VS Code and Visual Studio integrations.

Major services:

- completion
- diagnostics
- definitions
- formatting
- hover
- semantic tokens
- code actions
- C# projection
- workspace component discovery

Tooling APIs are intended for editor and language-server hosts. They should stay aligned with the language spec and the generator's parser/validation behavior.

Normal app projects should not reference `Csxaml.Tooling.Core`. It is for
editors, analyzers, language-server hosts, and contributor tooling that needs to
inspect `.csxaml` source.

## Most useful services

| Type | Use it for |
| --- | --- |
| [CsxamlCompletionService](xref:Csxaml.Tooling.Core.Completion.CsxamlCompletionService) | Tag, attribute, and value completions. |
| [CsxamlDiagnosticService](xref:Csxaml.Tooling.Core.Diagnostics.CsxamlDiagnosticService) | Editor diagnostics over `.csxaml` source. |
| [CsxamlDefinitionService](xref:Csxaml.Tooling.Core.Definitions.CsxamlDefinitionService) | Go-to-definition for component and symbol references. |
| [CsxamlFormattingService](xref:Csxaml.Tooling.Core.Formatting.CsxamlFormattingService) | Document formatting. |
| [CsxamlHoverService](xref:Csxaml.Tooling.Core.Hover.CsxamlHoverService) | Hover text for tags, attributes, and helper-code symbols. |
| [CsxamlMarkupScanner](xref:Csxaml.Tooling.Core.Markup.CsxamlMarkupScanner) | Markup element and attribute scanning. |
| [CsxamlWorkspaceLoader](xref:Csxaml.Tooling.Core.Projects.CsxamlWorkspaceLoader) | Workspace/project component discovery. |

Use the generated tooling namespace pages, starting with
<xref:Csxaml.Tooling.Core.Completion> and
<xref:Csxaml.Tooling.Core.Diagnostics>, for exact type and member documentation.
