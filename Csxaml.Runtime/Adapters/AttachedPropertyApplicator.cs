using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class AttachedPropertyApplicator
{
    public static void Apply(FrameworkElement element, NativeElementNode node)
    {
        GridAttachedPropertyApplicator.Clear(element);
        AutomationPropertiesAttachedPropertyApplicator.Clear(element);
        CanvasAttachedPropertyApplicator.Clear(element);
        RelativePanelAttachedPropertyApplicator.Clear(element);
        ToolTipServiceAttachedPropertyApplicator.Clear(element);
        VariableSizedWrapGridAttachedPropertyApplicator.Clear(element);

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
            case "Canvas":
                CanvasAttachedPropertyApplicator.Apply(element, property);
                break;
            case "Grid":
                GridAttachedPropertyApplicator.Apply(element, property);
                break;
            case "RelativePanel":
                RelativePanelAttachedPropertyApplicator.Apply(element, property);
                break;
            case "ToolTipService":
                ToolTipServiceAttachedPropertyApplicator.Apply(element, property);
                break;
            case "VariableSizedWrapGrid":
                VariableSizedWrapGridAttachedPropertyApplicator.Apply(element, property);
                break;
            default:
                throw CsxamlRuntimeExceptionBuilder.Wrap(
                    new InvalidOperationException(
                        $"Unsupported attached property owner '{property.OwnerName}'."),
                    "attached property application",
                    sourceInfo: property.SourceInfo,
                    detail: property.QualifiedName);
        }
    }
}
