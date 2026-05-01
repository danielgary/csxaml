using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal static class AttachedPropertyApplicator
{
    private static readonly ConditionalWeakTable<FrameworkElement, AppliedAttachedPropertyState> States = new();

    public static void Apply(FrameworkElement element, NativeElementNode node)
    {
        var state = States.GetValue(element, _ => new AppliedAttachedPropertyState());
        var next = node.AttachedProperties
            .Select(property => new AttachedPropertyKey(property.OwnerName, property.PropertyName))
            .ToArray();

        ClearRemovedProperties(element, state.Properties, next);
        foreach (var property in node.AttachedProperties)
        {
            ApplyProperty(element, property);
        }

        state.Replace(next);
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
            case "ScrollViewer":
                ScrollViewerAttachedPropertyApplicator.Apply(element, property);
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

    private static void ClearProperty(FrameworkElement element, AttachedPropertyKey property)
    {
        switch (property.OwnerName)
        {
            case "AutomationProperties":
                AutomationPropertiesAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "Canvas":
                CanvasAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "Grid":
                GridAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "RelativePanel":
                RelativePanelAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "ScrollViewer":
                ScrollViewerAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "ToolTipService":
                ToolTipServiceAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            case "VariableSizedWrapGrid":
                VariableSizedWrapGridAttachedPropertyApplicator.Clear(element, property.PropertyName);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported attached property owner '{property.OwnerName}'.");
        }
    }

    private static void ClearRemovedProperties(
        FrameworkElement element,
        IReadOnlyList<AttachedPropertyKey> previous,
        IReadOnlyList<AttachedPropertyKey> next)
    {
        foreach (var property in previous)
        {
            if (next.Contains(property))
            {
                continue;
            }

            ClearProperty(element, property);
        }
    }

    private readonly record struct AttachedPropertyKey(string OwnerName, string PropertyName);

    private sealed class AppliedAttachedPropertyState
    {
        public IReadOnlyList<AttachedPropertyKey> Properties { get; private set; } =
            Array.Empty<AttachedPropertyKey>();

        public void Replace(IReadOnlyList<AttachedPropertyKey> properties)
        {
            Properties = properties;
        }
    }
}
