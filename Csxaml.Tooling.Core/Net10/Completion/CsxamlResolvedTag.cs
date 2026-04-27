using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Projects;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Completion;

/// <summary>
/// Describes how a CSXAML tag resolves in the current workspace context.
/// </summary>
/// <param name="Kind">The tag resolution kind.</param>
/// <param name="TagName">The tag name that was resolved.</param>
/// <param name="Control">The native or external control metadata, when the tag resolved to a control.</param>
/// <param name="Component">The component symbol, when the tag resolved to a component.</param>
public sealed record CsxamlResolvedTag(
    CsxamlResolvedTagKind Kind,
    string TagName,
    ControlMetadataModel? Control,
    CsxamlWorkspaceComponentSymbol? Component);
