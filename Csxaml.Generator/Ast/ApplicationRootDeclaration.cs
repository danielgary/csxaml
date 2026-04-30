namespace Csxaml.Generator;

internal sealed record ApplicationRootDeclaration(
    string StartupTypeName,
    TextSpan StartupSpan,
    string? ResourcesTypeName,
    TextSpan? ResourcesSpan);
