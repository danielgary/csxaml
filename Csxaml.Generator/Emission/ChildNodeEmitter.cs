using System.Globalization;

namespace Csxaml.Generator;

internal sealed class ChildNodeEmitter
{
    private readonly CompilationContext _compilation;
    private readonly ParsedComponent _component;
    private readonly MarkupTagResolver _tagResolver = new();
    private readonly LocalNameGenerator _localNameGenerator = new();
    private readonly NativeAttributeEmitter _nativeAttributeEmitter = new();
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
            .Select(parameter => FormatValue(GetRequiredProperty(markupNode, parameter.Name)));

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new {FormatTypeLiteral(component.PropsTypeName!)}({string.Join(", ", arguments)})");
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

                case SlotOutletNode:
                    EmitSlotOutlet(listName);
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
        var hasAttachedProperties = _nativeAttributeEmitter.HasAttachedProperties(markupNode);
        var renderPositionId = _renderPositionIdGenerator.Next();

        _writer.WriteLine(
            hasAttachedProperties
                ? $"var {variableName} = new ComponentNode(typeof({FormatTypeLiteral(component.ComponentTypeName)}), {propsExpression}, {childContentExpression}, {attachedPropertiesExpression}, \"{renderPositionId}\", {keyExpression});"
                : $"var {variableName} = new ComponentNode(typeof({FormatTypeLiteral(component.ComponentTypeName)}), {propsExpression}, {childContentExpression}, \"{renderPositionId}\", {keyExpression});");
    }

    private void EmitForEachBlock(ForEachBlockNode forEachBlock, string listName)
    {
        _writer.WriteLine($"foreach (var {forEachBlock.ItemName} in {forEachBlock.CollectionExpression})");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitChildStatements(forEachBlock.Children, listName);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitIfBlock(IfBlockNode ifBlock, string listName)
    {
        _writer.WriteLine($"if ({ifBlock.ConditionExpression})");
        _writer.WriteLine("{");
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

    private void EmitSlotOutlet(string listName)
    {
        _writer.WriteLine($"{listName}.AddRange(ChildContent);");
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
        var hasAttachedProperties = _nativeAttributeEmitter.HasAttachedProperties(markupNode);
        var childrenExpression = "Array.Empty<Node>()";

        if (markupNode.Children.Count > 0)
        {
            var childrenVariableName = _localNameGenerator.Next("children");
            _writer.WriteLine($"var {childrenVariableName} = new List<Node>();");
            EmitChildStatements(markupNode.Children, childrenVariableName);
            childrenExpression = childrenVariableName;
        }

        _writer.WriteLine(
            hasAttachedProperties
                ? $"var {variableName} = new NativeElementNode(\"{runtimeTagName}\", {keyExpression}, {propertiesExpression}, {attachedPropertiesExpression}, {eventsExpression}, {childrenExpression});"
                : $"var {variableName} = new NativeElementNode(\"{runtimeTagName}\", {keyExpression}, {propertiesExpression}, {eventsExpression}, {childrenExpression});");
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

    private static string FormatValue(PropertyNode property)
    {
        return property.ValueKind == PropertyValueKind.StringLiteral
            ? $"\"{EscapeString(property.ValueText)}\""
            : property.ValueText;
    }

    private static PropertyNode GetRequiredProperty(MarkupNode node, string propertyName)
    {
        return node.Properties.Single(property => property.Name == propertyName);
    }
}
