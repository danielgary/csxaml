namespace Csxaml.Generator;

internal sealed partial class ChildNodeEmitter
{
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

    private string BuildNamedSlotContentExpression(IReadOnlyList<PropertyContentNode> propertyContentNodes)
    {
        if (propertyContentNodes.Count == 0)
        {
            return "new Dictionary<string, IReadOnlyList<Node>>(StringComparer.Ordinal)";
        }

        var namedSlotsName = _localNameGenerator.Next("namedSlots");
        _writer.WriteLine($"var {namedSlotsName} = new Dictionary<string, IReadOnlyList<Node>>(StringComparer.Ordinal);");
        foreach (var propertyContent in propertyContentNodes)
        {
            var contentExpression = BuildChildContentExpression(propertyContent.Children);
            _writer.WriteLine($"{namedSlotsName}[\"{EscapeString(propertyContent.PropertyName)}\"] = {contentExpression};");
        }

        return namedSlotsName;
    }

    private string BuildNativePropertyContentExpression(IReadOnlyList<PropertyContentNode> propertyContentNodes)
    {
        if (propertyContentNodes.Count == 0)
        {
            return "Array.Empty<NativePropertyContentValue>()";
        }

        var propertyContentName = _localNameGenerator.Next("propertyContent");
        _writer.WriteLine($"var {propertyContentName} = new List<NativePropertyContentValue>();");
        foreach (var propertyContent in propertyContentNodes)
        {
            var childrenExpression = BuildChildContentExpression(propertyContent.Children);
            var sourceInfoExpression = SourceInfoEmitter.Emit(
                _component.Source,
                _component.Definition.Name,
                propertyContent.Span,
                tagName: propertyContent.Name);
            _writer.WriteLine(
                $"{propertyContentName}.Add(new NativePropertyContentValue(\"{EscapeString(propertyContent.PropertyName)}\", {childrenExpression}, {sourceInfoExpression}));");
        }

        return propertyContentName;
    }
}
