using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal sealed class ExternalControlAdapter : INativeControlAdapter
{
    private readonly ExternalChildSetter _childSetter;
    private readonly IReadOnlyList<ExternalEventBinder> _eventBinders;
    private readonly IReadOnlyList<ExternalPropertyAccessor> _propertyAccessors;
    private readonly ExternalControlDescriptor _descriptor;

    public ExternalControlAdapter(ExternalControlDescriptor descriptor)
    {
        _descriptor = descriptor;
        _childSetter = ExternalChildSetter.Create(descriptor);
        _propertyAccessors = descriptor.Metadata.Properties
            .Select(property => ExternalPropertyAccessor.Create(descriptor.ControlType, property))
            .ToList();
        _eventBinders = descriptor.Metadata.Events
            .Select(eventMetadata => ExternalEventBinder.Create(descriptor.ControlType, eventMetadata))
            .ToList();
    }

    public string TagName => _descriptor.TagName;

    public object Create()
    {
        return Activator.CreateInstance(_descriptor.ControlType) as FrameworkElement ??
            throw new InvalidOperationException(
                $"External control '{_descriptor.ControlType.FullName}' must create a FrameworkElement instance.");
    }

    public void ApplyEvents(
        object element,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        foreach (var eventBinder in _eventBinders)
        {
            NativeElementReader.TryGetEventHandler<Action>(node, eventBinder.ExposedName, out var handler);
            bindingStore.Rebind(
                eventBinder.ExposedName,
                handler,
                action => eventBinder.Bind(element, action));
        }
    }

    public void ApplyProperties(object element, NativeElementNode node)
    {
        if (element is not DependencyObject)
        {
            throw new InvalidOperationException(
                $"External control '{TagName}' must create a DependencyObject.");
        }

        foreach (var propertyAccessor in _propertyAccessors)
        {
            propertyAccessor.Apply(element, node);
        }
    }

    public void SetChildren(object element, IReadOnlyList<object> children)
    {
        if (element is not FrameworkElement frameworkElement)
        {
            throw new InvalidOperationException(
                $"External control '{TagName}' must create a FrameworkElement.");
        }

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

        _childSetter.Set(frameworkElement, uiChildren);
    }
}
