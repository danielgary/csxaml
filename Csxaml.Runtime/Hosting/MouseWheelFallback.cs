using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class MouseWheelFallback
{
    public static ScrollViewer? FindTarget(
        UIElement rootElement,
        IntPtr windowHandle,
        int screenX,
        int screenY)
    {
        var clientPoint = PointCoordinateMapper.ScreenToClient(
            windowHandle,
            screenX,
            screenY);
        return WheelScrollTargetFinder.Find(rootElement, clientPoint);
    }

    public static bool TryScrollIfUnchanged(
        ScrollViewer scroller,
        double previousOffset,
        int wheelDelta)
    {
        if (!ScrollViewerWheelScroller.OffsetsMatch(previousOffset, scroller.VerticalOffset))
        {
            return false;
        }

        return ScrollViewerWheelScroller.TryScroll(scroller, wheelDelta);
    }
}
