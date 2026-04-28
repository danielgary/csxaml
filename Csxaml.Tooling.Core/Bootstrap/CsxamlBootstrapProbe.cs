namespace Csxaml.Tooling.Core.Bootstrap;

/// <summary>
/// Detects whether an editor document should be treated as CSXAML.
/// </summary>
public sealed class CsxamlBootstrapProbe
{
    /// <summary>
    /// The editor content type name used for CSXAML documents.
    /// </summary>
    public const string ContentTypeName = "csxaml";

    /// <summary>
    /// Probes a file name and returns CSXAML bootstrap metadata for editor hosts.
    /// </summary>
    /// <param name="fileName">The document file name or path to inspect.</param>
    /// <returns>The bootstrap probe result for the document.</returns>
    public CsxamlBootstrapProbeResult Probe(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new CsxamlBootstrapProbeResult(
                IsCsxamlFile: false,
                DisplayName: "unknown document",
                NormalizedFileName: string.Empty,
                Message: "CSXAML bootstrap is active, but no file name was available.");
        }

        var displayName = Path.GetFileName(fileName);
        var normalizedFileName = fileName.Trim();
        var isCsxamlFile = normalizedFileName.EndsWith(".csxaml", StringComparison.OrdinalIgnoreCase);
        var message = isCsxamlFile
            ? $"CSXAML bootstrap is active for '{displayName}'."
            : $"CSXAML bootstrap is active, but '{displayName}' is not a .csxaml file.";

        return new CsxamlBootstrapProbeResult(
            isCsxamlFile,
            displayName,
            normalizedFileName,
            message);
    }
}
