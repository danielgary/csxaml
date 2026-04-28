namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a parameter exposed by a compiled CSXAML component.
/// </summary>
/// <param name="Name">The parameter name used by markup and generated props objects.</param>
/// <param name="TypeName">The fully qualified CLR type name expected for the parameter value.</param>
public sealed record ComponentParameterMetadata(
    string Name,
    string TypeName);
