namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes where control child-content metadata came from.
/// </summary>
public enum ControlContentSource
{
    /// <summary>
    /// No specific source is known.
    /// </summary>
    Unknown,

    /// <summary>
    /// The metadata came from curated built-in control metadata.
    /// </summary>
    BuiltInMetadata,

    /// <summary>
    /// The metadata came from Microsoft.UI.Xaml.Markup.ContentPropertyAttribute.
    /// </summary>
    ContentPropertyAttribute,

    /// <summary>
    /// The metadata came from framework or property-name conventions.
    /// </summary>
    Convention
}
