using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
public sealed class InjectedServiceResolutionTests
{
    [TestMethod]
    public void ConstructingCoordinator_WithoutRequiredService_FailsWithComponentAndMemberContext()
    {
        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(
            () => new ComponentTreeCoordinator(new InjectedGreetingComponent()));

        StringAssert.Contains(error.Message, "Failed to resolve required service");
        StringAssert.Contains(error.Message, "Component: InjectedGreeting");
        StringAssert.Contains(error.Message, "Member: greeting");
        StringAssert.Contains(error.Message, "Stage: service resolution");
    }

    [TestMethod]
    public void Render_KeyedChild_ReusesInjectedComponentWithoutResolvingAgain()
    {
        var services = new ServiceCollection()
            .AddSingleton(new GreetingService("Hello"))
            .BuildServiceProvider();
        var host = new InjectedGreetingHostComponent();
        var coordinator = new ComponentTreeCoordinator(host, services);

        var firstTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());
        host.Version.Value++;
        var secondTree = RuntimeTreeHelpers.RootStackPanel(coordinator.Render());

        Assert.AreEqual("Greeting:Hello:1", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(firstTree, 1), "Text"));
        Assert.AreEqual("Greeting:Hello:1", RuntimeTreeHelpers.GetProperty<string>(RuntimeTreeHelpers.GetChildElement(secondTree, 1), "Text"));
    }
}
