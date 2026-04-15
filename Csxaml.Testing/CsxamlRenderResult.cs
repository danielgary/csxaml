namespace Csxaml.Testing;

public sealed class CsxamlRenderResult : IDisposable, IAsyncDisposable
{
    private readonly ComponentTreeCoordinator _coordinator;
    private NativeElementNode _root;

    internal CsxamlRenderResult(ComponentTreeCoordinator coordinator)
    {
        _coordinator = coordinator;
        _coordinator.TreeUpdated += HandleTreeUpdated;
        _root = CastRoot(_coordinator.Render());
    }

    public NativeElementNode Root => _root;

    public void Click(NativeElementNode node)
    {
        NativeElementInteractor.Click(node);
    }

    public void Dispose()
    {
        _coordinator.TreeUpdated -= HandleTreeUpdated;
        _coordinator.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        _coordinator.TreeUpdated -= HandleTreeUpdated;
        return _coordinator.DisposeAsync();
    }

    public void EnterText(NativeElementNode node, string text)
    {
        NativeElementInteractor.EnterText(node, text);
    }

    public NativeElementNode FindByAutomationId(string automationId)
    {
        return NativeElementQuery.FindByAutomationId(_root, automationId);
    }

    public NativeElementNode FindByAutomationName(string automationName)
    {
        return NativeElementQuery.FindByAutomationName(_root, automationName);
    }

    public NativeElementNode FindByText(string text)
    {
        return NativeElementQuery.FindByText(_root, text);
    }

    public void Rerender()
    {
        _root = CastRoot(_coordinator.Render());
    }

    public void SetChecked(NativeElementNode node, bool value)
    {
        NativeElementInteractor.SetChecked(node, value);
    }

    public NativeElementNode? TryFindByAutomationId(string automationId)
    {
        return NativeElementQuery.TryFindByAutomationId(_root, automationId);
    }

    public NativeElementNode? TryFindByAutomationName(string automationName)
    {
        return NativeElementQuery.TryFindByAutomationName(_root, automationName);
    }

    public NativeElementNode? TryFindByText(string text)
    {
        return NativeElementQuery.TryFindByText(_root, text);
    }

    private static NativeElementNode CastRoot(NativeNode root)
    {
        return root as NativeElementNode
            ?? throw new InvalidOperationException("CSXAML test rendering requires a native-element root.");
    }

    private void HandleTreeUpdated(NativeNode tree)
    {
        _root = CastRoot(tree);
    }
}
