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
        AutomationProperties.SetName(element, string.Empty);
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
