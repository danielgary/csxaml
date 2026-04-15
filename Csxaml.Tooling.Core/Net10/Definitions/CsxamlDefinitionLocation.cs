namespace Csxaml.Tooling.Core.Definitions;

public sealed record CsxamlDefinitionLocation(
    string FilePath,
    int Start,
    int Length);
