using System.Globalization;

namespace Csxaml.Generator;

internal sealed class ChildNodeEmitter
{
    private readonly ComponentCatalog _catalog;
    private readonly LocalNameGenerator _localNameGenerator = new();
    private readonly NativeAttributeEmitter _nativeAttributeEmitter = new();
    private readonly SlotNameGenerator _slotNameGenerator = new();
    private readonly IndentedCodeWriter _writer;

    public ChildNodeEmitter(IndentedCodeWriter writer, ComponentCatalog catalog)
    {
        _writer = writer;
        _catalog = catalog;
    }

    public void EmitRenderBody(MarkupNode root)
    {
        EmitMarkupNodeDeclaration(root, "rootNode");
        _writer.WriteLine("return rootNode;");
    }

    private string BuildComponentPropsExpression(MarkupNode markupNode)
    {
        var component = _catalog.GetComponent(markupNode.TagName);
        if (component.Parameters.Count == 0)
        {
            return "null";
        }

        var arguments = component.Parameters
            .Select(parameter => FormatValue(GetRequiredProperty(markupNode, parameter.Name)));

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new {markupNode.TagName}Props({string.Join(", ", arguments)})");
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
            }
        }
    }

    private void EmitComponentNodeDeclaration(MarkupNode markupNode, string variableName)
    {
        var propsExpression = BuildComponentPropsExpression(markupNode);
        var keyExpression = _nativeAttributeEmitter.BuildKeyExpression(markupNode);
        var slotName = _slotNameGenerator.Next();

        _writer.WriteLine(
            $"var {variableName} = new ComponentNode(typeof({markupNode.TagName}Component), {propsExpression}, \"{slotName}\", {keyExpression});");
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

    private void EmitMarkupNodeDeclaration(MarkupNode markupNode, string variableName)
    {
        if (ControlMetadataRegistry.IsNativeTag(markupNode.TagName))
        {
            EmitNativeNodeDeclaration(markupNode, variableName);
            return;
        }

        EmitComponentNodeDeclaration(markupNode, variableName);
    }

    private void EmitNativeNodeDeclaration(MarkupNode markupNode, string variableName)
    {
        var keyExpression = _nativeAttributeEmitter.BuildKeyExpression(markupNode);
        var propertiesExpression = _nativeAttributeEmitter.BuildPropertiesExpression(markupNode);
        var eventsExpression = _nativeAttributeEmitter.BuildEventsExpression(markupNode);
        var childrenExpression = "Array.Empty<Node>()";

        if (markupNode.Children.Count > 0)
        {
            var childrenVariableName = _localNameGenerator.Next("children");
            _writer.WriteLine($"var {childrenVariableName} = new List<Node>();");
            EmitChildStatements(markupNode.Children, childrenVariableName);
            childrenExpression = childrenVariableName;
        }

        _writer.WriteLine(
            $"var {variableName} = new NativeElementNode(\"{markupNode.TagName}\", {keyExpression}, {propertiesExpression}, {eventsExpression}, {childrenExpression});");
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
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
