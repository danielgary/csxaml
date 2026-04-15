using Csxaml.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Runtime.Tests.Testing;

[TestClass]
public sealed class CsxamlTestHostTests
{
    [TestMethod]
    public void Click_RerendersThroughLogicalTreeCoordinator()
    {
        using var render = CsxamlTestHost.Render(new FixedParentComponent());

        render.Click(render.FindByText("Increment"));

        Assert.IsNotNull(render.TryFindByText("Fixed:1"));
    }

    [TestMethod]
    public void Render_WithServiceOverrides_UsesInjectedService()
    {
        using var render = CsxamlTestHost.Render<InjectedGreetingComponent>(
            services => services.AddSingleton(new GreetingService("Hello from test host")));

        Assert.IsNotNull(render.FindByText("Greeting:Hello from test host:1"));
    }
}
