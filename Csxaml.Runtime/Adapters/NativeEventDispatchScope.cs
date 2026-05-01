namespace Csxaml.Runtime;

internal static class NativeEventDispatchScope
{
    [ThreadStatic]
    private static int _activeDispatchCount;

    public static bool IsActive => _activeDispatchCount > 0;

    public static void Invoke(Action action)
    {
        _activeDispatchCount++;
        try
        {
            action();
        }
        finally
        {
            _activeDispatchCount--;
        }
    }
}
