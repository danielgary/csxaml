namespace Csxaml.Runtime;

public abstract class ComponentInstance
{
    private readonly ChildComponentStore _childComponents = new();
    private IReadOnlyList<Node> _childContent = Array.Empty<Node>();

    public Action? RequestRender { get; set; }

    internal ChildComponentStore ChildComponents => _childComponents;

    protected IReadOnlyList<Node> ChildContent => _childContent;

    public virtual void SetProps(object? props)
    {
        if (props is not null)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' does not accept props.");
        }
    }

    internal void SetChildContent(IReadOnlyList<Node> childContent)
    {
        _childContent = childContent;
    }

    public abstract Node Render();
}
