namespace Csxaml.Runtime;

public sealed record NativeElementNode(
    string TagName,
    string? Key,
    IReadOnlyList<NativePropertyValue> Properties,
    IReadOnlyList<NativeEventValue> Events,
    IReadOnlyList<Node> Children) : NativeNode;
