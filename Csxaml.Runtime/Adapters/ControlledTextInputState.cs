namespace Csxaml.Runtime;

internal sealed class ControlledTextInputState
{
    private IDisposable? _inputDeferral;
    private int _inputDeferralVersion;
    private bool _isApplyingValue;
    private int? _queuedInputCompletionVersion;
    private bool _restoreFocusAfterInput;

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

    public void BeginInput()
    {
        BeginInput(restoreFocusAfterInput: false);
    }

    public void BeginInput(bool restoreFocusAfterInput)
    {
        _inputDeferral ??= NativeEventRenderDeferral.Begin();
        _inputDeferralVersion++;
        _queuedInputCompletionVersion = null;
        _restoreFocusAfterInput |= restoreFocusAfterInput;
    }

    public bool ConsumeFocusRestoreRequest()
    {
        var restoreFocus = _restoreFocusAfterInput;
        _restoreFocusAfterInput = false;
        return restoreFocus;
    }

    public void CompleteInput()
    {
        _inputDeferral?.Dispose();
        _inputDeferral = null;
        _queuedInputCompletionVersion = null;
    }

    public void ScheduleInputCompletion(Microsoft.UI.Dispatching.DispatcherQueue? dispatcher)
    {
        if (_inputDeferral is null)
        {
            return;
        }

        var version = _inputDeferralVersion;
        if (_queuedInputCompletionVersion == version)
        {
            return;
        }

        _queuedInputCompletionVersion = version;
        if (dispatcher?.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                () => CompleteInput(version)) == true)
        {
            return;
        }

        CompleteInput(version);
    }

    private void CompleteInput(int version)
    {
        if (version != _inputDeferralVersion)
        {
            return;
        }

        CompleteInput();
    }
}
