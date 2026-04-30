using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class StackPanelControlAdapter : ControlAdapter<StackPanel>
{
    public override string TagName => "StackPanel";

    protected override void ApplyProperties(StackPanel control, NativeElementNode node)
    {
        PanelBackgroundPropertyApplicator.Apply(control, node);
        ApplyOrientation(control, node);
        ApplySpacing(control, node);
    }

    protected override void SetChildren(StackPanel control, IReadOnlyList<UIElement> children)
    {
        UiElementCollectionPatcher.Update(control.Children, children);
    }

    private static void ApplyOrientation(StackPanel control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<Orientation>(node, "Orientation", out var orientation))
        {
            control.Orientation = orientation;
            return;
        }

        control.ClearValue(StackPanel.OrientationProperty);
    }

    private static void ApplySpacing(StackPanel control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Spacing", out var spacing))
        {
            control.Spacing = spacing;
            return;
        }

        control.ClearValue(StackPanel.SpacingProperty);
    }
}
