using System.Reflection;

namespace Csxaml.Generator;

internal sealed class ReferenceAssemblyTypeResolver : IDisposable
{
    private readonly Dictionary<string, IReadOnlyList<Type>> _cache = new(StringComparer.Ordinal);
    private readonly ReferenceAssemblyLoadContext _loadContext;
    private readonly IReadOnlyList<Assembly> _loadedAssemblies;
    private readonly IReadOnlyDictionary<string, string> _loadFailures;

    public ReferenceAssemblyTypeResolver(IReadOnlyList<string> referencePaths)
    {
        var platformAssemblyNames = GetPlatformAssemblyNames();
        var referenceAssemblyPaths = CreateUniqueAssemblyPaths(referencePaths);
        var resolverPaths = CreateResolverPaths(referenceAssemblyPaths);
        _loadContext = new ReferenceAssemblyLoadContext(resolverPaths);

        var loadedAssemblies = new List<Assembly>();
        var loadFailures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in referenceAssemblyPaths)
        {
            if (platformAssemblyNames.Contains(GetAssemblyNameKey(path)))
            {
                continue;
            }

            if (TryLoadAssembly(path, out var assembly, out var failure))
            {
                loadedAssemblies.Add(assembly!);
                continue;
            }

            loadFailures[path] = failure ?? "unknown load failure";
        }

        _loadedAssemblies = loadedAssemblies;
        _loadFailures = loadFailures;
    }

    public IReadOnlyList<Type> FindTypes(string fullName)
    {
        if (_cache.TryGetValue(fullName, out var cached))
        {
            return cached;
        }

        var matches = _loadedAssemblies
            .Select(assembly => assembly.GetType(fullName, throwOnError: false, ignoreCase: false))
            .Where(type => type is not null)
            .Cast<Type>()
            .GroupBy(type => type.AssemblyQualifiedName, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        _cache[fullName] = matches;
        return matches;
    }

    public IReadOnlyList<Assembly> LoadedAssemblies => _loadedAssemblies;

    public IReadOnlyDictionary<string, string> LoadFailures => _loadFailures;

    public void Dispose()
    {
        _loadContext.Unload();
    }

    private bool TryLoadAssembly(string path, out Assembly? assembly, out string? failure)
    {
        try
        {
            assembly = _loadContext.LoadFromAssemblyPath(path);
            failure = null;
            return true;
        }
        catch (Exception exception)
        {
            assembly = null;
            failure = exception.Message;
            return false;
        }
    }

    private static IReadOnlyDictionary<string, string> CreateResolverPaths(
        IReadOnlyList<string> referenceAssemblyPaths)
    {
        var pathsByAssemblyName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in GetTrustedPlatformAssemblyPaths())
        {
            TryAddAssemblyPath(pathsByAssemblyName, path);
        }

        foreach (var path in referenceAssemblyPaths)
        {
            TryAddAssemblyPath(pathsByAssemblyName, path);
        }

        return pathsByAssemblyName;
    }

    private static IReadOnlyList<string> CreateUniqueAssemblyPaths(IEnumerable<string> paths)
    {
        var pathsByAssemblyName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths.Where(File.Exists).Select(Path.GetFullPath))
        {
            AddOrReplaceAssemblyPath(pathsByAssemblyName, path);
        }

        return pathsByAssemblyName.Values.ToList();
    }

    private static HashSet<string> GetPlatformAssemblyNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in GetTrustedPlatformAssemblyPaths())
        {
            if (TryGetManagedAssemblyName(path, out var assemblyName))
            {
                names.Add(assemblyName!);
            }
        }

        return names;
    }

    private static IEnumerable<string> GetTrustedPlatformAssemblyPaths()
    {
        return (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)?
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries) ??
            Array.Empty<string>();
    }

    private static void AddOrReplaceAssemblyPath(
        IDictionary<string, string> pathsByAssemblyName,
        string path)
    {
        if (!TryGetManagedAssemblyName(path, out var assemblyName))
        {
            return;
        }

        pathsByAssemblyName[assemblyName!] = path;
    }

    private static void TryAddAssemblyPath(
        IDictionary<string, string> pathsByAssemblyName,
        string path)
    {
        if (!TryGetManagedAssemblyName(path, out var assemblyName) ||
            pathsByAssemblyName.ContainsKey(assemblyName!))
        {
            return;
        }

        pathsByAssemblyName[assemblyName!] = path;
    }

    private static string GetAssemblyNameKey(string path)
    {
        return TryGetManagedAssemblyName(path, out var assemblyName)
            ? assemblyName!
            : Path.GetFileNameWithoutExtension(path);
    }

    private static bool TryGetManagedAssemblyName(string path, out string? assemblyName)
    {
        try
        {
            assemblyName = AssemblyName.GetAssemblyName(path).Name ??
                Path.GetFileNameWithoutExtension(path);
            return true;
        }
        catch
        {
            assemblyName = null;
            return false;
        }
    }
}
