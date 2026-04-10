using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Csxaml.Runtime;

internal sealed class ButtonControlAdapter : ControlAdapter<Button>
{
    public override string TagName => "Button";

    protected override void ApplyEvents(
        Button control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeElementReader.TryGetEventHandler<Action>(node, "OnClick", out var onClick);
        bindingStore.Rebind(
            "OnClick",
            onClick,
            handler =>
            {
                RoutedEventHandler routedHandler = (_, _) => handler();
                control.Click += routedHandler;
                return () => control.Click -= routedHandler;
            });
    }

    protected override void ApplyProperties(Button control, NativeElementNode node)
    {
        ApplyBackground(control, node);
        ApplyContent(control, node);
        ApplyFontSize(control, node);
        ApplyForeground(control, node);
    }

    protected override void SetChildren(Button control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("Button does not support child elements.");
        }
    }

    private static void ApplyBackground(Button control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Background", out var background))
        {
            control.Background = BrushValueConverter.Convert(background);
            return;
        }

        control.ClearValue(Control.BackgroundProperty);
    }

    private static void ApplyContent(Button control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Content", out var content))
        {
            control.Content = content;
            return;
        }

        control.ClearValue(ContentControl.ContentProperty);
    }

    private static void ApplyFontSize(Button control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "FontSize", out var fontSize))
        {
            control.FontSize = fontSize;
            return;
        }

        control.ClearValue(Control.FontSizeProperty);
    }

    private static void ApplyForeground(Button control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Foreground", out var foreground))
        {
            control.Foreground = BrushValueConverter.Convert(foreground);
            return;
        }

        control.ClearValue(Control.ForegroundProperty);
    }
}
