namespace Csxaml.Runtime;

public sealed record NativeElementNode(
    string TagName,
    string? Key,
    IReadOnlyList<NativePropertyValue> Properties,
    IReadOnlyList<NativeAttachedPropertyValue> AttachedProperties,
    IReadOnlyList<NativeEventValue> Events,
    IReadOnlyList<Node> Children) : NativeNode
{
    public NativeElementNode(
        string tagName,
        string? key,
        IReadOnlyList<NativePropertyValue> properties,
        IReadOnlyList<NativeEventValue> events,
        IReadOnlyList<Node> children)
        : this(tagName, key, properties, Array.Empty<NativeAttachedPropertyValue>(), events, children)
    {
    }
}
