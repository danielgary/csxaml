namespace Csxaml.Runtime;

internal sealed class ControlledBoolInputState
{
    private bool _isApplyingValue;

    public void Apply(bool currentValue, bool desiredValue, Action<bool> assign)
    {
        if (currentValue == desiredValue)
        {
            return;
        }

        _isApplyingValue = true;
        try
        {
            assign(desiredValue);
        }
        finally
        {
            _isApplyingValue = false;
        }
    }

    public void Dispatch(bool currentValue, Action<bool>? handler)
    {
        if (_isApplyingValue || handler is null)
        {
            return;
        }

        handler(currentValue);
    }
}
