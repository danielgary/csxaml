using Csxaml.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

public sealed record CsxamlWorkspaceComponentSymbol(
    ComponentMetadata Metadata,
    string FilePath,
    int NameStart,
    int NameLength);
