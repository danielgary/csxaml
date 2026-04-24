using System.Reflection;
using Csxaml.ControlMetadata;

namespace Csxaml.Generator;

internal sealed class ReferencedComponentCatalogBuilder
{
    public IReadOnlyList<ComponentMetadata> Build(
        IReadOnlyList<string> referencePaths,
        bool ignoreLoadFailures = false)
    {
        if (referencePaths.Count == 0)
        {
            return Array.Empty<ComponentMetadata>();
        }

        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);
        var components = new List<ComponentMetadata>();
        foreach (var assembly in resolver.LoadedAssemblies)
        {
            components.AddRange(ReadManifest(assembly, ignoreLoadFailures).Components);
        }

        return components;
    }

    private static CompiledComponentManifest ReadManifest(Assembly assembly, bool ignoreLoadFailures)
    {
        var attribute = assembly.GetCustomAttribute<CsxamlComponentManifestProviderAttribute>();
        if (attribute is null)
        {
            return new CompiledComponentManifest(Array.Empty<ComponentMetadata>());
        }

        if (!typeof(IComponentManifestProvider).IsAssignableFrom(attribute.ProviderType))
        {
            throw new InvalidOperationException(
                $"Referenced assembly '{assembly.GetName().Name}' declares an invalid CSXAML component manifest provider.");
        }

        try
        {
            if (Activator.CreateInstance(attribute.ProviderType, nonPublic: true) is not IComponentManifestProvider provider)
            {
                throw new InvalidOperationException(
                    $"Referenced assembly '{assembly.GetName().Name}' failed to create its CSXAML component manifest provider.");
            }

            return provider.GetManifest();
        }
        catch (Exception) when (ignoreLoadFailures)
        {
            return new CompiledComponentManifest(Array.Empty<ComponentMetadata>());
        }
    }
}
