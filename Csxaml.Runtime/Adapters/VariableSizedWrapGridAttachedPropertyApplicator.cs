using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal static class VariableSizedWrapGridAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "ColumnSpan":
                VariableSizedWrapGrid.SetColumnSpan(element, ReadInt(property));
                break;
            case "RowSpan":
                VariableSizedWrapGrid.SetRowSpan(element, ReadInt(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported VariableSizedWrapGrid attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element)
    {
        element.ClearValue(VariableSizedWrapGrid.ColumnSpanProperty);
        element.ClearValue(VariableSizedWrapGrid.RowSpanProperty);
    }

    public static void Clear(FrameworkElement element, string propertyName)
    {
        switch (propertyName)
        {
            case "ColumnSpan":
                element.ClearValue(VariableSizedWrapGrid.ColumnSpanProperty);
                break;
            case "RowSpan":
                element.ClearValue(VariableSizedWrapGrid.RowSpanProperty);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported VariableSizedWrapGrid attached property '{propertyName}'.");
        }
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
