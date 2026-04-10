namespace Csxaml.Runtime;

public sealed record ButtonNode(string Content, Action OnClick) : NativeNode;
