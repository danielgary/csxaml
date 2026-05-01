namespace Csxaml.Runtime;

internal sealed class ControlledRangeInputState
{
    private bool _isApplyingValue;

    public void Apply(Action assign)
    {
        _isApplyingValue = true;
        try
        {
            assign();
        }
        finally
        {
            _isApplyingValue = false;
        }
    }

    public void Dispatch<TValue>(TValue currentValue, Action<TValue>? handler)
    {
        if (_isApplyingValue || handler is null)
        {
            return;
        }

        handler(currentValue);
    }
}
