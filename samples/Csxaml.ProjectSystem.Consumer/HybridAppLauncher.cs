using Microsoft.UI.Xaml;

namespace Csxaml.ProjectSystem.Consumer;

public static class HybridAppLauncher
{
    public static Type StartupWindowType => typeof(ShellWindow);

    public static Window CreateStartupWindow(IServiceProvider? services = null)
    {
        return new ShellWindow(services);
    }
}
