namespace Csxaml.Runtime;

internal sealed class ControlledTextInputState
{
    private bool _isApplyingValue;

    public void Apply(string currentValue, string desiredValue, Action<string> assign)
    {
        if (string.Equals(currentValue, desiredValue, StringComparison.Ordinal))
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

    public void Dispatch(string currentValue, Action<string>? handler)
    {
        if (_isApplyingValue || handler is null)
        {
            return;
        }

        handler(currentValue);
    }
}
