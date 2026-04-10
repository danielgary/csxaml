namespace Csxaml.ControlMetadata;

public sealed record PropertyMetadata(
    string Name,
    string ClrTypeName,
    bool IsWritable,
    bool IsDependencyProperty,
    bool IsAttached,
    bool ExposedInCsxaml,
    ValueKindHint ValueKindHint);
