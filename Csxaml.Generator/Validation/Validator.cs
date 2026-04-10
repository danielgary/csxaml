namespace Csxaml.Generator;

internal sealed class Validator
{
    public ComponentCatalog Validate(IReadOnlyList<ParsedComponent> components)
    {
        var catalog = BuildCatalog(components);
        foreach (var component in components)
        {
            ValidateComponent(component, catalog);
        }

        return catalog;
    }

    private static ComponentCatalog BuildCatalog(IReadOnlyList<ParsedComponent> components)
    {
        var entries = new Dictionary<string, ParsedComponent>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            if (!entries.TryAdd(component.Definition.Name, component))
            {
                throw DiagnosticFactory.FromSpan(
                    component.Source,
                    component.Definition.Span,
                    $"component '{component.Definition.Name}' is defined more than once");
            }
        }

        return new ComponentCatalog(entries);
    }

    private static void ValidateChildComponent(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalog catalog)
    {
        if (!catalog.Contains(node.TagName))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"unsupported tag name '{node.TagName}'");
        }

        if (node.Children.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"unsupported tag name '{node.TagName}'");
        }

        var definition = catalog.GetComponent(node.TagName);
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in node.Properties)
        {
            if (!propertyNames.Add(property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"unsupported prop name '{property.Name}'");
            }

            if (property.Name == "Key")
            {
                continue;
            }

            if (definition.Parameters.All(parameter => parameter.Name != property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"prop validation failure: unsupported prop name '{property.Name}' on component '{node.TagName}'");
            }
        }

        foreach (var parameter in definition.Parameters)
        {
            if (node.Properties.All(property => property.Name != parameter.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    node.Span,
                    $"prop validation failure: missing required prop '{parameter.Name}' on component '{node.TagName}'");
            }
        }
    }

    private static void ValidateChildlessNode(SourceDocument source, MarkupNode node)
    {
        if (node.Children.Count == 0)
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Span,
            $"unsupported tag name '{node.TagName}'");
    }

    private static void ValidateChildNodes(
        SourceDocument source,
        IReadOnlyList<ChildNode> childNodes,
        ComponentCatalog catalog)
    {
        foreach (var childNode in childNodes)
        {
            switch (childNode)
            {
                case ForEachBlockNode forEachBlock:
                    ValidateChildNodes(source, forEachBlock.Children, catalog);
                    break;

                case IfBlockNode ifBlock:
                    ValidateChildNodes(source, ifBlock.Children, catalog);
                    break;

                case MarkupNode markupNode:
                    ValidateMarkupNode(source, markupNode, catalog);
                    break;
            }
        }
    }

    private static void ValidateComponent(
        ParsedComponent component,
        ComponentCatalog catalog)
    {
        ValidateUniqueNames(
            component.Source,
            component.Definition.Parameters.Select(parameter => (parameter.Name, parameter.Span)));

        ValidateUniqueNames(
            component.Source,
            component.Definition.StateFields.Select(field => (field.Name, field.Span)));

        ValidateMarkupNode(component.Source, component.Definition.Root, catalog, isRoot: true);
    }

    private static void ValidateMarkupNode(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalog catalog,
        bool isRoot = false)
    {
        if (node.TagName == "Button")
        {
            ValidatePropertySet(source, node, "Content", "OnClick");
            ValidateChildlessNode(source, node);
            return;
        }

        if (node.TagName == "StackPanel")
        {
            if (!isRoot)
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    node.Span,
                    "unsupported tag name 'StackPanel'");
            }

            if (node.Properties.Count > 0)
            {
                throw DiagnosticFactory.FromSpan(source, node.Span, "unsupported prop name");
            }

            ValidateChildNodes(source, node.Children, catalog);
            return;
        }

        if (node.TagName == "TextBlock")
        {
            ValidatePropertySet(source, node, "Text");
            ValidateChildlessNode(source, node);
            return;
        }

        ValidateChildComponent(source, node, catalog);
    }

    private static void ValidatePropertySet(
        SourceDocument source,
        MarkupNode node,
        params string[] expectedPropertyNames)
    {
        var expected = expectedPropertyNames.ToHashSet(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in node.Properties)
        {
            if (!seen.Add(property.Name) || !expected.Contains(property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"unsupported prop name '{property.Name}'");
            }
        }

        if (seen.Count != expected.Count)
        {
            throw DiagnosticFactory.FromSpan(source, node.Span, "unsupported prop name");
        }
    }

    private static void ValidateUniqueNames(
        SourceDocument source,
        IEnumerable<(string Name, TextSpan Span)> entries)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!names.Add(entry.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    entry.Span,
                    $"duplicate name '{entry.Name}'");
            }
        }
    }
}
