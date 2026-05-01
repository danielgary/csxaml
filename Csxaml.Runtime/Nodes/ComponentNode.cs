namespace Csxaml.Runtime;

/// <summary>
/// Represents a child component requested by a component render method.
/// </summary>
/// <param name="ComponentType">The component type to instantiate or reuse.</param>
/// <param name="Props">The props object supplied to the component.</param>
/// <param name="ChildContent">The child content passed into the component.</param>
/// <param name="NamedSlotContent">The named slot content passed into the component.</param>
/// <param name="AttachedProperties">Attached properties applied to the component usage.</param>
/// <param name="RenderPositionId">The stable generated position identifier for unkeyed reconciliation.</param>
/// <param name="Key">The optional explicit reconciliation key.</param>
/// <param name="SourceInfo">Source-location metadata for the component usage, when available.</param>
public sealed record ComponentNode(
    Type ComponentType,
    object? Props,
    IReadOnlyList<Node> ChildContent,
    IReadOnlyDictionary<string, IReadOnlyList<Node>> NamedSlotContent,
    IReadOnlyList<NativeAttachedPropertyValue> AttachedProperties,
    string RenderPositionId,
    string? Key,
    CsxamlSourceInfo? SourceInfo = null) : Node
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNode"/> class without child content.
    /// </summary>
    /// <param name="componentType">The component type to instantiate or reuse.</param>
    /// <param name="props">The props object supplied to the component.</param>
    /// <param name="renderPositionId">The stable generated position identifier for unkeyed reconciliation.</param>
    /// <param name="key">The optional explicit reconciliation key.</param>
    public ComponentNode(
        Type componentType,
        object? props,
        string renderPositionId,
        string? key)
        : this(
            componentType,
            props,
            Array.Empty<Node>(),
            EmptyNamedSlots(),
            Array.Empty<NativeAttachedPropertyValue>(),
            renderPositionId,
            key,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNode"/> class with child content.
    /// </summary>
    /// <param name="componentType">The component type to instantiate or reuse.</param>
    /// <param name="props">The props object supplied to the component.</param>
    /// <param name="childContent">The child content passed into the component.</param>
    /// <param name="attachedProperties">Attached properties applied to the component usage.</param>
    /// <param name="renderPositionId">The stable generated position identifier for unkeyed reconciliation.</param>
    /// <param name="key">The optional explicit reconciliation key.</param>
    /// <param name="sourceInfo">Source-location metadata for the component usage, when available.</param>
    public ComponentNode(
        Type componentType,
        object? props,
        IReadOnlyList<Node> childContent,
        IReadOnlyList<NativeAttachedPropertyValue> attachedProperties,
        string renderPositionId,
        string? key,
        CsxamlSourceInfo? sourceInfo = null)
        : this(
            componentType,
            props,
            childContent,
            EmptyNamedSlots(),
            attachedProperties,
            renderPositionId,
            key,
            sourceInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNode"/> class with child content.
    /// </summary>
    /// <param name="componentType">The component type to instantiate or reuse.</param>
    /// <param name="props">The props object supplied to the component.</param>
    /// <param name="childContent">The child content passed into the component.</param>
    /// <param name="renderPositionId">The stable generated position identifier for unkeyed reconciliation.</param>
    /// <param name="key">The optional explicit reconciliation key.</param>
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
            EmptyNamedSlots(),
            Array.Empty<NativeAttachedPropertyValue>(),
            renderPositionId,
            key,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNode"/> class with attached properties.
    /// </summary>
    /// <param name="componentType">The component type to instantiate or reuse.</param>
    /// <param name="props">The props object supplied to the component.</param>
    /// <param name="attachedProperties">Attached properties applied to the component usage.</param>
    /// <param name="renderPositionId">The stable generated position identifier for unkeyed reconciliation.</param>
    /// <param name="key">The optional explicit reconciliation key.</param>
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
            EmptyNamedSlots(),
            attachedProperties,
            renderPositionId,
            key,
            null)
    {
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<Node>> EmptyNamedSlots()
    {
        return new Dictionary<string, IReadOnlyList<Node>>(StringComparer.Ordinal);
    }
}
