namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class RootKindParserTests
{
    [TestMethod]
    public void Parse_PageRoot_RecordsComponentKind()
    {
        var component = GeneratorTestHarness.Parse(
            "HomePage.csxaml",
            """
            component Page HomePage {
                render <Grid />;
            }
            """).Definition;

        Assert.AreEqual(ComponentKind.Page, component.Kind);
        Assert.AreEqual("HomePage", component.Name);
    }

    [TestMethod]
    public void Parse_WindowRoot_RecordsRootProperties()
    {
        var component = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                Title = "CSXAML Starter";
                Width = 960;
                Height = 640;

                render <HomePage />;
            }
            """).Definition;

        Assert.AreEqual(ComponentKind.Window, component.Kind);
        Assert.HasCount(3, component.RootProperties);
        Assert.AreEqual("Title", component.RootProperties[0].Name);
        Assert.AreEqual("\"CSXAML Starter\"", component.RootProperties[0].ValueExpression);
        Assert.AreEqual("Width", component.RootProperties[1].Name);
        Assert.AreEqual("960", component.RootProperties[1].ValueExpression);
    }

    [TestMethod]
    public void Parse_ApplicationRoot_RecordsStartupAndResources()
    {
        var component = GeneratorTestHarness.Parse(
            "App.csxaml",
            """
            component Application App {
                startup MainWindow;
                resources AppResources;
            }
            """).Definition;

        Assert.AreEqual(ComponentKind.Application, component.Kind);
        Assert.IsNotNull(component.Application);
        Assert.AreEqual("MainWindow", component.Application.StartupTypeName);
        Assert.AreEqual("AppResources", component.Application.ResourcesTypeName);
    }

    [TestMethod]
    public void Parse_ResourceDictionaryRoot_RecordsComponentKind()
    {
        var component = GeneratorTestHarness.Parse(
            "AppResources.csxaml",
            """
            component ResourceDictionary AppResources {
                render <ResourceDictionary />;
            }
            """).Definition;

        Assert.AreEqual(ComponentKind.ResourceDictionary, component.Kind);
    }
}
