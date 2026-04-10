namespace Csxaml.ControlMetadata;

public static class ControlMetadataRegistry
{
    private static readonly IReadOnlyDictionary<string, ControlMetadata> ControlsByTagName =
        GeneratedControlMetadata.All.ToDictionary(
            control => control.TagName,
            StringComparer.Ordinal);

    public static IReadOnlyList<ControlMetadata> Controls => GeneratedControlMetadata.All;

    public static ControlMetadata GetControl(string tagName)
    {
        if (!ControlsByTagName.TryGetValue(tagName, out var control))
        {
            throw new InvalidOperationException(
                $"Unsupported control tag '{tagName}'.");
        }

        return control;
    }

    public static bool IsNativeTag(string tagName)
    {
        return ControlsByTagName.ContainsKey(tagName);
    }

    public static bool TryGetControl(string tagName, out ControlMetadata? control)
    {
        return ControlsByTagName.TryGetValue(tagName, out control);
    }
}
