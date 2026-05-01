using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class ScrollViewerWheelScroller
{
    public static bool TryScroll(ScrollViewer scroller, int wheelDelta)
    {
        var currentOffset = scroller.VerticalOffset;
        var nextOffset = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset,
            scroller.ScrollableHeight,
            wheelDelta);

        if (OffsetsMatch(nextOffset, currentOffset))
        {
            return false;
        }

        return scroller.ChangeView(null, nextOffset, null, disableAnimation: true);
    }

    public static bool OffsetsMatch(double left, double right)
    {
        return Math.Abs(left - right) < 0.1;
    }
}
