namespace Csxaml.Runtime;

/// <summary>
/// Represents a native control element in the CSXAML runtime tree.
/// </summary>
/// <param name="TagName">The native control tag name to render.</param>
/// <param name="Key">The optional explicit reconciliation key.</param>
/// <param name="Properties">The property values assigned to the element.</param>
/// <param name="AttachedProperties">The attached property values assigned to the element.</param>
/// <param name="Events">The event handlers assigned to the element.</param>
/// <param name="Children">The child nodes rendered inside the element.</param>
/// <param name="SourceInfo">Source-location metadata for the element, when available.</param>
public sealed record NativeElementNode(
    string TagName,
    string? Key,
    IReadOnlyList<NativePropertyValue> Properties,
    IReadOnlyList<NativeAttachedPropertyValue> AttachedProperties,
    IReadOnlyList<NativeEventValue> Events,
    IReadOnlyList<Node> Children,
    CsxamlSourceInfo? SourceInfo = null) : NativeNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativeElementNode"/> class without attached properties.
    /// </summary>
    /// <param name="tagName">The native control tag name to render.</param>
    /// <param name="key">The optional explicit reconciliation key.</param>
    /// <param name="properties">The property values assigned to the element.</param>
    /// <param name="events">The event handlers assigned to the element.</param>
    /// <param name="children">The child nodes rendered inside the element.</param>
    public NativeElementNode(
        string tagName,
        string? key,
        IReadOnlyList<NativePropertyValue> properties,
        IReadOnlyList<NativeEventValue> events,
        IReadOnlyList<Node> children)
        : this(tagName, key, properties, Array.Empty<NativeAttachedPropertyValue>(), events, children, null)
    {
    }
}
