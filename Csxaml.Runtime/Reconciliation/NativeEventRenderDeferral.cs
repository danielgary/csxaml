namespace Csxaml.Runtime;

internal static class NativeEventRenderDeferral
{
    [ThreadStatic]
    private static int _deferDepth;

    [ThreadStatic]
    private static List<Action>? _pendingRenders;

    public static bool IsActive => _deferDepth > 0;

    public static IDisposable Begin()
    {
        _deferDepth++;
        return new DeferralScope();
    }

    public static void Queue(Action render)
    {
        _pendingRenders ??= [];
        _pendingRenders.Add(render);
    }

    private static void End()
    {
        _deferDepth--;
        if (_deferDepth > 0)
        {
            return;
        }

        var pending = _pendingRenders ?? [];
        _pendingRenders = null;
        Schedule(pending);
    }

    private static void Schedule(IReadOnlyList<Action> pending)
    {
        var dispatcher = DispatcherQueueAccessor.GetForCurrentThread();
        foreach (var render in pending)
        {
            if (dispatcher is not null &&
                dispatcher.TryEnqueue(
                    Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                    () => render()))
            {
                continue;
            }

            render();
        }
    }

    private sealed class DeferralScope : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            End();
        }
    }
}
