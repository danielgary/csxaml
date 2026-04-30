using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;

namespace Csxaml.Runtime;

internal static class AutomationPropertiesAttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.PropertyName)
        {
            case "AutomationId":
                AutomationProperties.SetAutomationId(element, ReadString(property));
                break;
            case "HelpText":
                AutomationProperties.SetHelpText(element, ReadString(property));
                break;
            case "ItemStatus":
                AutomationProperties.SetItemStatus(element, ReadString(property));
                break;
            case "ItemType":
                AutomationProperties.SetItemType(element, ReadString(property));
                break;
            case "LabeledBy":
                AutomationProperties.SetLabeledBy(element, ReadElement(property));
                break;
            case "Name":
                AutomationProperties.SetName(element, ReadString(property));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported AutomationProperties attached property '{property.PropertyName}'.");
        }
    }

    public static void Clear(FrameworkElement element)
    {
        AutomationProperties.SetAutomationId(element, string.Empty);
        AutomationProperties.SetHelpText(element, string.Empty);
        AutomationProperties.SetItemStatus(element, string.Empty);
        AutomationProperties.SetItemType(element, string.Empty);
        element.ClearValue(AutomationProperties.LabeledByProperty);
        AutomationProperties.SetName(element, string.Empty);
    }

    private static UIElement? ReadElement(NativeAttachedPropertyValue property)
    {
        if (property.Value is null)
        {
            return null;
        }

        if (property.Value is UIElement element)
        {
            return element;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a UIElement value.");
    }

    private static string ReadString(NativeAttachedPropertyValue property)
    {
        if (NativeAttachedPropertyValueConverter.TryConvert<string?>(property, out var value))
        {
            return value ?? string.Empty;
        }

        throw new InvalidOperationException(
            $"Attached property '{property.QualifiedName}' expected a string value.");
    }
}
