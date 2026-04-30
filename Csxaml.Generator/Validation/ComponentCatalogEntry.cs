namespace Csxaml.Generator;

internal sealed record ComponentCatalogEntry(
    Csxaml.ControlMetadata.ComponentMetadata Metadata,
    bool IsLocal,
    ComponentDefinition? LocalDefinition)
{
    public string AssemblyName => Metadata.AssemblyName;

    public string ComponentTypeName => Metadata.ComponentTypeName;

    public string Name => Metadata.Name;

    public string NamespaceName => Metadata.NamespaceName;

    public IReadOnlyList<Csxaml.ControlMetadata.ComponentParameterMetadata> Parameters => Metadata.Parameters;

    public string? PropsTypeName => Metadata.PropsTypeName;

    public bool SupportsDefaultSlot => Metadata.SupportsDefaultSlot;

    public IReadOnlyList<Csxaml.ControlMetadata.ComponentSlotMetadata> NamedSlots => Metadata.NamedSlots;

    public Csxaml.ControlMetadata.ComponentKind Kind => Metadata.Kind;
}
