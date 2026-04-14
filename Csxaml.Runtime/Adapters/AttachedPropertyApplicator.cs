using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class AttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeElementNode node)
    {
        GridAttachedPropertyApplicator.Clear(element);
        AutomationPropertiesAttachedPropertyApplicator.Clear(element);

        foreach (var property in node.AttachedProperties)
        {
            ApplyProperty(element, property);
        }
    }

    private static void ApplyProperty(FrameworkElement element, NativeAttachedPropertyValue property)
    {
        switch (property.OwnerName)
        {
            case "AutomationProperties":
                AutomationPropertiesAttachedPropertyApplicator.Apply(element, property);
                break;
            case "Grid":
                GridAttachedPropertyApplicator.Apply(element, property);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported attached property owner '{property.OwnerName}'.");
        }
    }
}
