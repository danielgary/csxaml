using System.Diagnostics;
using System.IO;
using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

internal sealed class FeatureGalleryAppSession : IDisposable
{
    private static readonly TimeSpan LaunchTimeout = TimeSpan.FromSeconds(20);

    private readonly TestContext testContext;
    private bool disposed;

    private FeatureGalleryAppSession(
        TestContext testContext,
        Process process,
        AutomationElement window)
    {
        this.testContext = testContext;
        Process = process;
        Window = window;
    }

    public Process Process { get; }

    public AutomationElement Window { get; private set; }

    public IntPtr MainWindowHandle => Process.MainWindowHandle;

    public static FeatureGalleryAppSession Launch(TestContext testContext)
    {
        UiAutomationTestGuard.RequireExplicitInteractiveRun();

        var executablePath = FeatureGalleryPaths.ExecutablePath;
        Assert.IsTrue(
            File.Exists(executablePath),
            $"Feature gallery executable was not found at '{executablePath}'. Build the sample before running UI automation tests.");

        var process = Process.Start(new ProcessStartInfo(executablePath)
        {
            WorkingDirectory = Path.GetDirectoryName(executablePath)!
        });

        Assert.IsNotNull(process, "Failed to launch the feature gallery process.");

        var window = AutomationWait.Until(
            () => AutomationElementQueries.FindProcessWindow(process.Id),
            LaunchTimeout,
            "the feature gallery main window");

        testContext.WriteLine($"Launched feature gallery process {process.Id}.");
        testContext.WriteLine($"Main window: '{window.Current.Name}'.");

        return new FeatureGalleryAppSession(testContext, process, window);
    }

    public void NavigateToRefsAndEvents()
    {
        var button = AutomationWait.Until(
            () => Window.FindButtonByName("Refs and events"),
            TimeSpan.FromSeconds(10),
            "the Refs and events navigation button");

        button.Invoke();
        RefreshWindow();

        AutomationWait.Until(
            () => Window.FindByAutomationId("FeatureGallerySearchBox"),
            TimeSpan.FromSeconds(10),
            "the Refs and events search textbox");
    }

    public AutomationElement WaitForAutomationId(string automationId)
    {
        RefreshWindow();
        return AutomationWait.Until(
            () => Window.FindByAutomationId(automationId),
            TimeSpan.FromSeconds(10),
            $"element with AutomationId '{automationId}'");
    }

    public void WriteVisibleText(string label)
    {
        testContext.WriteLine(label);

        foreach (var text in Window.ReadVisibleText())
        {
            testContext.WriteLine($"  {text}");
        }
    }

    public void RefreshWindow()
    {
        if (Process.HasExited)
        {
            Assert.Fail($"Feature gallery process exited with code {Process.ExitCode}.");
        }

        Window = AutomationWait.Until(
            () => AutomationElementQueries.FindProcessWindow(Process.Id),
            TimeSpan.FromSeconds(5),
            "the feature gallery main window");
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (!Process.HasExited)
        {
            Process.CloseMainWindow();
            if (!Process.WaitForExit(3000))
            {
                Process.Kill(entireProcessTree: true);
                Process.WaitForExit(3000);
            }
        }

        Process.Dispose();
    }
}
