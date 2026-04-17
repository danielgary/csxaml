namespace Csxaml.Runtime;

public sealed class State<T>
{
    private readonly Action? _beforeWrite;
    private readonly Action _invalidate;
    private T _value;

    public State(T value, Action invalidate, Action? beforeWrite = null)
    {
        _value = value;
        _invalidate = invalidate;
        _beforeWrite = beforeWrite;
    }

    public T Value
    {
        get => _value;
        set
        {
            _beforeWrite?.Invoke();
            if (HasEqualValue(_value, value))
            {
                return;
            }

            _value = value;
            _invalidate();
        }
    }

    public void Touch()
    {
        _beforeWrite?.Invoke();
        _invalidate();
    }

    private static bool HasEqualValue(T previous, T next)
    {
        if (typeof(T).IsValueType)
        {
            return EqualityComparer<T>.Default.Equals(previous, next);
        }

        return ReferenceEquals(previous, next);
    }
}
