namespace Csxaml.FeatureGallery.UiTests;

internal static class UiAutomationTestGuard
{
    public static void RequireExplicitInteractiveRun()
    {
        if (!Environment.UserInteractive)
        {
            Assert.Inconclusive("UI automation tests require an interactive Windows desktop.");
        }

        if (Environment.GetEnvironmentVariable("CSXAML_RUN_UI_AUTOMATION") != "1")
        {
            Assert.Inconclusive("Set CSXAML_RUN_UI_AUTOMATION=1 to launch and drive the real feature gallery app.");
        }
    }
}
