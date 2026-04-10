using System.Globalization;

namespace Csxaml.Generator;

internal sealed class ChildNodeEmitter
{
    private readonly ComponentCatalog _catalog;
    private readonly SlotNameGenerator _slotNameGenerator = new();
    private readonly IndentedCodeWriter _writer;

    public ChildNodeEmitter(IndentedCodeWriter writer, ComponentCatalog catalog)
    {
        _writer = writer;
        _catalog = catalog;
    }

    public void EmitRenderBody(MarkupNode root)
    {
        _writer.WriteLine("var children = new List<Node>();");
        EmitChildStatements(root.Children, "children");
        _writer.WriteLine("return new StackPanelNode(children);");
    }

    private void EmitButtonNode(MarkupNode markupNode, string listName)
    {
        var content = FormatValue(GetRequiredProperty(markupNode, "Content"));
        var onClick = FormatValue(GetRequiredProperty(markupNode, "OnClick"));
        _writer.WriteLine($"{listName}.Add(new ButtonNode({content}, {onClick}));");
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
                    EmitMarkupNode(markupNode, listName);
                    break;
            }
        }
    }

    private void EmitComponentNode(MarkupNode markupNode, string listName)
    {
        var key = markupNode.Properties.SingleOrDefault(property => property.Name == "Key");
        var propsExpression = BuildPropsExpression(markupNode);
        var keyExpression = key is null ? "null" : FormatValue(key);
        var slotName = _slotNameGenerator.Next();

        _writer.WriteLine(
            $"{listName}.Add(new ComponentNode(typeof({markupNode.TagName}Component), {propsExpression}, \"{slotName}\", {keyExpression}));");
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

    private void EmitMarkupNode(MarkupNode markupNode, string listName)
    {
        if (string.Equals(markupNode.TagName, "Button", StringComparison.Ordinal))
        {
            EmitButtonNode(markupNode, listName);
            return;
        }

        if (string.Equals(markupNode.TagName, "TextBlock", StringComparison.Ordinal))
        {
            EmitTextBlockNode(markupNode, listName);
            return;
        }

        EmitComponentNode(markupNode, listName);
    }

    private void EmitTextBlockNode(MarkupNode markupNode, string listName)
    {
        var text = FormatValue(GetRequiredProperty(markupNode, "Text"));
        _writer.WriteLine($"{listName}.Add(new TextBlockNode({text}));");
    }

    private string BuildPropsExpression(MarkupNode markupNode)
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
