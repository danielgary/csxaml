namespace Csxaml.Generator;

internal sealed record NativePropertyContentTarget(
    string Name,
    ControlContentKind Kind,
    string? PropertyTypeName);
