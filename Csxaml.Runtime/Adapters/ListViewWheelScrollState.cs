using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class ListViewWheelScrollState
{
    private bool _isAttached;
    private ScrollViewer? _scroller;
    private double _seenOffset;

    public void EnsureAttached(ListView listView)
    {
        if (_isAttached)
        {
            return;
        }

        _isAttached = true;
        listView.AddHandler(
            UIElement.PointerWheelChangedEvent,
            new PointerEventHandler(OnPointerWheelChanged),
            handledEventsToo: true);
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
    {
        if (sender is not ListView listView)
        {
            return;
        }

        var scroller = GetScroller(listView);
        if (scroller is null || scroller.ScrollableHeight <= 0)
        {
            return;
        }

        var currentOffset = scroller.VerticalOffset;
        if (args.Handled && !OffsetsMatch(currentOffset, _seenOffset))
        {
            _seenOffset = currentOffset;
            return;
        }

        var delta = args.GetCurrentPoint(listView).Properties.MouseWheelDelta;
        var nextOffset = WheelScrollOffsetCalculator.CalculateNextOffset(
            currentOffset,
            scroller.ScrollableHeight,
            delta);
        if (OffsetsMatch(nextOffset, currentOffset))
        {
            _seenOffset = currentOffset;
            return;
        }

        if (scroller.ChangeView(null, nextOffset, null, disableAnimation: true))
        {
            _seenOffset = nextOffset;
            args.Handled = true;
        }
    }

    private ScrollViewer? GetScroller(ListView listView)
    {
        if (_scroller is not null)
        {
            return _scroller;
        }

        _scroller = FindDescendantScrollViewer(listView);
        if (_scroller is not null)
        {
            _seenOffset = _scroller.VerticalOffset;
        }

        return _scroller;
    }

    private static ScrollViewer? FindDescendantScrollViewer(DependencyObject root)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is ScrollViewer scroller)
            {
                return scroller;
            }

            var descendant = FindDescendantScrollViewer(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static bool OffsetsMatch(double left, double right)
    {
        return Math.Abs(left - right) < 0.1;
    }
}
