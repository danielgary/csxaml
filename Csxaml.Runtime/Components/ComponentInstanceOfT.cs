namespace Csxaml.Runtime;

/// <summary>
/// Provides the base class for CSXAML components that accept strongly typed props.
/// </summary>
/// <typeparam name="TProps">The props type accepted by the component.</typeparam>
public abstract class ComponentInstance<TProps> : ComponentInstance
{
    private TProps _props = default!;

    /// <summary>
    /// Gets the current props supplied by the parent component.
    /// </summary>
    protected TProps Props => _props;

    /// <inheritdoc />
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
