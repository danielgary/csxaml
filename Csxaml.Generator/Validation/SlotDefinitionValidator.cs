namespace Csxaml.Generator;

internal sealed class SlotDefinitionValidator
{
    public void Validate(SourceDocument source, ComponentDefinition definition)
    {
        ValidateRoot(source, definition.Root);

        var slotOutlets = new List<SlotOutletNode>();
        Collect(source, definition.Root, slotOutlets, insideForEach: false);

        var defaultSlots = slotOutlets
            .Where(slot => !slot.TryGetName(out _))
            .ToList();
        if (defaultSlots.Count > 1)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                defaultSlots[1].Span,
                $"component '{definition.Name}' declares more than one default slot");
        }

        ValidateDuplicateNamedSlots(source, definition, slotOutlets);
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
            : root is PropertyContentNode
                ? "property content cannot be the component root"
            : "components must render a single markup root node";

        throw DiagnosticFactory.FromSpan(source, root.Span, message);
    }

    private static void ValidateDuplicateNamedSlots(
        SourceDocument source,
        ComponentDefinition definition,
        IReadOnlyList<SlotOutletNode> slotOutlets)
    {
        var seenNames = new Dictionary<string, SlotOutletNode>(StringComparer.Ordinal);
        foreach (var slotOutlet in slotOutlets)
        {
            if (!slotOutlet.TryGetName(out var name))
            {
                continue;
            }

            if (seenNames.TryAdd(name, slotOutlet))
            {
                continue;
            }

            throw DiagnosticFactory.FromSpan(
                source,
                slotOutlet.Span,
                $"component '{definition.Name}' declares more than one named slot '{name}'");
        }
    }

    private static void ValidateSlotOutlet(SourceDocument source, SlotOutletNode slotOutlet)
    {
        foreach (var property in slotOutlet.Properties)
        {
            if (string.Equals(property.Name, "Name", StringComparison.Ordinal))
            {
                ValidateSlotName(source, property);
                continue;
            }

            throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                "slot does not support attributes other than Name");
        }

        if (slotOutlet.Children.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                slotOutlet.Span,
                "slot does not support child content");
        }
    }

    private static void ValidateSlotName(SourceDocument source, PropertyNode property)
    {
        if (property.ValueKind != PropertyValueKind.StringLiteral ||
            string.IsNullOrWhiteSpace(property.ValueText))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                "slot Name must be a non-empty string literal");
        }
    }

    private static void Collect(
        SourceDocument source,
        ChildNode node,
        List<SlotOutletNode> slotOutlets,
        bool insideForEach)
    {
        switch (node)
        {
            case SlotOutletNode slotOutlet:
                if (insideForEach)
                {
                    var slotKind = slotOutlet.TryGetName(out var name)
                        ? $"named slot '{name}'"
                        : "default slot";
                    throw DiagnosticFactory.FromSpan(
                        source,
                        slotOutlet.Span,
                        $"{slotKind} cannot appear inside foreach");
                }

                slotOutlets.Add(slotOutlet);
                return;

            case MarkupNode markupNode:
                foreach (var child in markupNode.Children)
                {
                    Collect(source, child, slotOutlets, insideForEach);
                }

                foreach (var propertyContent in markupNode.PropertyContent)
                {
                    foreach (var child in propertyContent.Children)
                    {
                        Collect(source, child, slotOutlets, insideForEach);
                    }
                }

                return;

            case IfBlockNode ifBlock:
                foreach (var child in ifBlock.Children)
                {
                    Collect(source, child, slotOutlets, insideForEach);
                }

                return;

            case ForEachBlockNode forEachBlock:
                foreach (var child in forEachBlock.Children)
                {
                    Collect(source, child, slotOutlets, insideForEach: true);
                }

                return;
        }
    }
}
