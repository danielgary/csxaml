namespace Csxaml.Runtime;

public sealed record ComponentNode(
    Type ComponentType,
    object? Props,
    string SlotName,
    string? Key) : Node;
