namespace Csxaml.Runtime;

public sealed record CsxamlRuntimeFrame(
    string Stage,
    string? ComponentName,
    CsxamlSourceInfo? SourceInfo,
    string? Detail);
