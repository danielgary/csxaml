using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Csxaml.Runtime;

internal static class WheelScrollTargetFinder
{
    public static ScrollViewer? Find(UIElement rootElement, Point clientPoint)
    {
        var containingScroller = FindContainingScrollViewer(
            rootElement,
            rootElement,
            clientPoint);
        if (containingScroller is not null)
        {
            return containingScroller;
        }

        var candidates = VisualTreeHelper.FindElementsInHostCoordinates(
            clientPoint,
            rootElement,
            includeAllElements: true);
        ScrollViewer? fallback = null;

        foreach (var candidate in candidates)
        {
            var scroller = FindAncestorScrollViewer(candidate, ref fallback);
            if (scroller is not null)
            {
                return scroller;
            }
        }

        foreach (var candidate in candidates)
        {
            var scroller = FindDescendantScrollViewer(candidate, ref fallback);
            if (scroller is not null)
            {
                return scroller;
            }
        }

        return fallback;
    }

    private static ScrollViewer? FindAncestorScrollViewer(
        DependencyObject? current,
        ref ScrollViewer? fallback)
    {
        while (current is not null)
        {
            if (current is ScrollViewer scroller)
            {
                fallback ??= scroller;
                if (CanScrollVertically(scroller))
                {
                    return scroller;
                }
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static ScrollViewer? FindDescendantScrollViewer(
        DependencyObject root,
        ref ScrollViewer? fallback)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is ScrollViewer scroller)
            {
                fallback ??= scroller;
                if (CanScrollVertically(scroller))
                {
                    return scroller;
                }
            }

            var descendant = FindDescendantScrollViewer(child, ref fallback);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static ScrollViewer? FindContainingScrollViewer(
        DependencyObject current,
        UIElement rootElement,
        Point clientPoint)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(current);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(current, index);
            var descendant = FindContainingScrollViewer(
                child,
                rootElement,
                clientPoint);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        if (current is not ScrollViewer scroller ||
            !CanScrollVertically(scroller) ||
            !ContainsPoint(scroller, rootElement, clientPoint))
        {
            return null;
        }

        return scroller;
    }

    private static bool ContainsPoint(
        FrameworkElement element,
        UIElement rootElement,
        Point clientPoint)
    {
        if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
        {
            return false;
        }

        try
        {
            var bounds = element.TransformToVisual(rootElement)
                .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            return bounds.Contains(clientPoint);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool CanScrollVertically(ScrollViewer scroller)
    {
        return scroller.ScrollableHeight > 0;
    }
}
