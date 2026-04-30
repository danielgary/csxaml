namespace Csxaml.Generator.Tests.Validation;

[TestClass]
public sealed class RootKindValidationTests
{
    [TestMethod]
    public void Validate_PageAndWindowRoots_AllowsWindowToRenderPageBody()
    {
        var page = GeneratorTestHarness.Parse(
            "HomePage.csxaml",
            """
            component Page HomePage {
                render <Grid />;
            }
            """);
        var window = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                Title = "CSXAML Starter";
                Width = 960;
                Height = 640;
                Backdrop = "Mica";

                render <HomePage />;
            }
            """);

        GeneratorTestHarness.Validate(page, window);
    }

    [TestMethod]
    public void Validate_WindowRoot_RejectsParameters()
    {
        var component = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow(string Title) {
                render <Grid />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(exception.Message, "does not support component parameters");
    }

    [TestMethod]
    public void Validate_PageRoot_RejectsSlots()
    {
        var component = GeneratorTestHarness.Parse(
            "HomePage.csxaml",
            """
            component Page HomePage {
                render <Grid>
                    <Slot />
                </Grid>;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(exception.Message, "does not support slots");
    }

    [TestMethod]
    public void Validate_WindowRoot_RejectsSingleSizeProperty()
    {
        var component = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                Width = 960;

                render <Grid />;
            }
            """);

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Validate(component));

        StringAssert.Contains(exception.Message, "Width");
        StringAssert.Contains(exception.Message, "Height");
    }
}
