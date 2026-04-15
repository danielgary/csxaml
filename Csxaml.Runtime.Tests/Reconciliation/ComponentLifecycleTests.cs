namespace Csxaml.Runtime.Tests.Reconciliation;

[TestClass]
[DoNotParallelize]
public sealed class ComponentLifecycleTests
{
    [TestMethod]
    public void Render_RetainedChild_MountsOnlyOnce()
    {
        MountedProbeChildComponent.Reset();
        var host = new MountedProbeHostComponent();
        var coordinator = new ComponentTreeCoordinator(host);

        coordinator.Render();
        var mounted = MountedProbeChildComponent.LastCreated!;
        host.Version.Value++;
        coordinator.Render();

        Assert.AreEqual(1, mounted.MountCount);
    }

    [TestMethod]
    public void Render_RemovedChild_DisposesAndPostUnmountInvalidationNoops()
    {
        DisposableProbeChildComponent.Reset();
        var host = new DisposableProbeHostComponent();
        var coordinator = new ComponentTreeCoordinator(host);

        coordinator.Render();
        var removed = DisposableProbeChildComponent.LastCreated!;
        host.ShowChild = false;
        coordinator.Render();
        var updateCount = 0;
        coordinator.TreeUpdated += _ => updateCount++;

        removed.Count.Value++;

        Assert.AreEqual(1, DisposableProbeChildComponent.DisposeCount, DisposableProbeChildComponent.DisposalTrace);
        Assert.AreEqual(0, updateCount);
    }

    [TestMethod]
    public void Dispose_Coordinator_DisposesRetainedChildSubtree()
    {
        DisposableProbeChildComponent.Reset();
        var host = new DisposableProbeHostComponent();
        var coordinator = new ComponentTreeCoordinator(host);

        coordinator.Render();
        coordinator.Dispose();

        Assert.AreEqual(1, DisposableProbeChildComponent.DisposeCount, DisposableProbeChildComponent.DisposalTrace);
    }

    [TestMethod]
    public async Task DisposeAsync_Coordinator_DisposesAsyncRoot()
    {
        AsyncDisposableProbeComponent.Reset();
        var coordinator = new ComponentTreeCoordinator(new AsyncDisposableProbeComponent());

        coordinator.Render();
        await coordinator.DisposeAsync();

        Assert.AreEqual(1, AsyncDisposableProbeComponent.DisposeAsyncCount);
    }
}
