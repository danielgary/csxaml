namespace Csxaml.Generator.Tests.Emission;

[TestClass]
public sealed class ApplicationRootEmissionTests
{
    [TestMethod]
    public void Emit_ApplicationRoot_GeneratesApplicationShell()
    {
        var app = GeneratorTestHarness.Parse(
            "App.csxaml",
            """
            component Application App {
                startup MainWindow;
                resources AppResources;
            }
            """);
        var window = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                render <Grid />;
            }
            """);
        var resources = GeneratorTestHarness.Parse(
            "AppResources.csxaml",
            """
            component ResourceDictionary AppResources {
                render <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <CustomResources />
                    </ResourceDictionary.MergedDictionaries>
                </ResourceDictionary>;
            }
            """);
        var compilation = new Validator().Validate([app, window, resources], CreateGeneratedProject());

        var emitted = new CodeEmitter().Emit(app, compilation);

        StringAssert.Contains(emitted, "public sealed partial class App : global::Microsoft.UI.Xaml.Application");
        StringAssert.Contains(emitted, "InitializeComponent();");
        StringAssert.Contains(emitted, "AppResources.ApplyTo(Resources);");
        StringAssert.Contains(emitted, "_window = new MainWindow(_services);");
    }

    [TestMethod]
    public void Emit_ApplicationRoot_SkipsDefaultWinUiResourceDictionary()
    {
        var app = GeneratorTestHarness.Parse(
            "App.csxaml",
            """
            component Application App {
                startup MainWindow;
                resources AppResources;
            }
            """);
        var window = GeneratorTestHarness.Parse(
            "MainWindow.csxaml",
            """
            component Window MainWindow {
                render <Grid />;
            }
            """);
        var resources = GeneratorTestHarness.Parse(
            "AppResources.csxaml",
            """
            component ResourceDictionary AppResources {
                render <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <XamlControlsResources />
                    </ResourceDictionary.MergedDictionaries>
                </ResourceDictionary>;
            }
            """);
        var compilation = new Validator().Validate([app, window, resources], CreateGeneratedProject());

        var emitted = new CodeEmitter().Emit(app, compilation);

        StringAssert.Contains(emitted, "InitializeComponent();");
        Assert.IsFalse(emitted.Contains("AppResources.ApplyTo(Resources);", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Emit_ResourceDictionaryRoot_AddsMergedDictionaries()
    {
        var resources = GeneratorTestHarness.Parse(
            "AppResources.csxaml",
            """
            component ResourceDictionary AppResources {
                render <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <XamlControlsResources />
                    </ResourceDictionary.MergedDictionaries>
                </ResourceDictionary>;
            }
            """);

        var emitted = GeneratorTestHarness.Emit(resources);

        StringAssert.Contains(emitted, "public sealed partial class AppResources : global::Microsoft.UI.Xaml.ResourceDictionary");
        StringAssert.Contains(emitted, "public static global::Microsoft.UI.Xaml.ResourceDictionary Create()");
        StringAssert.Contains(emitted, "global::Microsoft.UI.Xaml.Markup.XamlReader.Load");
        StringAssert.Contains(emitted, "<controls:XamlControlsResources />");
        StringAssert.Contains(emitted, "public static void ApplyTo(global::Microsoft.UI.Xaml.ResourceDictionary resources)");
    }

    [TestMethod]
    public void GenerateFiles_GeneratedMode_AddsEntryPoint()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"csxaml-app-mode-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        try
        {
            WriteFile(
                temp,
                "App.csxaml",
                """
                component Application App {
                    startup MainWindow;
                }
                """);
            WriteFile(
                temp,
                "MainWindow.csxaml",
                """
                component Window MainWindow {
                    render <Grid />;
                }
                """);

            var files = new GeneratorRunner().GenerateFiles(
                new GeneratorOptions(
                    Path.Combine(temp, "Generated"),
                    "TestProject",
                    "TestProject",
                    "TestProject.__CsxamlGenerated",
                    CsxamlApplicationMode.Generated,
                    Array.Empty<string>(),
                    Directory.GetFiles(temp, "*.csxaml")));

            Assert.IsTrue(files.Any(file => file.OutputPath.EndsWith("GeneratedApplicationEntryPoint.g.cs", StringComparison.Ordinal)));
            Assert.IsTrue(files.Any(file => file.OutputPath.EndsWith("App.xaml", StringComparison.Ordinal)));
            Assert.IsTrue(files.Any(file => file.Content.Contains("<controls:XamlControlsResources />", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    [TestMethod]
    public void GenerateFiles_PageRoot_AddsHiddenPageXamlCompanion()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"csxaml-page-xaml-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        try
        {
            WriteFile(
                temp,
                "HomePage.csxaml",
                """
                namespace TestProject;

                component Page HomePage {
                    render <Grid />;
                }
                """);

            var files = new GeneratorRunner().GenerateFiles(
                new GeneratorOptions(
                    Path.Combine(temp, "Generated"),
                    "TestProject",
                    "TestProject",
                    "TestProject.__CsxamlGenerated",
                    CsxamlApplicationMode.Hybrid,
                    Array.Empty<string>(),
                    Directory.GetFiles(temp, "*.csxaml")));

            var pageXaml = files.Single(file => file.OutputPath.EndsWith("HomePage.xaml", StringComparison.Ordinal));
            StringAssert.Contains(pageXaml.Content, "x:Class=\"TestProject.HomePage\"");
            StringAssert.Contains(pageXaml.Content, "<Page");
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    private static ProjectGenerationContext CreateGeneratedProject()
    {
        return new ProjectGenerationContext(
            "TestProject",
            "TestProject",
            "TestProject.__CsxamlGenerated",
            CsxamlApplicationMode.Generated);
    }

    private static void WriteFile(string directory, string fileName, string content)
    {
        File.WriteAllText(Path.Combine(directory, fileName), GeneratorTestHarness.Normalize(content));
    }
}
