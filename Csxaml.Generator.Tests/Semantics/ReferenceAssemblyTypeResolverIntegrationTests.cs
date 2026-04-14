using System.Reflection;

namespace Csxaml.Generator.Tests.Semantics;

[TestClass]
public sealed class ReferenceAssemblyTypeResolverIntegrationTests
{
    [TestMethod]
    public void FindTypes_LocalExternalControlReference_IsDiscoverable()
    {
        var referencesFile = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "Csxaml.Runtime.Tests",
                "obj",
                "DemoCsxamlReferences.txt"));
        if (!File.Exists(referencesFile))
        {
            Assert.Inconclusive($"Reference list not found: {referencesFile}");
        }

        var referencePaths = File.ReadAllLines(referencesFile)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToList();
        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);

        var matches = resolver.FindTypes("Csxaml.ExternalControls.StatusButton");
        if (matches.Count == 0)
        {
            var loadedAssemblies = GetLoadedAssemblyNames(resolver);
            var loadFailures = GetLoadFailures(resolver);
            Assert.Fail(
                "StatusButton was not discovered. Loaded assemblies: " +
                string.Join(", ", loadedAssemblies) +
                ". Load failures: " +
                string.Join(" | ", loadFailures));
        }

        Assert.HasCount(1, matches);
        Assert.AreEqual("Csxaml.ExternalControls.StatusButton", matches[0].FullName);
    }

    private static IReadOnlyList<string> GetLoadedAssemblyNames(ReferenceAssemblyTypeResolver resolver)
    {
        var field = typeof(ReferenceAssemblyTypeResolver).GetField(
            "_loadedAssemblies",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var assemblies = (IReadOnlyList<Assembly>?)field?.GetValue(resolver) ?? Array.Empty<Assembly>();
        return assemblies
            .Select(assembly => assembly.GetName().Name ?? assembly.FullName ?? "<unknown>")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> GetLoadFailures(ReferenceAssemblyTypeResolver resolver)
    {
        var field = typeof(ReferenceAssemblyTypeResolver).GetField(
            "_loadFailures",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var failures = (IReadOnlyDictionary<string, string>?)field?.GetValue(resolver) ??
            new Dictionary<string, string>(StringComparer.Ordinal);
        return failures
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => $"{entry.Key}: {entry.Value}")
            .ToList();
    }
}
