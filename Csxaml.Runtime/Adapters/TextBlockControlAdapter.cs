using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class TextBlockControlAdapter : ControlAdapter<TextBlock>
{
    public override string TagName => "TextBlock";

    protected override void ApplyProperties(TextBlock control, NativeElementNode node)
    {
        ApplyFontSize(control, node);
        ApplyForeground(control, node);
        ApplyText(control, node);
    }

    protected override void SetChildren(TextBlock control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("TextBlock does not support child elements.");
        }
    }

    private static void ApplyFontSize(TextBlock control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "FontSize", out var fontSize))
        {
            control.FontSize = fontSize;
            return;
        }

        control.ClearValue(TextBlock.FontSizeProperty);
    }

    private static void ApplyForeground(TextBlock control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Foreground", out var foreground))
        {
            control.Foreground = BrushValueConverter.Convert(foreground);
            return;
        }

        control.ClearValue(TextBlock.ForegroundProperty);
    }

    private static void ApplyText(TextBlock control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<string?>(node, "Text", out var text))
        {
            control.Text = text ?? string.Empty;
            return;
        }

        control.ClearValue(TextBlock.TextProperty);
    }
}
