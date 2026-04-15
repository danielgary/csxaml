using System.Xml.Linq;

namespace Csxaml.VisualStudio.Tests;

[TestClass]
public sealed class ExtensionEntrypointTests
{
    [TestMethod]
    public void Generated_manifest_declares_supported_dotnet_host_versions()
    {
        var manifestPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "Csxaml.VisualStudio",
                "obj",
                "Debug",
                "net8.0-windows8.0",
                "extension.vsixmanifest"));
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
