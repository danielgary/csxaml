namespace Csxaml.Generator;

internal static class ComponentCatalogBuilder
{
    public static ComponentCatalog Build(
        IReadOnlyList<ParsedComponent> components,
        ProjectGenerationContext project,
        IReadOnlyList<Csxaml.ControlMetadata.ComponentMetadata> referencedComponents)
    {
        var entries = new List<ComponentCatalogEntry>();
        var seenLocalNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            var metadata = CreateLocalMetadata(component, project);
            if (!seenLocalNames.Add($"{metadata.NamespaceName}.{metadata.Name}"))
            {
                throw DiagnosticFactory.FromSpan(
                    component.Source,
                    component.Definition.Span,
                    $"component '{metadata.NamespaceName}.{component.Definition.Name}' is defined more than once");
            }

            entries.Add(new ComponentCatalogEntry(metadata, IsLocal: true, LocalDefinition: component.Definition));
        }

        entries.AddRange(
            referencedComponents.Select(
                metadata => new ComponentCatalogEntry(metadata, IsLocal: false, LocalDefinition: null)));
        return new ComponentCatalog(entries);
    }

    private static Csxaml.ControlMetadata.ComponentMetadata CreateLocalMetadata(
        ParsedComponent component,
        ProjectGenerationContext project)
    {
        var namespaceName = component.File.Namespace?.NamespaceName ?? project.DefaultComponentNamespace;
        var definition = component.Definition;
        return new Csxaml.ControlMetadata.ComponentMetadata(
            definition.Name,
            namespaceName,
            project.AssemblyName,
            GetComponentTypeName(namespaceName, definition),
            definition.Parameters.Count == 0 ? null : $"{namespaceName}.{definition.Name}Props",
            definition.Parameters
                .Select(
                    parameter => new Csxaml.ControlMetadata.ComponentParameterMetadata(
                        parameter.Name,
                        parameter.TypeName))
                .ToList(),
            definition.SupportsDefaultSlot,
            definition.NamedSlots
                .Select(name => new Csxaml.ControlMetadata.ComponentSlotMetadata(name))
                .ToList(),
            ToMetadataKind(definition.Kind));
    }

    private static string GetComponentTypeName(string namespaceName, ComponentDefinition definition)
    {
        return definition.Kind is ComponentKind.Application or ComponentKind.ResourceDictionary
            ? $"{namespaceName}.{definition.Name}"
            : $"{namespaceName}.{definition.Name}Component";
    }

    private static Csxaml.ControlMetadata.ComponentKind ToMetadataKind(ComponentKind kind)
    {
        return kind switch
        {
            ComponentKind.Element => Csxaml.ControlMetadata.ComponentKind.Element,
            ComponentKind.Page => Csxaml.ControlMetadata.ComponentKind.Page,
            ComponentKind.Window => Csxaml.ControlMetadata.ComponentKind.Window,
            ComponentKind.Application => Csxaml.ControlMetadata.ComponentKind.Application,
            ComponentKind.ResourceDictionary => Csxaml.ControlMetadata.ComponentKind.ResourceDictionary,
            _ => throw new InvalidOperationException($"Unsupported component kind '{kind}'.")
        };
    }
}
