using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class RelativePanelAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "AlignLeftWithPanel":
                RelativePanel.SetAlignLeftWithPanel(element, ReadBool(property));
                break;
            case "AlignTopWithPanel":
                RelativePanel.SetAlignTopWithPanel(element, ReadBool(property));
                break;
            case "Below":
                RelativePanel.SetBelow(element, ReadObject(property));
                break;
            case "RightOf":
                RelativePanel.SetRightOf(element, ReadObject(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported RelativePanel attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element)
    {
        element.ClearValue(RelativePanel.AlignLeftWithPanelProperty);
        element.ClearValue(RelativePanel.AlignTopWithPanelProperty);
        element.ClearValue(RelativePanel.BelowProperty);
        element.ClearValue(RelativePanel.RightOfProperty);
    }

    public static void Clear(FrameworkElement element, string propertyName)
    {
        switch (propertyName)
        {
            case "AlignLeftWithPanel":
                element.ClearValue(RelativePanel.AlignLeftWithPanelProperty);
                break;
            case "AlignTopWithPanel":
                element.ClearValue(RelativePanel.AlignTopWithPanelProperty);
                break;
            case "Below":
                element.ClearValue(RelativePanel.BelowProperty);
                break;
            case "RightOf":
                element.ClearValue(RelativePanel.RightOfProperty);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported RelativePanel attached property '{propertyName}'.");
        }
    }

    private static bool ReadBool(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<bool>(property, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a bool value.");
    }

    private static object? ReadObject(NativeAttachedPropertyValue property)
    {
        return property.Value;
    }
}
