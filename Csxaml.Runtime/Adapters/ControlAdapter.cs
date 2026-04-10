using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal abstract class ControlAdapter<TControl> : INativeControlAdapter
    where TControl : FrameworkElement, new()
{
    public abstract string TagName { get; }

    public object Create()
    {
        return new TControl();
    }

    public void ApplyEvents(
        object element,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        ApplyEvents((TControl)element, node, bindingStore);
    }

    public void ApplyProperties(object element, NativeElementNode node)
    {
        ApplyProperties((TControl)element, node);
    }

    public void SetChildren(object element, IReadOnlyList<object> children)
    {
        var uiChildren = new UIElement[children.Count];
        for (var index = 0; index < children.Count; index++)
        {
            if (children[index] is not UIElement uiElement)
            {
                throw new InvalidOperationException(
                    $"Child projection for '{TagName}' must be a UIElement.");
            }

            uiChildren[index] = uiElement;
        }

        SetChildren((TControl)element, uiChildren);
    }

    protected virtual void ApplyEvents(
        TControl control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        bindingStore.Clear();
    }

    protected abstract void ApplyProperties(TControl control, NativeElementNode node);

    protected abstract void SetChildren(TControl control, IReadOnlyList<UIElement> children);
}
