namespace Csxaml.Tooling.Core.Projects;

internal sealed record CsxamlCachedComponentSymbol(
    CsxamlFileStamp Stamp,
    CsxamlWorkspaceComponentSymbol? Symbol);
