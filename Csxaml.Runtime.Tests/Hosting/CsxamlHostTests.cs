using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
[DoNotParallelize]
public sealed class CsxamlHostTests
{
    [TestMethod]
    public void Render_RootInstancePath_AddsRenderedElementToPanel()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var panel = new StackPanel();
            var host = new CsxamlHost(panel, new AttachedRootComponent());

            host.Render();

            Assert.AreEqual(1, panel.Children.Count);
            Assert.IsInstanceOfType<Border>(panel.Children[0]);
        });
    }

    [TestMethod]
    public void Render_RootComponentType_UsesServiceAwareActivation()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var services = new ServiceCollection()
                .AddSingleton(new GreetingService("Hello from host"))
                .BuildServiceProvider();
            var panel = new StackPanel();
            var host = new CsxamlHost(panel, typeof(ConstructorInjectedRootComponent), services);

            host.Render();

            Assert.AreEqual(1, panel.Children.Count);
            Assert.AreEqual("Hello from host", ((TextBlock)panel.Children[0]).Text);
        });
    }

    [TestMethod]
    public void DisposeAsync_ClearsHostPanelAndDisposesRootComponent()
    {
        WinUiTestEnvironment.Run(() =>
        {
            AsyncDisposableProbeComponent.Reset();
            var panel = new StackPanel();
            var host = new CsxamlHost(panel, new AsyncDisposableProbeComponent());

            host.Render();
            host.DisposeAsync().AsTask().GetAwaiter().GetResult();

            Assert.AreEqual(0, panel.Children.Count);
            Assert.AreEqual(1, AsyncDisposableProbeComponent.DisposeAsyncCount);
        });
    }
}
