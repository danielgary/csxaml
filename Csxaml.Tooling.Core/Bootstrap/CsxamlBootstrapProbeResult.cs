namespace Csxaml.Tooling.Core.Bootstrap;

/// <summary>
/// Describes the result of probing a document for CSXAML editor support.
/// </summary>
/// <param name="IsCsxamlFile">A value indicating whether the document has a CSXAML file name.</param>
/// <param name="DisplayName">The file name displayed in editor-facing messages.</param>
/// <param name="NormalizedFileName">The trimmed file name or path used for comparison.</param>
/// <param name="Message">A human-readable bootstrap status message.</param>
public sealed record CsxamlBootstrapProbeResult(
    bool IsCsxamlFile,
    string DisplayName,
    string NormalizedFileName,
    string Message);
