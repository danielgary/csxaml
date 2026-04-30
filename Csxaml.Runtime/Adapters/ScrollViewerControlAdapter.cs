using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class ScrollViewerControlAdapter : ControlAdapter<ScrollViewer>
{
    public override string TagName => "ScrollViewer";

    protected override void ApplyProperties(ScrollViewer control, NativeElementNode node)
    {
        ApplyHorizontalScrollBarVisibility(control, node);
        ApplyHorizontalScrollMode(control, node);
        ApplyVerticalScrollBarVisibility(control, node);
        ApplyVerticalScrollMode(control, node);
    }

    protected override void SetChildren(ScrollViewer control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 1)
        {
            throw new InvalidOperationException("ScrollViewer supports only one child.");
        }

        var nextChild = children.Count == 0 ? null : children[0];
        if (ReferenceEquals(control.Content, nextChild))
        {
            return;
        }

        control.Content = nextChild;
    }

    private static void ApplyHorizontalScrollBarVisibility(
        ScrollViewer control,
        NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ScrollBarVisibility>(
                node,
                "HorizontalScrollBarVisibility",
                out var visibility))
        {
            control.HorizontalScrollBarVisibility = visibility;
            return;
        }

        control.ClearValue(ScrollViewer.HorizontalScrollBarVisibilityProperty);
    }

    private static void ApplyHorizontalScrollMode(
        ScrollViewer control,
        NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ScrollMode>(
                node,
                "HorizontalScrollMode",
                out var mode))
        {
            control.HorizontalScrollMode = mode;
            return;
        }

        control.ClearValue(ScrollViewer.HorizontalScrollModeProperty);
    }

    private static void ApplyVerticalScrollBarVisibility(
        ScrollViewer control,
        NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ScrollBarVisibility>(
                node,
                "VerticalScrollBarVisibility",
                out var visibility))
        {
            control.VerticalScrollBarVisibility = visibility;
            return;
        }

        control.ClearValue(ScrollViewer.VerticalScrollBarVisibilityProperty);
    }

    private static void ApplyVerticalScrollMode(
        ScrollViewer control,
        NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ScrollMode>(
                node,
                "VerticalScrollMode",
                out var mode))
        {
            control.VerticalScrollMode = mode;
            return;
        }

        control.ClearValue(ScrollViewer.VerticalScrollModeProperty);
    }
}
