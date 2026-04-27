namespace Csxaml.Testing;

/// <summary>
/// Represents an in-memory CSXAML render session for component tests.
/// </summary>
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

    /// <summary>
    /// Gets the latest rendered native root element.
    /// </summary>
    public NativeElementNode Root => _root;

    /// <summary>
    /// Invokes the click handler assigned to a native node.
    /// </summary>
    /// <param name="node">The node whose click handler should be invoked.</param>
    public void Click(NativeElementNode node)
    {
        NativeElementInteractor.Click(node);
    }

    /// <summary>
    /// Disposes the render session and component tree.
    /// </summary>
    public void Dispose()
    {
        _coordinator.TreeUpdated -= HandleTreeUpdated;
        _coordinator.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the render session and component tree.
    /// </summary>
    /// <returns>A task-like value that completes when asynchronous disposal has finished.</returns>
    public ValueTask DisposeAsync()
    {
        _coordinator.TreeUpdated -= HandleTreeUpdated;
        return _coordinator.DisposeAsync();
    }

    /// <summary>
    /// Invokes the text-change handler assigned to a native node.
    /// </summary>
    /// <param name="node">The node whose text handler should be invoked.</param>
    /// <param name="text">The text value to pass to the handler.</param>
    public void EnterText(NativeElementNode node, string text)
    {
        NativeElementInteractor.EnterText(node, text);
    }

    /// <summary>
    /// Finds the first node with the specified automation id.
    /// </summary>
    /// <param name="automationId">The automation id to search for.</param>
    /// <returns>The first matching native element.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching node is found.</exception>
    public NativeElementNode FindByAutomationId(string automationId)
    {
        return NativeElementQuery.FindByAutomationId(_root, automationId);
    }

    /// <summary>
    /// Finds the first node with the specified automation name.
    /// </summary>
    /// <param name="automationName">The automation name to search for.</param>
    /// <returns>The first matching native element.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching node is found.</exception>
    public NativeElementNode FindByAutomationName(string automationName)
    {
        return NativeElementQuery.FindByAutomationName(_root, automationName);
    }

    /// <summary>
    /// Finds the first node with the specified text or content value.
    /// </summary>
    /// <param name="text">The text or content value to search for.</param>
    /// <returns>The first matching native element.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching node is found.</exception>
    public NativeElementNode FindByText(string text)
    {
        return NativeElementQuery.FindByText(_root, text);
    }

    /// <summary>
    /// Forces the component tree to render again.
    /// </summary>
    public void Rerender()
    {
        _root = CastRoot(_coordinator.Render());
    }

    /// <summary>
    /// Invokes the checked-change handler assigned to a native node.
    /// </summary>
    /// <param name="node">The node whose checked handler should be invoked.</param>
    /// <param name="value">The checked value to pass to the handler.</param>
    public void SetChecked(NativeElementNode node, bool value)
    {
        NativeElementInteractor.SetChecked(node, value);
    }

    /// <summary>
    /// Attempts to find the first node with the specified automation id.
    /// </summary>
    /// <param name="automationId">The automation id to search for.</param>
    /// <returns>The first matching native element, or <see langword="null"/> when none is found.</returns>
    public NativeElementNode? TryFindByAutomationId(string automationId)
    {
        return NativeElementQuery.TryFindByAutomationId(_root, automationId);
    }

    /// <summary>
    /// Attempts to find the first node with the specified automation name.
    /// </summary>
    /// <param name="automationName">The automation name to search for.</param>
    /// <returns>The first matching native element, or <see langword="null"/> when none is found.</returns>
    public NativeElementNode? TryFindByAutomationName(string automationName)
    {
        return NativeElementQuery.TryFindByAutomationName(_root, automationName);
    }

    /// <summary>
    /// Attempts to find the first node with the specified text or content value.
    /// </summary>
    /// <param name="text">The text or content value to search for.</param>
    /// <returns>The first matching native element, or <see langword="null"/> when none is found.</returns>
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
