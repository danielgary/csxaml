namespace Csxaml.Runtime.Tests.Rendering;

internal sealed class FakeControlAdapter : INativeControlAdapter
{
    public FakeControlAdapter(string tagName, bool supportsChildren = true)
    {
        TagName = tagName;
        SupportsChildren = supportsChildren;
    }

    public bool SupportsChildren { get; }

    public string TagName { get; }

    public void ApplyEvents(
        object element,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        var fakeElement = RequireElement(element);
        NativeElementReader.TryGetEventHandler<Action>(node, "OnClick", out var onClick);
        bindingStore.Rebind(
            "OnClick",
            onClick,
            handler =>
            {
                fakeElement.Events["OnClick"] = handler;
                return () => fakeElement.Events.Remove("OnClick");
            });
    }

    public void ApplyProperties(object element, NativeElementNode node)
    {
        var fakeElement = RequireElement(element);
        fakeElement.Properties.Clear();

        foreach (var property in node.Properties)
        {
            fakeElement.Properties[property.Name] = property.Value;
        }
    }

    public object Create()
    {
        return new FakeElement(TagName);
    }

    public void SetChildren(object element, IReadOnlyList<object> children)
    {
        var fakeElement = RequireElement(element);
        if (!SupportsChildren && children.Count > 0)
        {
            throw new InvalidOperationException($"{TagName} does not support child elements.");
        }

        fakeElement.Children.Clear();
        foreach (var child in children)
        {
            fakeElement.Children.Add(RequireElement(child));
        }
    }

    private static FakeElement RequireElement(object element)
    {
        return element as FakeElement ??
            throw new InvalidOperationException("Fake control adapters require fake elements.");
    }
}
