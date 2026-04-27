using Csxaml.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Describes a CSXAML component discovered in the current workspace.
/// </summary>
/// <param name="Metadata">The component metadata used by tooling features.</param>
/// <param name="FilePath">The source file path containing the component declaration.</param>
/// <param name="NameStart">The zero-based start offset of the component name.</param>
/// <param name="NameLength">The length of the component name.</param>
public sealed record CsxamlWorkspaceComponentSymbol(
    ComponentMetadata Metadata,
    string FilePath,
    int NameStart,
    int NameLength);
