using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

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
        CommonElementEventBinder.Apply(control, node, bindingStore, bindKeyDown: false);
        BindKeyDown(control, node, bindingStore);
        NativeElementReader.TryGetEventHandler<Action<string>>(node, "OnTextChanged", out var onTextChanged);
        bindingStore.Rebind(
            "OnTextChanged",
            onTextChanged,
            handler =>
            {
                TextChangedEventHandler typedHandler = (_, _) =>
                {
                    var state = GetState(control);
                    try
                    {
                        NativeEventDispatchScope.Invoke(
                            () => state.Dispatch(control.Text ?? string.Empty, handler));
                    }
                    finally
                    {
                        state.ScheduleInputCompletion(control.DispatcherQueue);
                    }
                };

                control.TextChanged += typedHandler;
                return () => control.TextChanged -= typedHandler;
        });
    }

    private static void BindKeyDown(
        TextBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<KeyRoutedEventArgs>(
            node,
            bindingStore,
            "OnKeyDown",
            handler =>
            {
                KeyEventHandler typedHandler = (_, args) =>
                {
                    var state = GetState(control);
                    state.BeginInput(control.FocusState != FocusState.Unfocused);
                    try
                    {
                        handler(args);
                    }
                    finally
                    {
                        state.ScheduleInputCompletion(control.DispatcherQueue);
                    }
                };

                control.AddHandler(UIElement.KeyDownEvent, typedHandler, handledEventsToo: true);
                return () => control.RemoveHandler(UIElement.KeyDownEvent, typedHandler);
            });
    }

    protected override void ApplyProperties(TextBox control, NativeElementNode node)
    {
        var state = GetState(control);
        var restoreFocus = state.ConsumeFocusRestoreRequest() ||
            control.FocusState != FocusState.Unfocused;

        ApplyAcceptsReturn(control, node);
        ApplyMinHeight(control, node);
        ApplyPlaceholderText(control, node);
        ApplyText(control, node);
        ApplyTextWrapping(control, node);
        RestoreFocusAfterRender(control, restoreFocus);
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

    private static void RestoreFocusAfterRender(TextBox control, bool restoreFocus)
    {
        if (!restoreFocus)
        {
            return;
        }

        control.DispatcherQueue?.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            () =>
            {
                if (control.FocusState == FocusState.Unfocused && control.XamlRoot is not null)
                {
                    control.Focus(FocusState.Programmatic);
                }
            });
    }
}
