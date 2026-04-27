namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a compiled CSXAML component that can be referenced by other markup.
/// </summary>
/// <param name="Name">The component tag name exposed to CSXAML markup.</param>
/// <param name="NamespaceName">The CLR namespace that contains the component type.</param>
/// <param name="AssemblyName">The assembly that contains the component type.</param>
/// <param name="ComponentTypeName">The fully qualified CLR type name of the generated component.</param>
/// <param name="PropsTypeName">The fully qualified CLR props type name, or <see langword="null"/> when the component has no props type.</param>
/// <param name="Parameters">The named parameters accepted by the component.</param>
/// <param name="SupportsDefaultSlot">A value indicating whether the component accepts unnamed child content.</param>
public sealed record ComponentMetadata(
    string Name,
    string NamespaceName,
    string AssemblyName,
    string ComponentTypeName,
    string? PropsTypeName,
    IReadOnlyList<ComponentParameterMetadata> Parameters,
    bool SupportsDefaultSlot);
