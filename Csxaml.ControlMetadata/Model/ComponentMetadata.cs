namespace Csxaml.ControlMetadata;

public sealed record ComponentMetadata(
    string Name,
    string NamespaceName,
    string AssemblyName,
    string ComponentTypeName,
    string? PropsTypeName,
    IReadOnlyList<ComponentParameterMetadata> Parameters,
    bool SupportsDefaultSlot);
