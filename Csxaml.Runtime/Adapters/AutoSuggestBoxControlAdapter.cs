using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Csxaml.Runtime;

internal sealed class AutoSuggestBoxControlAdapter : ControlAdapter<AutoSuggestBox>
{
    public override string TagName => "AutoSuggestBox";

    protected override void ApplyEvents(
        AutoSuggestBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        CommonElementEventBinder.Apply(control, node, bindingStore);
        BindQuerySubmitted(control, node, bindingStore);
        BindSuggestionChosen(control, node, bindingStore);
    }

    protected override void ApplyProperties(AutoSuggestBox control, NativeElementNode node)
    {
        ApplyPlaceholderText(control, node);
        ApplyText(control, node);
    }

    protected override void SetChildren(AutoSuggestBox control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("AutoSuggestBox does not support child elements.");
        }
    }

    private static void BindQuerySubmitted(
        AutoSuggestBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<AutoSuggestBoxQuerySubmittedEventArgs>(
            node,
            bindingStore,
            "OnQuerySubmitted",
            handler =>
            {
                TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> typedHandler =
                    (_, args) => handler(args);
                control.QuerySubmitted += typedHandler;
                return () => control.QuerySubmitted -= typedHandler;
            });
    }

    private static void BindSuggestionChosen(
        AutoSuggestBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<AutoSuggestBoxSuggestionChosenEventArgs>(
            node,
            bindingStore,
            "OnSuggestionChosen",
            handler =>
            {
                TypedEventHandler<AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs> typedHandler =
                    (_, args) => handler(args);
                control.SuggestionChosen += typedHandler;
                return () => control.SuggestionChosen -= typedHandler;
            });
    }

    private static void ApplyPlaceholderText(AutoSuggestBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<string?>(node, "PlaceholderText", out var placeholderText))
        {
            control.PlaceholderText = placeholderText ?? string.Empty;
            return;
        }

        control.ClearValue(AutoSuggestBox.PlaceholderTextProperty);
    }

    private static void ApplyText(AutoSuggestBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<string?>(node, "Text", out var text))
        {
            control.Text = text ?? string.Empty;
            return;
        }

        control.ClearValue(AutoSuggestBox.TextProperty);
    }
}
