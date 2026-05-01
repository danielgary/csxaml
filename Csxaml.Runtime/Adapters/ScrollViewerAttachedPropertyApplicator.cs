using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class ScrollViewerAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "HorizontalScrollBarVisibility":
                element.SetValue(
                    ScrollViewer.HorizontalScrollBarVisibilityProperty,
                    ReadScrollBarVisibility(property));
                break;
            case "HorizontalScrollMode":
                element.SetValue(ScrollViewer.HorizontalScrollModeProperty, ReadScrollMode(property));
                break;
            case "VerticalScrollBarVisibility":
                element.SetValue(
                    ScrollViewer.VerticalScrollBarVisibilityProperty,
                    ReadScrollBarVisibility(property));
                break;
            case "VerticalScrollMode":
                element.SetValue(ScrollViewer.VerticalScrollModeProperty, ReadScrollMode(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported ScrollViewer attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element, string propertyName)
    {
        switch (propertyName)
        {
            case "HorizontalScrollBarVisibility":
                element.ClearValue(ScrollViewer.HorizontalScrollBarVisibilityProperty);
                break;
            case "HorizontalScrollMode":
                element.ClearValue(ScrollViewer.HorizontalScrollModeProperty);
                break;
            case "VerticalScrollBarVisibility":
                element.ClearValue(ScrollViewer.VerticalScrollBarVisibilityProperty);
                break;
            case "VerticalScrollMode":
                element.ClearValue(ScrollViewer.VerticalScrollModeProperty);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported ScrollViewer attached property '{propertyName}'.");
        }
    }

    private static ScrollBarVisibility ReadScrollBarVisibility(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<ScrollBarVisibility>(property, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a ScrollBarVisibility value.");
    }

    private static ScrollMode ReadScrollMode(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<ScrollMode>(property, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a ScrollMode value.");
    }
}
