using System.Reflection;
using System.Text;
using Csxaml.ControlMetadata;
using Csxaml.Generator;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

internal sealed class CsxamlExternalControlCache
{
    private readonly ExternalControlMetadataBuilder _externalControlMetadataBuilder;
    private IReadOnlyList<ControlMetadataModel> _cachedControls = Array.Empty<ControlMetadataModel>();
    private string? _cachedSignature;

    public CsxamlExternalControlCache(ExternalControlMetadataBuilder externalControlMetadataBuilder)
    {
        _externalControlMetadataBuilder = externalControlMetadataBuilder;
    }

    public IReadOnlyList<ControlMetadataModel> Load(IReadOnlyList<string> referencePaths)
    {
        var signature = CreateSignature(referencePaths);
        if (string.Equals(signature, _cachedSignature, StringComparison.Ordinal))
        {
            return _cachedControls;
        }

        _cachedControls = LoadControls(referencePaths);
        _cachedSignature = signature;
        return _cachedControls;
    }

    private IReadOnlyList<ControlMetadataModel> LoadControls(IReadOnlyList<string> referencePaths)
    {
        var controls = new List<ControlMetadataModel>();
        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);
        foreach (var assembly in resolver.LoadedAssemblies)
        {
            foreach (var type in GetLoadableTypes(assembly))
            {
                if (!TryBuildExternalControl(type, out var metadata))
                {
                    continue;
                }

                controls.Add(metadata);
            }
        }

        return controls
            .DistinctBy(control => control.ClrTypeName, StringComparer.Ordinal)
            .OrderBy(control => control.ClrTypeName, StringComparer.Ordinal)
            .ToList();
    }

    private bool TryBuildExternalControl(Type type, out ControlMetadataModel metadata)
    {
        try
        {
            if (type.FullName is null || type.Namespace is null)
            {
                metadata = null!;
                return false;
            }

            if (ControlMetadataRegistry.Controls.Any(control => control.ClrTypeName == type.FullName))
            {
                metadata = null!;
                return false;
            }

            if (_externalControlMetadataBuilder.TryBuild(type, out var builtMetadata, out _))
            {
                metadata = builtMetadata!;
                return true;
            }
        }
        catch (Exception)
        {
            // Tooling should degrade gracefully when a referenced assembly cannot fully load in the editor host.
        }

        metadata = null!;
        return false;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(type => type is not null)!;
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static string CreateSignature(IReadOnlyList<string> referencePaths)
    {
        var builder = new StringBuilder();
        foreach (var path in referencePaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var stamp = CsxamlFileStamp.Read(path);
            builder
                .Append(path)
                .Append('|')
                .Append(stamp.Length)
                .Append('|')
                .Append(stamp.LastWriteTimeUtcTicks)
                .AppendLine();
        }

        return builder.ToString();
    }
}
