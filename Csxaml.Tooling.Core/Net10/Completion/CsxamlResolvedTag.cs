using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Projects;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Completion;

public sealed record CsxamlResolvedTag(
    CsxamlResolvedTagKind Kind,
    string TagName,
    ControlMetadataModel? Control,
    CsxamlWorkspaceComponentSymbol? Component);
