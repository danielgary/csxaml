using System.Text.Json;
using System.Text.RegularExpressions;

namespace Csxaml.VisualStudio.Tests;

[TestClass]
public sealed partial class PublishManifestTests
{
    [TestMethod]
    public void Publish_manifest_internal_name_is_marketplace_safe()
    {
        var manifestPath = VisualStudioArtifactPaths.GetPublishManifestPath();
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));

        var internalName = manifest.RootElement
            .GetProperty("identity")
            .GetProperty("internalName")
            .GetString();

        Assert.IsNotNull(internalName);
        Assert.IsLessThan(63, internalName.Length);
        Assert.IsTrue(MarketplaceInternalNamePattern().IsMatch(internalName));
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9-]*$")]
    private static partial Regex MarketplaceInternalNamePattern();
}
