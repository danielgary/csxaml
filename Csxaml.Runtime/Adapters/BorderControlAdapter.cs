using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class BorderControlAdapter : ControlAdapter<Border>
{
    public override string TagName => "Border";

    protected override void ApplyProperties(Border control, NativeElementNode node)
    {
        ApplyBackground(control, node);
        ApplyBorderBrush(control, node);
        ApplyBorderThickness(control, node);
        ApplyPadding(control, node);
    }

    protected override void SetChildren(Border control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 1)
        {
            throw new InvalidOperationException("Border supports only one child.");
        }

        control.Child = children.Count == 0 ? null : children[0];
    }

    private static void ApplyBackground(Border control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Background", out var background))
        {
            control.Background = BrushValueConverter.Convert(background);
            return;
        }

        control.ClearValue(Border.BackgroundProperty);
    }

    private static void ApplyBorderBrush(Border control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "BorderBrush", out var borderBrush))
        {
            control.BorderBrush = BrushValueConverter.Convert(borderBrush);
            return;
        }

        control.ClearValue(Border.BorderBrushProperty);
    }

    private static void ApplyBorderThickness(Border control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<Thickness>(node, "BorderThickness", out var borderThickness))
        {
            control.BorderThickness = borderThickness;
            return;
        }

        control.ClearValue(Border.BorderThicknessProperty);
    }

    private static void ApplyPadding(Border control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<Thickness>(node, "Padding", out var padding))
        {
            control.Padding = padding;
            return;
        }

        control.ClearValue(Border.PaddingProperty);
    }
}
