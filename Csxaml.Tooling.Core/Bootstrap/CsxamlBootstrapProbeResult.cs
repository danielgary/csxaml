namespace Csxaml.Tooling.Core.Bootstrap;

public sealed record CsxamlBootstrapProbeResult(
    bool IsCsxamlFile,
    string DisplayName,
    string NormalizedFileName,
    string Message);
