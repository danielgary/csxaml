namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes an attached property that can be assigned from CSXAML markup.
/// </summary>
/// <param name="OwnerName">The simple owner name used in markup, such as <c>Grid</c>.</param>
/// <param name="PropertyName">The property name used after the owner qualifier.</param>
/// <param name="ClrOwnerTypeName">The fully qualified CLR type name that declares the attached property.</param>
/// <param name="ClrTypeName">The fully qualified CLR type name of the property value.</param>
/// <param name="ValueKindHint">A hint used by tooling and emitters when converting literal values.</param>
/// <param name="RequiredParentTagName">The parent control tag required for the property, or <see langword="null"/> when any parent is valid.</param>
/// <param name="DependencyPropertyFieldName">The dependency-property field name when the owner exposes one.</param>
public sealed record AttachedPropertyMetadata(
    string OwnerName,
    string PropertyName,
    string ClrOwnerTypeName,
    string ClrTypeName,
    ValueKindHint ValueKindHint,
    string? RequiredParentTagName,
    string? DependencyPropertyFieldName = null)
{
    /// <summary>
    /// Gets the markup-qualified attached property name, such as <c>Grid.Row</c>.
    /// </summary>
    public string QualifiedName => $"{OwnerName}.{PropertyName}";
}
