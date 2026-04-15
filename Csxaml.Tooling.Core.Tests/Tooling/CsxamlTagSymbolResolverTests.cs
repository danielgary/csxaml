using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlTagSymbolResolverTests
{
    [TestMethod]
    public void Resolve_WhenBuiltInAndComponentShareSimpleName_ReturnsAmbiguous()
    {
        var workspace = new CsxamlWorkspaceSnapshot(
            new CsxamlProjectInfo(
                "Test.csproj",
                "C:\\repo",
                "TestAssembly",
                "TestNamespace",
                "net10.0"),
            [
                new CsxamlWorkspaceComponentSymbol(
                    new ComponentMetadata(
                        "Button",
                        "TestNamespace",
                        "TestAssembly",
                        "TestNamespace.ButtonComponent",
                        null,
                        Array.Empty<ComponentParameterMetadata>(),
                        false),
                    "C:\\repo\\Button.csxaml",
                    0,
                    "Button".Length)
            ],
            Array.Empty<Csxaml.ControlMetadata.ControlMetadata>());

        var resolvedTag = new CsxamlTagSymbolResolver().Resolve(
            "Button",
            Array.Empty<CsxamlUsingDirectiveInfo>(),
            "TestNamespace",
            workspace);

        Assert.AreEqual(CsxamlResolvedTagKind.Ambiguous, resolvedTag.Kind);
    }

    [TestMethod]
    public void Resolve_StaticUsingDirective_DoesNotParticipateInTagLookup()
    {
        var workspace = new CsxamlWorkspaceSnapshot(
            new CsxamlProjectInfo(
                "Test.csproj",
                "C:\\repo",
                "TestAssembly",
                "TestNamespace",
                "net10.0"),
            Array.Empty<CsxamlWorkspaceComponentSymbol>(),
            Array.Empty<Csxaml.ControlMetadata.ControlMetadata>());

        var resolvedTag = new CsxamlTagSymbolResolver().Resolve(
            "Math",
            [
                new CsxamlUsingDirectiveInfo("System.Math", null, true, 0, 24)
            ],
            "TestNamespace",
            workspace);

        Assert.AreEqual(CsxamlResolvedTagKind.None, resolvedTag.Kind);
    }
}
