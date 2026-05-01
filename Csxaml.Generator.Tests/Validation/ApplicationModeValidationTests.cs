namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class ApplicationModeValidationTests
{
    [TestMethod]
    public void Validate_GeneratedMode_AllowsApplicationWindowAndResources()
    {
        var app = ParseApp();
        var window = ParseWindow();
        var resources = ParseResources();
        var project = CreateGeneratedProject();

        new Validator().Validate([app, window, resources], project);
    }

    [TestMethod]
    public void Validate_HybridMode_RejectsApplicationRoot()
    {
        var app = ParseApp();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(app));

        StringAssert.Contains(exception.Message, "CsxamlApplicationMode=Generated");
    }

    [TestMethod]
    public void Validate_GeneratedMode_RequiresApplicationRoot()
    {
        var window = ParseWindow();
        var project = CreateGeneratedProject();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => new Validator().Validate([window], project));

        StringAssert.Contains(exception.Message, "requires exactly one component Application");
    }

    [TestMethod]
    public void Validate_GeneratedMode_RejectsMissingStartupWindow()
    {
        var app = GeneratorTestHarness.Parse(
            "App.csxaml",
            """
            component Application App {
                startup MissingWindow;
            }
            """);
        var project = CreateGeneratedProject();

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => new Validator().Validate([app], project));

        StringAssert.Contains(exception.Message, "startup target 'MissingWindow'");
    }

    private static ProjectGenerationContext CreateGeneratedProject()
    {
        return new ProjectGenerationContext(
            "TestProject",
            "TestProject",
            "TestProject.__CsxamlGenerated",
            CsxamlApplicationMode.Generated);
    }

    private static ParsedComponent ParseApp()
    {
        return GeneratorTestHarness.Parse(
            "App.csxaml",
            """
            component Application App {
                startup MainWindow;
                resources AppResources;
            }
            """);
    }

    private static ParsedComponent ParseResources()
    {
        return GeneratorTestHarness.Parse(
            "AppResources.csxaml",
            """
            component ResourceDictionary AppResources {
                render <ResourceDictionary />;
            }
            """);
    }

    private static ParsedComponent ParseWindow()
    {
        return GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                render <Grid />;
            }
            """);
    }
}
