namespace Csxaml.Runtime;

internal interface INativeControlAdapter
{
    string TagName { get; }

    object Create();

    void ApplyProperties(object element, NativeElementNode node);

    void ApplyEvents(
        object element,
        NativeElementNode node,
        NativeEventBindingStore bindingStore);

    void SetChildren(object element, IReadOnlyList<object> children);

    void SetPropertyContent(
        object element,
        IReadOnlyDictionary<string, IReadOnlyList<object>> propertyContent);
}
