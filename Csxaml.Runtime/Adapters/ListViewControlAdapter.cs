using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

internal sealed class ListViewControlAdapter : ControlAdapter<ListView>
{
    public override string TagName => "ListView";

    protected override void ApplyEvents(
        ListView control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
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
            control.IsItemClickEnabled = isItemClickEnabled;
            return;
        }

        control.ClearValue(ListViewBase.IsItemClickEnabledProperty);
    }

    private static void ApplyItemsSource(ListView control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<object?>(node, "ItemsSource", out var itemsSource))
        {
            control.ItemsSource = itemsSource;
            return;
        }

        control.ItemsSource = null;
    }

    private static void ApplySelectionMode(ListView control, NativeElementNode node)
    {
        if (NativeElementReader.TryGetPropertyValue<ListViewSelectionMode>(node, "SelectionMode", out var selectionMode))
        {
            control.SelectionMode = selectionMode;
            return;
        }

        control.ClearValue(ListView.SelectionModeProperty);
    }
}
