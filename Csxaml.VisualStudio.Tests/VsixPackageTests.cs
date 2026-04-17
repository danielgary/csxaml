using System.IO.Compression;

namespace Csxaml.VisualStudio.Tests;

[TestClass]
public sealed class VsixPackageTests
{
    [TestMethod]
    public void Packaged_vsix_contains_language_server_payload()
    {
        var vsixPath = VisualStudioArtifactPaths.GetVsixPath();

        using var archive = ZipFile.OpenRead(vsixPath);
        var entries = archive.Entries
            .Select(entry => entry.FullName)
            .ToList();

        CollectionAssert.Contains(entries, "LanguageServer/Csxaml.LanguageServer.exe");
        CollectionAssert.Contains(entries, "LanguageServer/Csxaml.LanguageServer.runtimeconfig.json");
        CollectionAssert.Contains(entries, "LanguageServer/Csxaml.Tooling.Core.dll");
    }
}
