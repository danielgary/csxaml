namespace Csxaml.Runtime;

public sealed record ComponentNode(
    Type ComponentType,
    object? Props,
    IReadOnlyList<Node> ChildContent,
    IReadOnlyList<NativeAttachedPropertyValue> AttachedProperties,
    string RenderPositionId,
    string? Key,
    CsxamlSourceInfo? SourceInfo = null) : Node
{
    public ComponentNode(
        Type componentType,
        object? props,
        string renderPositionId,
        string? key)
        : this(
            componentType,
            props,
            Array.Empty<Node>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            renderPositionId,
            key,
            null)
    {
    }

    public ComponentNode(
        Type componentType,
        object? props,
        IReadOnlyList<Node> childContent,
        string renderPositionId,
        string? key)
        : this(
            componentType,
            props,
            childContent,
            Array.Empty<NativeAttachedPropertyValue>(),
            renderPositionId,
            key,
            null)
    {
    }

    public ComponentNode(
        Type componentType,
        object? props,
        IReadOnlyList<NativeAttachedPropertyValue> attachedProperties,
        string renderPositionId,
        string? key)
        : this(
            componentType,
            props,
            Array.Empty<Node>(),
            attachedProperties,
            renderPositionId,
            key,
            null)
    {
    }
}
