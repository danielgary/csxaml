namespace Csxaml.Runtime;

public sealed class State<T>
{
    private readonly Action _invalidate;
    private T _value;

    public State(T value, Action invalidate)
    {
        _value = value;
        _invalidate = invalidate;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _invalidate();
            }
        }
    }
}
