namespace Csxaml.ControlMetadata;

/// <summary>
/// Provides lookup access to the generated native-control metadata table.
/// </summary>
public static class ControlMetadataRegistry
{
    private static readonly IReadOnlyDictionary<string, ControlMetadata> ControlsByTagName =
        GeneratedControlMetadata.All.ToDictionary(
            control => control.TagName,
            StringComparer.Ordinal);

    /// <summary>
    /// Gets all native controls known to the current metadata table.
    /// </summary>
    public static IReadOnlyList<ControlMetadata> Controls => GeneratedControlMetadata.All;

    /// <summary>
    /// Gets metadata for the native control with the specified tag name.
    /// </summary>
    /// <param name="tagName">The CSXAML tag name to resolve.</param>
    /// <returns>The matching control metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the tag name is not known.</exception>
    public static ControlMetadata GetControl(string tagName)
    {
        if (!ControlsByTagName.TryGetValue(tagName, out var control))
        {
            throw new InvalidOperationException(
                $"Unsupported control tag '{tagName}'.");
        }

        return control;
    }

    /// <summary>
    /// Determines whether the specified tag name maps to a known native control.
    /// </summary>
    /// <param name="tagName">The CSXAML tag name to inspect.</param>
    /// <returns><see langword="true"/> when the tag is a native control; otherwise, <see langword="false"/>.</returns>
    public static bool IsNativeTag(string tagName)
    {
        return ControlsByTagName.ContainsKey(tagName);
    }

    /// <summary>
    /// Attempts to get metadata for the native control with the specified tag name.
    /// </summary>
    /// <param name="tagName">The CSXAML tag name to resolve.</param>
    /// <param name="control">The matching control metadata when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the tag is known; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetControl(string tagName, out ControlMetadata? control)
    {
        return ControlsByTagName.TryGetValue(tagName, out control);
    }
}
