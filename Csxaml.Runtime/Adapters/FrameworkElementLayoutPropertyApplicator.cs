using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class FrameworkElementLayoutPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeElementNode node)
    {
        ApplyHeight(element, node);
        ApplyHorizontalAlignment(element, node);
        ApplyMargin(element, node);
        ApplyVerticalAlignment(element, node);
        ApplyWidth(element, node);
    }

    private static void ApplyHeight(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Height", out var height))
        {
            element.Height = height;
            return;
        }

        element.ClearValue(FrameworkElement.HeightProperty);
    }

    private static void ApplyHorizontalAlignment(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<HorizontalAlignment>(node, "HorizontalAlignment", out var alignment))
        {
            element.HorizontalAlignment = alignment;
            return;
        }

        element.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
    }

    private static void ApplyMargin(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<Thickness>(node, "Margin", out var margin))
        {
            element.Margin = margin;
            return;
        }

        element.ClearValue(FrameworkElement.MarginProperty);
    }

    private static void ApplyVerticalAlignment(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<VerticalAlignment>(node, "VerticalAlignment", out var alignment))
        {
            element.VerticalAlignment = alignment;
            return;
        }

        element.ClearValue(FrameworkElement.VerticalAlignmentProperty);
    }

    private static void ApplyWidth(FrameworkElement element, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Width", out var width))
        {
            element.Width = width;
            return;
        }

        element.ClearValue(FrameworkElement.WidthProperty);
    }
}
