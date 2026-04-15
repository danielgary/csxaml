using Csxaml.Tooling.Core.Markup;

namespace Csxaml.Tooling.Core.Tests.Markup;

[TestClass]
public sealed class CsxamlUsingDirectiveScannerTests
{
    [TestMethod]
    public void Scan_StaticUsingDirective_PreservesStaticImport()
    {
        var directives = CsxamlUsingDirectiveScanner.Scan(
            """
            using static System.Math;
            using Controls = Microsoft.UI.Xaml.Controls;
            """);

        Assert.HasCount(2, directives);
        Assert.AreEqual("System.Math", directives[0].QualifiedName);
        Assert.IsTrue(directives[0].IsStatic);
        Assert.IsNull(directives[0].Alias);
        Assert.AreEqual("Microsoft.UI.Xaml.Controls", directives[1].QualifiedName);
        Assert.IsFalse(directives[1].IsStatic);
        Assert.AreEqual("Controls", directives[1].Alias);
    }
}
