namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a native control property that can be inspected or assigned by CSXAML tooling.
/// </summary>
/// <param name="Name">The property name exposed by the native control.</param>
/// <param name="ClrTypeName">The fully qualified CLR type name of the property value.</param>
/// <param name="IsWritable">A value indicating whether generated code can assign the property.</param>
/// <param name="IsDependencyProperty">A value indicating whether the property is backed by a dependency property.</param>
/// <param name="IsAttached">A value indicating whether the property is an attached property.</param>
/// <param name="ExposedInCsxaml">A value indicating whether the property is intended to be set from CSXAML.</param>
/// <param name="ValueKindHint">A hint used by tooling and emitters when converting literal values.</param>
public sealed record PropertyMetadata(
    string Name,
    string ClrTypeName,
    bool IsWritable,
    bool IsDependencyProperty,
    bool IsAttached,
    bool ExposedInCsxaml,
    ValueKindHint ValueKindHint);
