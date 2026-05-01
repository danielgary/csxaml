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
        try
        {
            ApplyEvents((TControl)element, node, bindingStore);
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "event application",
                sourceInfo: node.SourceInfo,
                detail: TagName);
        }
    }

    public void ApplyProperties(object element, NativeElementNode node)
    {
        try
        {
            var control = (TControl)element;
            FrameworkElementLayoutPropertyApplicator.Apply(control, node);
            FrameworkElementStylePropertyApplicator.Apply(control, node);
            ApplyProperties(control, node);
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "property application",
                sourceInfo: node.SourceInfo,
                detail: TagName);
        }
    }

    public void SetChildren(object element, IReadOnlyList<object> children)
    {
        try
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
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "child assignment",
                sourceInfo: null,
                detail: TagName);
        }
    }

    public void SetPropertyContent(
        object element,
        IReadOnlyDictionary<string, IReadOnlyList<object>> propertyContent)
    {
        try
        {
            foreach (var entry in propertyContent)
            {
                NativePropertyContentSetter.Set(element, entry.Key, entry.Value);
            }
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "property-content assignment",
                sourceInfo: null,
                detail: TagName);
        }
    }

    protected virtual void ApplyEvents(
        TControl control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        CommonElementEventBinder.Apply(control, node, bindingStore);
    }

    protected abstract void ApplyProperties(TControl control, NativeElementNode node);

    protected abstract void SetChildren(TControl control, IReadOnlyList<UIElement> children);
}
