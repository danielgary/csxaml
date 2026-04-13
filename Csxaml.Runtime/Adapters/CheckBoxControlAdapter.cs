using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Csxaml.Runtime;

internal sealed class CheckBoxControlAdapter : ControlAdapter<CheckBox>
{
    private static readonly ConditionalWeakTable<CheckBox, ControlledBoolInputState> States = new();

    public override string TagName => "CheckBox";

    protected override void ApplyEvents(
        CheckBox control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeElementReader.TryGetEventHandler<Action<bool>>(node, "OnCheckedChanged", out var onCheckedChanged);
        bindingStore.Rebind(
            "OnCheckedChanged",
            onCheckedChanged,
            handler =>
            {
                RoutedEventHandler checkedHandler = (_, _) => InvokeHandler(control, handler);
                RoutedEventHandler uncheckedHandler = (_, _) => InvokeHandler(control, handler);
                RoutedEventHandler indeterminateHandler = (_, _) => InvokeHandler(control, handler);

                control.Checked += checkedHandler;
                control.Unchecked += uncheckedHandler;
                control.Indeterminate += indeterminateHandler;

                return () =>
                {
                    control.Checked -= checkedHandler;
                    control.Unchecked -= uncheckedHandler;
                    control.Indeterminate -= indeterminateHandler;
                };
            });
    }

    protected override void ApplyProperties(CheckBox control, NativeElementNode node)
    {
        ApplyContent(control, node);
        ApplyIsChecked(control, node);
    }

    protected override void SetChildren(CheckBox control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("CheckBox does not support child elements.");
        }
    }

    private static void ApplyContent(CheckBox control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "Content", out var content))
        {
            control.Content = content;
            return;
        }

        control.ClearValue(ContentControl.ContentProperty);
    }

    private static void ApplyIsChecked(CheckBox control, NativeElementNode node)
    {
        if (!NativeElementReader.TryGetPropertyValue<bool?>(node, "IsChecked", out var isChecked))
        {
            SetIsChecked(control, false);
            return;
        }

        SetIsChecked(control, Normalize(isChecked));
    }

    private static ControlledBoolInputState GetState(CheckBox control)
    {
        return States.GetValue(control, _ => new ControlledBoolInputState());
    }

    private static void InvokeHandler(CheckBox control, Action<bool> handler)
    {
        GetState(control).Dispatch(Normalize(control.IsChecked), handler);
    }

    private static bool Normalize(bool? value)
    {
        return value ?? false;
    }

    private static void SetIsChecked(CheckBox control, bool value)
    {
        GetState(control).Apply(Normalize(control.IsChecked), value, checkedValue => control.IsChecked = checkedValue);
    }
}
