namespace Csxaml.Generator;

internal sealed class ChildNodeEmitter
{
    private readonly CompilationContext _compilation;
    private readonly ParsedComponent _component;
    private readonly MarkupTagResolver _tagResolver = new();
    private readonly LocalNameGenerator _localNameGenerator = new();
    private readonly NativeAttributeEmitter _nativeAttributeEmitter;
    private readonly RenderPositionIdGenerator _renderPositionIdGenerator = new();
    private readonly IndentedCodeWriter _writer;

    public ChildNodeEmitter(
        IndentedCodeWriter writer,
        ParsedComponent component,
        CompilationContext compilation)
    {
        _writer = writer;
        _component = component;
        _compilation = compilation;
        _nativeAttributeEmitter = new NativeAttributeEmitter(
            component.Source,
            component.Definition.Name,
            new AttachedPropertyBindingResolver(component));
    }

    public void EmitRenderBody(ChildNode root)
    {
        if (root is not MarkupNode markupRoot)
        {
            throw new InvalidOperationException(
                "Components must emit a single markup root node.");
        }

        EmitMarkupNodeDeclaration(markupRoot, "rootNode");
        _writer.WriteLine("return rootNode;");
    }

    private string BuildComponentPropsExpression(
        MarkupNode markupNode,
        ComponentCatalogEntry component)
    {
        if (component.Parameters.Count == 0)
        {
            return "null";
        }

        var arguments = component.Parameters
            .Select(parameter => FormatComponentArgument(GetRequiredProperty(markupNode, parameter.Name)))
            .ToList();

        return
            $$"""
            new {{FormatTypeLiteral(component.PropsTypeName!)}}
            (
            {{FormatArgumentList(arguments)}}
            )
            """;
    }

    private void EmitChildStatements(IReadOnlyList<ChildNode> childNodes, string listName)
    {
        foreach (var childNode in childNodes)
        {
            switch (childNode)
            {
                case ForEachBlockNode forEachBlock:
                    EmitForEachBlock(forEachBlock, listName);
                    break;

                case IfBlockNode ifBlock:
                    EmitIfBlock(ifBlock, listName);
                    break;

                case MarkupNode markupNode:
                    EmitMarkupNodeAppend(markupNode, listName);
                    break;

                case SlotOutletNode slotOutlet:
                    EmitSlotOutlet(slotOutlet, listName);
                    break;
            }
        }
    }

    private void EmitComponentNodeDeclaration(
        MarkupNode markupNode,
        ComponentCatalogEntry component,
        string variableName)
    {
        var propsExpression = BuildComponentPropsExpression(markupNode, component);
        var childContentExpression = BuildChildContentExpression(markupNode.Children);
        var attachedPropertiesExpression = _nativeAttributeEmitter.BuildAttachedPropertiesExpression(markupNode);
        var keyExpression = _nativeAttributeEmitter.BuildKeyExpression(markupNode);
        var renderPositionId = _renderPositionIdGenerator.Next();
        var sourceInfoExpression = SourceInfoEmitter.Emit(
            _component.Source,
            _component.Definition.Name,
            markupNode.Span,
            tagName: markupNode.TagName);

        var declaration =
            $$"""
            var {{variableName}} = new ComponentNode(
            {{FormatArgumentList(
                [
                    $"typeof({FormatTypeLiteral(component.ComponentTypeName)})",
                    propsExpression,
                    childContentExpression,
                    attachedPropertiesExpression,
                    $"\"{renderPositionId}\"",
                    keyExpression,
                    sourceInfoExpression
                ])}}
            );
            """;

        _writer.WriteMappedBlock(
            declaration,
            _component.Source,
            markupNode.Span,
            "component-usage",
            markupNode.TagName);
    }

