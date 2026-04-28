namespace Csxaml.Benchmarks;

internal sealed class RuntimeScenario<TComponent> : IDisposable
    where TComponent : ComponentInstance
{
    private int _updatedTreeCount;

    public RuntimeScenario(TComponent root)
    {
        Root = root;
        Coordinator = new ComponentTreeCoordinator(root);
        Coordinator.TreeUpdated += _ => _updatedTreeCount++;
    }

    public TComponent Root { get; }

    public ComponentTreeCoordinator Coordinator { get; }

    public int UpdatedTreeCount => _updatedTreeCount;

    public NativeNode Render()
    {
        return Coordinator.Render();
    }

    public void Dispose()
    {
        Coordinator.Dispose();
    }
}
