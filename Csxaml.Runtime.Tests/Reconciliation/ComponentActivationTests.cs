using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class ComponentActivationTests
{
    [TestMethod]
    public void Render_RootInstancePath_StillWorksWithoutServices()
    {
        var coordinator = new ComponentTreeCoordinator(new FixedParentComponent());

        var root = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Version:0", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(root, 0), "Text"));
    }

    [TestMethod]
    public void Render_ChildComponent_UsesServiceAwareActivator()
    {
        var services = new ServiceCollection()
            .AddSingleton(new GreetingService("Hello"))
            .BuildServiceProvider();
        var coordinator = new ComponentTreeCoordinator(
            new ParentUsesConstructorInjectedChildComponent(),
            services);

        var root = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        var child = RuntimeTreeHelpers.GetChildElement(root, 0);

        Assert.AreEqual("Child:Hello", RuntimeTreeHelpers.GetProperty<string>(child, "Text"));
    }
}
