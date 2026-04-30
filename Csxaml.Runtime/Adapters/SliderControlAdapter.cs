using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Csxaml.Runtime;

internal sealed class SliderControlAdapter : ControlAdapter<Slider>
{
    private static readonly ConditionalWeakTable<Slider, SliderInteractionState> States = new();

    public override string TagName => "Slider";

    protected override void ApplyEvents(
        Slider control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        GetState(control).EnsureDragEvents(control);
        CommonElementEventBinder.Apply(control, node, bindingStore);
        NativeEventArgsBinder.Rebind<RangeBaseValueChangedEventArgs>(
            node,
            bindingStore,
            "OnValueChanged",
            handler =>
            {
                RangeBaseValueChangedEventHandler typedHandler = (_, args) =>
                    GetState(control).RangeInput.Dispatch(args, handler);
                control.ValueChanged += typedHandler;
                return () => control.ValueChanged -= typedHandler;
            });
    }

    protected override void ApplyProperties(Slider control, NativeElementNode node)
    {
        GetState(control).RangeInput.Apply(
            () =>
            {
                ApplyMaximum(control, node);
                ApplyMinimum(control, node);
                ApplyValue(control, node);
            });
    }

    protected override void SetChildren(Slider control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("Slider does not support child elements.");
        }
    }

    private static void ApplyMaximum(Slider control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Maximum", out var maximum))
        {
            if (EqualityComparer<double>.Default.Equals(control.Maximum, maximum))
            {
                return;
            }

            control.Maximum = maximum;
            return;
        }

        ClearLocalValue(control, RangeBase.MaximumProperty);
    }

    private static void ApplyMinimum(Slider control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Minimum", out var minimum))
        {
            if (EqualityComparer<double>.Default.Equals(control.Minimum, minimum))
            {
                return;
            }

            control.Minimum = minimum;
            return;
        }

        ClearLocalValue(control, RangeBase.MinimumProperty);
    }

    private static void ApplyValue(Slider control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<double>(node, "Value", out var value))
        {
            if (EqualityComparer<double>.Default.Equals(control.Value, value))
            {
                return;
            }

            control.Value = value;
            return;
        }

        ClearLocalValue(control, RangeBase.ValueProperty);
    }

    private static SliderInteractionState GetState(Slider control)
    {
        return States.GetValue(control, _ => new SliderInteractionState());
    }

    private static void ClearLocalValue(Slider control, DependencyProperty property)
    {
        if (control.ReadLocalValue(property) == DependencyProperty.UnsetValue)
        {
            return;
        }

        control.ClearValue(property);
    }
}
