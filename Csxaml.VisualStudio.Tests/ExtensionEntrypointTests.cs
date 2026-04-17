using System.Xml.Linq;

namespace Csxaml.VisualStudio.Tests;

[TestClass]
public sealed class ExtensionEntrypointTests
{
    [TestMethod]
    public void Generated_manifest_declares_supported_dotnet_host_versions()
    {
        var manifestPath = VisualStudioArtifactPaths.GetManifestPath();
        var document = XDocument.Load(manifestPath);
        XNamespace manifestNamespace = "http://schemas.microsoft.com/developer/vsx-schema/2011";

        Assert.AreEqual(
            "net8.0;net10.0",
            document.Root?
                .Element(manifestNamespace + "Installation")?
                .Element(manifestNamespace + "DotnetTargetVersions")?
                .Value);
    }
}
