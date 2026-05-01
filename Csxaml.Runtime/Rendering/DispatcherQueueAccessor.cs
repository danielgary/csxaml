using System.Runtime.InteropServices;

namespace Csxaml.Runtime;

internal static class DispatcherQueueAccessor
{
    public static Microsoft.UI.Dispatching.DispatcherQueue? GetForCurrentThread()
    {
        try
        {
            return Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        }
        catch (COMException)
        {
            return null;
        }
    }
}
