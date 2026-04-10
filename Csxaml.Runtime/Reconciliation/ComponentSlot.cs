namespace Csxaml.Runtime;

internal readonly record struct ComponentSlot(string MatchKey)
{
    public static ComponentSlot Create(
        ComponentNode node,
        Dictionary<string, int> slotOccurrences)
    {
        var slotKey = node.Key is null
            ? CreateUnkeyedSlotKey(node, slotOccurrences)
            : CreateKeyedSlotKey(node);

        return new ComponentSlot(slotKey);
    }

    private static string CreateKeyedSlotKey(ComponentNode node)
    {
        return $"{node.SlotName}|{node.ComponentType.FullName}|key:{node.Key}";
    }

    private static string CreateUnkeyedSlotKey(
        ComponentNode node,
        Dictionary<string, int> slotOccurrences)
    {
        var occurrence = slotOccurrences.TryGetValue(node.SlotName, out var current)
            ? current
            : 0;

        slotOccurrences[node.SlotName] = occurrence + 1;
        return $"{node.SlotName}|{node.ComponentType.FullName}|index:{occurrence}";
    }
}
