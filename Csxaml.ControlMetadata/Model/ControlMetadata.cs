namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a native control that can be instantiated from CSXAML markup.
/// </summary>
/// <param name="TagName">The control tag name recognized by CSXAML markup.</param>
/// <param name="ClrTypeName">The fully qualified CLR type name created for the control.</param>
/// <param name="BaseTypeName">The fully qualified CLR base type name, or <see langword="null"/> when none is known.</param>
/// <param name="ChildKind">The supported child-content shape for the control.</param>
/// <param name="Content">The default child-content property metadata for the control.</param>
/// <param name="Properties">The settable properties exposed for the control.</param>
/// <param name="Events">The events exposed for the control.</param>
public sealed record ControlMetadata(
    string TagName,
    string ClrTypeName,
    string? BaseTypeName,
    ControlChildKind ChildKind,
    ControlContentMetadata Content,
    IReadOnlyList<PropertyMetadata> Properties,
    IReadOnlyList<EventMetadata> Events)
{
    /// <summary>
    /// Initializes a metadata record using the older child-kind-only model.
    /// </summary>
    public ControlMetadata(
        string TagName,
        string ClrTypeName,
        string? BaseTypeName,
        ControlChildKind ChildKind,
        IReadOnlyList<PropertyMetadata> Properties,
        IReadOnlyList<EventMetadata> Events)
        : this(
            TagName,
            ClrTypeName,
            BaseTypeName,
            ChildKind,
            ControlContentMetadata.FromChildKind(ChildKind),
            Properties,
            Events)
    {
    }
}
