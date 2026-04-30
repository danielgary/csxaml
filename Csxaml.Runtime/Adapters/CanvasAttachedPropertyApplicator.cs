using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class CanvasAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "Left":
                Canvas.SetLeft(element, ReadDouble(property));
                break;
            case "Top":
                Canvas.SetTop(element, ReadDouble(property));
                break;
            case "ZIndex":
                Canvas.SetZIndex(element, ReadInt(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported Canvas attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element)
    {
        element.ClearValue(Canvas.LeftProperty);
        element.ClearValue(Canvas.TopProperty);
        element.ClearValue(Canvas.ZIndexProperty);
    }

    private static double ReadDouble(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<double>(property, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a double value.");
    }

    private static int ReadInt(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<int>(property, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected an int value.");
    }
}
