namespace Csxaml.Tooling.Core.Definitions;

/// <summary>
/// Describes the source location of a CSXAML definition target.
/// </summary>
/// <param name="FilePath">The file path containing the definition.</param>
/// <param name="Start">The zero-based start offset of the definition name.</param>
/// <param name="Length">The length of the definition name.</param>
public sealed record CsxamlDefinitionLocation(
    string FilePath,
    int Start,
    int Length);
