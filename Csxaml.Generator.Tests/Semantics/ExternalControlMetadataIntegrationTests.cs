namespace Csxaml.Generator.Tests.Semantics;

[TestClass]
public sealed class ExternalControlMetadataIntegrationTests
{
    [TestMethod]
    public void TryBuild_InfoBarMetadata_IncludesRepresentativePackageProperties()
    {
        var referencePaths = ReadDemoReferences();
        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);
        var matches = resolver.FindTypes("Microsoft.UI.Xaml.Controls.InfoBar");
        Assert.HasCount(1, matches);

        var builder = new ExternalControlMetadataBuilder();
        var success = builder.TryBuild(matches[0], out var metadata, out var reason);

        Assert.IsTrue(success, reason);
        Assert.IsNotNull(metadata);
        var propertyNames = metadata.Properties.Select(property => property.Name).ToList();
        var description = string.Join(", ", metadata.Properties.Select(
            property => $"{property.Name}:{property.ClrTypeName}:{property.IsDependencyProperty}"));
        CollectionAssert.Contains(propertyNames, "IsOpen", description);
        CollectionAssert.Contains(propertyNames, "Severity", description);
        CollectionAssert.Contains(propertyNames, "Title", description);
        CollectionAssert.Contains(propertyNames, "Message", description);
        CollectionAssert.Contains(propertyNames, "Style", description);
        Assert.AreEqual(
            ValueKindHint.Style,
            metadata.Properties.Single(property => property.Name == "Style").ValueKindHint);
    }

    private static IReadOnlyList<string> ReadDemoReferences()
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

        return File.ReadAllLines(referencesFile)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToList();
    }
}
