namespace Csxaml.Runtime;

public abstract class ComponentInstance<TProps> : ComponentInstance
{
    private TProps _props = default!;

    protected TProps Props => _props;

    public override void SetProps(object? props)
    {
        if (props is not TProps typedProps)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' expected props of type '{typeof(TProps).Name}'.");
        }

        _props = typedProps;
    }
}
