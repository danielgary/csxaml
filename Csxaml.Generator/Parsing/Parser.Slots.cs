namespace Csxaml.Generator;

internal sealed partial class Parser
{
    private static bool SupportsDefaultSlot(ChildNode node)
    {
        if (node is SlotOutletNode slotOutlet)
        {
            return !slotOutlet.TryGetName(out _);
        }

        return node switch
        {
            MarkupNode markup => MarkupSupportsDefaultSlot(markup),
            IfBlockNode ifBlock => ifBlock.Children.Any(SupportsDefaultSlot),
            ForEachBlockNode forEachBlock => forEachBlock.Children.Any(SupportsDefaultSlot),
            _ => false
        };
    }

    private static bool MarkupSupportsDefaultSlot(MarkupNode markup)
    {
        return markup.Children.Any(SupportsDefaultSlot) ||
            markup.PropertyContent.Any(property => property.Children.Any(SupportsDefaultSlot));
    }

    private static IReadOnlyList<string> FindNamedSlots(ChildNode node)
    {
        var slots = new List<string>();
        CollectNamedSlots(node, slots);
        return slots;
    }

    private static void CollectNamedSlots(ChildNode node, List<string> slots)
    {
        switch (node)
        {
            case SlotOutletNode slotOutlet when slotOutlet.TryGetName(out var name):
                slots.Add(name);
                return;
            case MarkupNode markup:
                CollectNamedSlots(markup, slots);
                return;
            case IfBlockNode ifBlock:
                CollectNamedSlots(ifBlock.Children, slots);
                return;
            case ForEachBlockNode forEachBlock:
                CollectNamedSlots(forEachBlock.Children, slots);
                return;
        }
    }

    private static void CollectNamedSlots(MarkupNode markup, List<string> slots)
    {
        CollectNamedSlots(markup.Children, slots);
        foreach (var propertyContent in markup.PropertyContent)
        {
            CollectNamedSlots(propertyContent.Children, slots);
        }
    }

    private static void CollectNamedSlots(IReadOnlyList<ChildNode> nodes, List<string> slots)
    {
        foreach (var node in nodes)
        {
            CollectNamedSlots(node, slots);
        }
    }
}
