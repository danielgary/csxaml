namespace Csxaml.Generator;

internal sealed class SlotDefinitionValidator
{
    public void Validate(SourceDocument source, ComponentDefinition definition)
    {
        ValidateRoot(source, definition.Root);

        var slotOutlets = new List<SlotOutletNode>();
        Collect(definition.Root, slotOutlets);

        if (slotOutlets.Count > 1)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                slotOutlets[1].Span,
                $"component '{definition.Name}' declares more than one default slot");
        }

        foreach (var slotOutlet in slotOutlets)
        {
            ValidateSlotOutlet(source, slotOutlet);
        }
    }

    private static void ValidateRoot(SourceDocument source, ChildNode root)
    {
        if (root is MarkupNode)
        {
            return;
        }

        var message = root is SlotOutletNode
            ? "default slot cannot be the component root"
            : "components must render a single markup root node";

        throw DiagnosticFactory.FromSpan(source, root.Span, message);
    }

    private static void ValidateSlotOutlet(SourceDocument source, SlotOutletNode slotOutlet)
    {
        foreach (var property in slotOutlet.Properties)
        {
            if (string.Equals(property.Name, "Name", StringComparison.Ordinal))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    "named slots are not supported yet");
            }

            throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                "default slot does not support attributes");
        }

        if (slotOutlet.Children.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                slotOutlet.Span,
                "default slot does not support child content");
        }
    }

    private static void Collect(ChildNode node, List<SlotOutletNode> slotOutlets)
    {
        switch (node)
        {
            case SlotOutletNode slotOutlet:
                slotOutlets.Add(slotOutlet);
                return;

            case MarkupNode markupNode:
                foreach (var child in markupNode.Children)
                {
                    Collect(child, slotOutlets);
                }

                return;

            case IfBlockNode ifBlock:
                foreach (var child in ifBlock.Children)
                {
                    Collect(child, slotOutlets);
                }

                return;

            case ForEachBlockNode forEachBlock:
                foreach (var child in forEachBlock.Children)
                {
                    Collect(child, slotOutlets);
                }

                return;
        }
    }
}
