namespace Csxaml.Runtime;

public abstract class ComponentInstance
{
    private readonly ChildComponentStore _childComponents = new();

    public Action? RequestRender { get; set; }

    internal ChildComponentStore ChildComponents => _childComponents;

    public virtual void SetProps(object? props)
    {
        if (props is not null)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' does not accept props.");
        }
    }

    public abstract Node Render();
}
