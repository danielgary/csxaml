namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class RootKindEmissionTests
{
    [TestMethod]
    public void Emit_PageRoot_GeneratesPageShellAndBodyComponent()
    {
        var component = GeneratorTestHarness.Parse(
            "HomePage.csxaml",
            """
            component Page HomePage {
                render <Grid />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "public sealed partial class HomePage : global::Microsoft.UI.Xaml.Controls.Page");
        StringAssert.Contains(emitted, "InitializeComponent();");
        StringAssert.Contains(emitted, "new HomePageComponent()");
        StringAssert.Contains(emitted, "public sealed class HomePageComponent : ComponentInstance");
    }

    [TestMethod]
    public void Emit_WindowRoot_GeneratesWindowShellPropertiesAndBodyComponent()
    {
        var component = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                Title = "CSXAML Starter";
                Width = 960;
                Height = 640;
                Backdrop = "Mica";

                render <Grid />;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(component);

        StringAssert.Contains(emitted, "public sealed partial class MainWindow : global::Microsoft.UI.Xaml.Window");
        StringAssert.Contains(emitted, "AppWindow.Title");
        StringAssert.Contains(emitted, "AppWindow.Resize");
        StringAssert.Contains(emitted, "SystemBackdrop = new global::Microsoft.UI.Xaml.Media.MicaBackdrop()");
        StringAssert.Contains(emitted, "new MainWindowComponent()");
    }
}
