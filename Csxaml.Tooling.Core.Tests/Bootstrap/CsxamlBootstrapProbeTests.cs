using Csxaml.Tooling.Core.Bootstrap;

namespace Csxaml.Tooling.Core.Tests.Bootstrap;

[TestClass]
public sealed class CsxamlBootstrapProbeTests
{
    private readonly CsxamlBootstrapProbe _probe = new();

    [TestMethod]
    public void Probe_recognizes_csxaml_files_case_insensitively()
    {
        var result = _probe.Probe(@"C:\Repo\TodoBoard.CSXAML");

        Assert.IsTrue(result.IsCsxamlFile);
        Assert.AreEqual("TodoBoard.CSXAML", result.DisplayName);
        StringAssert.Contains(result.Message, "CSXAML bootstrap is active");
    }

    [TestMethod]
    public void Probe_reports_non_csxaml_files_clearly()
    {
        var result = _probe.Probe(@"C:\Repo\TodoBoard.xaml");

        Assert.IsFalse(result.IsCsxamlFile);
        StringAssert.Contains(result.Message, "not a .csxaml file");
    }

    [TestMethod]
    public void Probe_handles_missing_file_names()
    {
        var result = _probe.Probe(null);

        Assert.IsFalse(result.IsCsxamlFile);
        Assert.AreEqual("unknown document", result.DisplayName);
        Assert.AreEqual(string.Empty, result.NormalizedFileName);
    }
}
