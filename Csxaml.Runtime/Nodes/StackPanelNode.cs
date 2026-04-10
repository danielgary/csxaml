namespace Csxaml.Runtime;

public sealed record StackPanelNode(IReadOnlyList<Node> Children) : NativeNode;
