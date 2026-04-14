namespace Csxaml.Generator;

internal static class ReferenceListFileReader
{
    public static IReadOnlyList<string> Read(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return Array.Empty<string>();
        }

        return File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(line => line, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
