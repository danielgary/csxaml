using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
[DoNotParallelize]
public sealed class CsxamlRootHostTests
{
    [TestMethod]
    public void Mount_Page_AddsRenderedElementToContent()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var page = new Page();
            using var host = CsxamlRootHost.Mount(page, new AttachedRootComponent());

            Assert.IsInstanceOfType<Border>(page.Content);
        });
    }

    [TestMethod]
    public void Dispose_Page_ClearsRenderedContent()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var page = new Page();
            var host = CsxamlRootHost.Mount(page, new AttachedRootComponent());

            host.Dispose();

            Assert.IsNull(page.Content);
        });
    }

    [TestMethod]
    public void Mount_Window_AddsRenderedElementToContent()
    {
        WinUiTestEnvironment.RunInWindow(window =>
        {
            using var host = CsxamlRootHost.Mount(window, new AttachedRootComponent());

            Assert.IsInstanceOfType<Border>(window.Content);
        });
    }
}
