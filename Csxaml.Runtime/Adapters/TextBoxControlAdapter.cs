using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class TextBoxControlAdapter : ControlAdapter<TextBox>
{
    private static readonly ConditionalWeakTable<TextBox, ControlledTextInputState> States = new();

    public override string TagName => "TextBox";

    protected override void ApplyEvents(
        TextBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeElementReader.TryGetEventHandler<Action<string>>(node, "OnTextChanged", out var onTextChanged);
        bindingStore.Rebind(
            "OnTextChanged",
            onTextChanged,
            handler =>
            {
                TextChangedEventHandler typedHandler = (_, _) =>
                {
                    GetState(control).Dispatch(control.Text ?? string.Empty, handler);
                };

                control.TextChanged += typedHandler;
                return () => control.TextChanged -= typedHandler;
            });
    }

    protected override void ApplyProperties(TextBox control, NativeElementNode node)
    {
        ApplyAcceptsReturn(control, node);
        ApplyMinHeight(control, node);
        ApplyPlaceholderText(control, node);
        ApplyText(control, node);
        ApplyTextWrapping(control, node);
        ApplyWidth(control, node);
    }

    protected override void SetChildren(TextBox control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("TextBox does not support child elements.");
        }
    }

    private static void ApplyAcceptsReturn(TextBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<bool>(node, "AcceptsReturn", out var acceptsReturn))
        {
            control.AcceptsReturn = acceptsReturn;
            return;
        }

        control.ClearValue(TextBox.AcceptsReturnProperty);
    }

    private static void ApplyMinHeight(TextBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "MinHeight", out var minHeight))
        {
            control.MinHeight = minHeight;
            return;
        }

        control.ClearValue(FrameworkElement.MinHeightProperty);
    }

    private static void ApplyPlaceholderText(TextBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<string?>(node, "PlaceholderText", out var placeholderText))
        {
            control.PlaceholderText = placeholderText ?? string.Empty;
            return;
        }

        control.ClearValue(TextBox.PlaceholderTextProperty);
    }

    private static void ApplyText(TextBox control, NativeElementNode node)
    {
        if (!NativeElementReader.TryGetPropertyValue<string?>(node, "Text", out var text))
        {
            SetText(control, string.Empty);
            return;
        }

        SetText(control, text ?? string.Empty);
    }

    private static void ApplyTextWrapping(TextBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<TextWrapping>(node, "TextWrapping", out var textWrapping))
        {
            control.TextWrapping = textWrapping;
            return;
        }

        control.ClearValue(TextBox.TextWrappingProperty);
    }

    private static void ApplyWidth(TextBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Width", out var width))
        {
            control.Width = width;
            return;
        }

        control.ClearValue(FrameworkElement.WidthProperty);
    }

    private static ControlledTextInputState GetState(TextBox control)
    {
        return States.GetValue(control, _ => new ControlledTextInputState());
    }

    private static void SetText(TextBox control, string text)
    {
        var selection = TextSelectionRange.Capture(control);
        GetState(control).Apply(
            control.Text ?? string.Empty,
            text,
            value =>
            {
                control.Text = value;
                selection.Restore(control, value);
            });
    }
}
