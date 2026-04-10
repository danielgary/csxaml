using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class StackPanelControlAdapter : ControlAdapter<StackPanel>
{
    public override string TagName => "StackPanel";

    protected override void ApplyProperties(StackPanel control, NativeElementNode node)
    {
        ApplyBackground(control, node);
        ApplyOrientation(control, node);
        ApplySpacing(control, node);
    }

    protected override void SetChildren(StackPanel control, IReadOnlyList<UIElement> children)
    {
        control.Children.Clear();
        foreach (var child in children)
        {
            control.Children.Add(child);
        }
    }

    private static void ApplyBackground(StackPanel control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Background", out var background))
        {
            control.Background = BrushValueConverter.Convert(background);
            return;
        }

        control.ClearValue(Panel.BackgroundProperty);
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
