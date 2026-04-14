namespace Csxaml.Runtime;

internal readonly record struct ComponentMatchKey(string Value)
{
    public static ComponentMatchKey Create(
        ComponentNode node,
        Dictionary<string, int> positionOccurrences)
    {
        var value = node.Key is null
            ? CreateUnkeyedValue(node, positionOccurrences)
            : CreateKeyedValue(node);

        return new ComponentMatchKey(value);
    }

    private static string CreateKeyedValue(ComponentNode node)
    {
        return $"{node.RenderPositionId}|{node.ComponentType.FullName}|key:{node.Key}";
    }

    private static string CreateUnkeyedValue(
        ComponentNode node,
        Dictionary<string, int> positionOccurrences)
    {
        var occurrence = positionOccurrences.TryGetValue(node.RenderPositionId, out var current)
            ? current
            : 0;

        positionOccurrences[node.RenderPositionId] = occurrence + 1;
        return $"{node.RenderPositionId}|{node.ComponentType.FullName}|index:{occurrence}";
    }
}
