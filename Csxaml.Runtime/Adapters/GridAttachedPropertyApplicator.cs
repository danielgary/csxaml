using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class GridAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "Column":
                Grid.SetColumn(element, ReadInt(property));
                break;
            case "ColumnSpan":
                Grid.SetColumnSpan(element, ReadInt(property));
                break;
            case "Row":
                Grid.SetRow(element, ReadInt(property));
                break;
            case "RowSpan":
                Grid.SetRowSpan(element, ReadInt(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported Grid attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element)
    {
        Grid.SetColumn(element, 0);
        Grid.SetColumnSpan(element, 1);
        Grid.SetRow(element, 0);
        Grid.SetRowSpan(element, 1);
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