    private void EmitForEachBlock(ForEachBlockNode forEachBlock, string listName)
    {
        var header =
            $$"""
            foreach (var {{forEachBlock.ItemName}} in
            {{CodeBlockFormatter.Indent(LineDirectiveFormatter.Wrap(_component.Source, forEachBlock.CollectionSpan, forEachBlock.CollectionExpression), 4)}}
                )
            {
            """;

        _writer.WriteMappedBlock(
            header,
            _component.Source,
            forEachBlock.Span,
            "foreach-block",
            forEachBlock.ItemName);
        _writer.PushIndent();
        EmitChildStatements(forEachBlock.Children, listName);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitIfBlock(IfBlockNode ifBlock, string listName)
    {
        var header =
            $$"""
            if (
            {{CodeBlockFormatter.Indent(LineDirectiveFormatter.Wrap(_component.Source, ifBlock.ConditionSpan, ifBlock.ConditionExpression), 4)}}
                )
            {
            """;

        _writer.WriteMappedBlock(
            header,
            _component.Source,
            ifBlock.Span,
            "if-block",
            "if");
        _writer.PushIndent();
        EmitChildStatements(ifBlock.Children, listName);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitMarkupNodeAppend(MarkupNode markupNode, string listName)
    {
        var variableName = _localNameGenerator.Next("childNode");
        EmitMarkupNodeDeclaration(markupNode, variableName);
        _writer.WriteLine($"{listName}.Add({variableName});");
    }

    private void EmitSlotOutlet(SlotOutletNode slotOutlet, string listName)
    {
        _writer.WriteMappedLine(
            $"{listName}.AddRange(ChildContent);",
            _component.Source,
            slotOutlet.Span,
            "slot-outlet",
            "Slot");
    }

    private void EmitMarkupNodeDeclaration(MarkupNode markupNode, string variableName)
    {
        var resolvedTag = _tagResolver.Resolve(
            _component.Source,
            _component,
            markupNode,
            _compilation);
        if (resolvedTag.Kind == ResolvedTagKind.Native)
        {
            EmitNativeNodeDeclaration(markupNode, resolvedTag.NativeControl!, resolvedTag.RuntimeTagName, variableName);
            return;
        }

        EmitComponentNodeDeclaration(markupNode, resolvedTag.Component!, variableName);
    }

    private void EmitNativeNodeDeclaration(MarkupNode markupNode, string variableName)
    {
        throw new InvalidOperationException(
            "Native node emission requires resolved control metadata.");
    }

    private void EmitNativeNodeDeclaration(
        MarkupNode markupNode,
        ControlMetadataModel control,
        string runtimeTagName,
        string variableName)
    {
        var keyExpression = _nativeAttributeEmitter.BuildKeyExpression(markupNode);
        var propertiesExpression = _nativeAttributeEmitter.BuildPropertiesExpression(markupNode, control);
        var attachedPropertiesExpression = _nativeAttributeEmitter.BuildAttachedPropertiesExpression(markupNode);
        var eventsExpression = _nativeAttributeEmitter.BuildEventsExpression(markupNode, control);
        var childrenExpression = "Array.Empty<Node>()";

        if (markupNode.Children.Count > 0)
        {
            var childrenVariableName = _localNameGenerator.Next("children");
            _writer.WriteLine($"var {childrenVariableName} = new List<Node>();");
            EmitChildStatements(markupNode.Children, childrenVariableName);
            childrenExpression = childrenVariableName;
        }

        var sourceInfoExpression = SourceInfoEmitter.Emit(
            _component.Source,
            _component.Definition.Name,
            markupNode.Span,
            tagName: markupNode.TagName);
        var declaration =
            $$"""
            var {{variableName}} = new NativeElementNode(
            {{FormatArgumentList(
                [
                    $"\"{runtimeTagName}\"",
                    keyExpression,
                    propertiesExpression,
                    attachedPropertiesExpression,
                    eventsExpression,
                    childrenExpression,
                    sourceInfoExpression
                ])}}
            );
            """;

        _writer.WriteMappedBlock(
            declaration,
            _component.Source,
            markupNode.Span,
            "native-tag",
            markupNode.TagName);
    }

    private string BuildChildContentExpression(IReadOnlyList<ChildNode> childNodes)
    {
        if (childNodes.Count == 0)
        {
            return "Array.Empty<Node>()";
        }

        var childContentName = _localNameGenerator.Next("childContent");
        _writer.WriteLine($"var {childContentName} = new List<Node>();");
        EmitChildStatements(childNodes, childContentName);
        return childContentName;
    }

    private string FormatComponentArgument(PropertyNode property)
    {
        return property.ValueKind == PropertyValueKind.StringLiteral
            ? $"\"{EscapeString(property.ValueText)}\""
            : LineDirectiveFormatter.Wrap(_component.Source, property.ValueSpan, property.ValueText);
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string FormatTypeLiteral(string clrTypeName)
    {
        return $"global::{clrTypeName.Replace("+", ".", StringComparison.Ordinal)}";
    }

    private static string FormatArgumentList(IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>(arguments.Count);
        for (var index = 0; index < arguments.Count; index++)
        {
            lines.Add(
                index == arguments.Count - 1
                    ? CodeBlockFormatter.FormatLastArgument(arguments[index], 4)
                    : CodeBlockFormatter.FormatArgument(arguments[index], 4));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static PropertyNode GetRequiredProperty(MarkupNode node, string propertyName)
    {
        return node.Properties.Single(property => property.Name == propertyName);
    }
}
