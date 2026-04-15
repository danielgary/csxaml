namespace Csxaml.Tooling.Core.Bootstrap;

public sealed class CsxamlBootstrapProbe
{
    public const string ContentTypeName = "csxaml";

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
