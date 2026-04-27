namespace Csxaml.Runtime;

/// <summary>
/// Stores component state and requests rendering when the value changes.
/// </summary>
/// <typeparam name="T">The state value type.</typeparam>
public sealed class State<T>
{
    private readonly Action? _beforeWrite;
    private readonly Action _invalidate;
    private T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="State{T}"/> class.
    /// </summary>
    /// <param name="value">The initial state value.</param>
    /// <param name="invalidate">The callback invoked when the state should trigger a render.</param>
    /// <param name="beforeWrite">An optional callback invoked before any state write is accepted.</param>
    public State(T value, Action invalidate, Action? beforeWrite = null)
    {
        _value = value;
        _invalidate = invalidate;
        _beforeWrite = beforeWrite;
    }

    /// <summary>
    /// Gets or sets the current state value.
    /// </summary>
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

    /// <summary>
    /// Forces a render request without changing the stored value.
    /// </summary>
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
