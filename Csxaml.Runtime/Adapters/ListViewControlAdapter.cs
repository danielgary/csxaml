using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.CompilerServices;

namespace Csxaml.Runtime;

internal sealed class ListViewControlAdapter : ControlAdapter<ListView>
{
    private static readonly ConditionalWeakTable<ListView, ListViewWheelScrollState> WheelScrollStates = new();

    public override string TagName => "ListView";

    protected override void ApplyEvents(
        ListView control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        WheelScrollStates.GetValue(control, _ => new ListViewWheelScrollState()).EnsureAttached(control);
        CommonElementEventBinder.Apply(control, node, bindingStore);
        BindItemClick(control, node, bindingStore);
        BindSelectionChanged(control, node, bindingStore);
    }

    protected override void ApplyProperties(ListView control, NativeElementNode node)
    {
        ApplyIsItemClickEnabled(control, node);
        ApplyItemsSource(control, node);
        ApplySelectionMode(control, node);
    }

    protected override void SetChildren(ListView control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("ListView does not support child elements.");
        }
    }

    private static void BindItemClick(
        ListView control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<ItemClickEventArgs>(
            node,
            bindingStore,
            "OnItemClick",
            handler =>
            {
                ItemClickEventHandler typedHandler = (_, args) => handler(args);
                control.ItemClick += typedHandler;
                return () => control.ItemClick -= typedHandler;
            });
    }

    private static void BindSelectionChanged(
        ListView control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<SelectionChangedEventArgs>(
            node,
            bindingStore,
            "OnSelectionChanged",
            handler =>
            {
                SelectionChangedEventHandler typedHandler = (_, args) => handler(args);
                control.SelectionChanged += typedHandler;
                return () => control.SelectionChanged -= typedHandler;
            });
    }

    private static void ApplyIsItemClickEnabled(ListView control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<bool>(node, "IsItemClickEnabled", out var isItemClickEnabled))
        {
            if (control.IsItemClickEnabled == isItemClickEnabled)
            {
                return;
            }

            control.IsItemClickEnabled = isItemClickEnabled;
            return;
        }

        ClearLocalValue(control, ListViewBase.IsItemClickEnabledProperty);
    }

    private static void ApplyItemsSource(ListView control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "ItemsSource", out var itemsSource))
        {
            if (ReferenceEquals(control.ItemsSource, itemsSource))
            {
                return;
            }

            control.ItemsSource = itemsSource;
            return;
        }

        if (control.ItemsSource is null)
        {
            return;
        }

        control.ItemsSource = null;
    }

    private static void ApplySelectionMode(ListView control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ListViewSelectionMode>(node, "SelectionMode", out var selectionMode))
        {
            if (control.SelectionMode == selectionMode)
            {
                return;
            }

            control.SelectionMode = selectionMode;
            return;
        }

        ClearLocalValue(control, ListView.SelectionModeProperty);
    }

    private static void ClearLocalValue(ListView control, DependencyProperty property)
    {
        if (control.ReadLocalValue(property) == DependencyProperty.UnsetValue)
        {
            return;
        }

        control.ClearValue(property);
    }
}
