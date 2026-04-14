using System.Reflection;
using System.Runtime.Loader;

namespace Csxaml.Generator;

internal sealed class ReferenceAssemblyLoadContext : AssemblyLoadContext
{
    private readonly IReadOnlyDictionary<string, string> _pathsByAssemblyName;

    public ReferenceAssemblyLoadContext(IReadOnlyDictionary<string, string> pathsByAssemblyName)
        : base(isCollectible: true)
    {
        _pathsByAssemblyName = pathsByAssemblyName;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var sharedAssembly = TryLoadSharedAssembly(assemblyName);
        if (sharedAssembly is not null)
        {
            return sharedAssembly;
        }

        if (_pathsByAssemblyName.TryGetValue(assemblyName.Name ?? string.Empty, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }

    private static Assembly? TryLoadSharedAssembly(AssemblyName assemblyName)
    {
        if (!string.Equals(
                assemblyName.Name,
                "Csxaml.ControlMetadata",
                StringComparison.Ordinal))
        {
            return null;
        }

        return AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
            candidate =>
                string.Equals(
                    candidate.GetName().Name,
                    assemblyName.Name,
                    StringComparison.OrdinalIgnoreCase));
    }
}
