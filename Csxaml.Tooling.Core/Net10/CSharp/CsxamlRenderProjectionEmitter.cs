using Csxaml.ControlMetadata;
using Csxaml.Generator;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlRenderProjectionEmitter
{
    private readonly string _currentNamespace;
    private readonly SourceDocument _source;
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly IReadOnlyList<CsxamlUsingDirectiveInfo> _usingDirectives;
    private readonly CsxamlWorkspaceSnapshot _workspace;
    private readonly CsxamlProjectionWriter _writer;
    private int _expressionIndex;

    public CsxamlRenderProjectionEmitter(
        SourceDocument source,
        CsxamlProjectionWriter writer,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        _source = source;
        _writer = writer;
        _usingDirectives = usingDirectives;
        _currentNamespace = currentNamespace;
        _workspace = workspace;
    }

    public void Emit(ChildNode node)
    {
        switch (node)
        {
            case ForEachBlockNode forEachBlock:
                EmitForEachBlock(forEachBlock);
                break;
            case IfBlockNode ifBlock:
                EmitIfBlock(ifBlock);
                break;
            case MarkupNode markupNode:
                EmitMarkupNode(markupNode);
                break;
            case SlotOutletNode slotOutletNode:
                EmitSlotOutletNode(slotOutletNode);
                break;
        }
    }

    private void EmitForEachBlock(ForEachBlockNode node)
    {
        var collectionSpan = CsxamlExpressionSpanLocator.GetForEachCollectionSpan(_source, node);
        _writer.AppendSynthetic($"foreach (var {node.ItemName} in ");
        AppendMappedSpan(collectionSpan);
        _writer.AppendSynthetic(")\n{\n");
        EmitChildren(node.Children);
        _writer.AppendSynthetic("}\n");
    }

    private void EmitIfBlock(IfBlockNode node)
    {
        var conditionSpan = CsxamlExpressionSpanLocator.GetIfConditionSpan(_source, node);
        _writer.AppendSynthetic("if (");
        AppendMappedSpan(conditionSpan);
        _writer.AppendSynthetic(")\n{\n");
        EmitChildren(node.Children);
        _writer.AppendSynthetic("}\n");
    }

    private void EmitMarkupNode(MarkupNode node)
    {
        var resolvedTag = _tagResolver.Resolve(node.TagName, _usingDirectives, _currentNamespace, _workspace);
        EmitRefExpression(node);
        EmitPropertyExpressions(node.Properties, resolvedTag);
        EmitChildren(node.Children);
    }

    private void EmitRefExpression(MarkupNode node)
    {
        if (node.Ref?.ValueKind != PropertyValueKind.Expression)
        {
            return;
        }

        EmitPropertyExpression(
            node.Ref.ValueSpan,
            CsxamlProjectedPropertyType.Plain("Csxaml.Runtime.ElementRef"));
    }

    private void EmitSlotOutletNode(SlotOutletNode node)
    {
        foreach (var property in node.Properties.Where(property => property.ValueKind == PropertyValueKind.Expression))
        {
            EmitPropertyExpression(
                CsxamlExpressionSpanLocator.GetPropertyExpressionSpan(_source, property),
                CsxamlProjectedPropertyType.Plain("object"));
        }

        EmitChildren(node.Children);
    }

    private void EmitChildren(IReadOnlyList<ChildNode> children)
    {
        foreach (var child in children)
        {
            Emit(child);
        }
    }

    private void EmitPropertyExpressions(
        IReadOnlyList<PropertyNode> properties,
        CsxamlResolvedTag resolvedTag)
    {
        foreach (var property in properties.Where(property => property.ValueKind == PropertyValueKind.Expression))
        {
            EmitPropertyExpression(
                CsxamlExpressionSpanLocator.GetPropertyExpressionSpan(_source, property),
                ResolvePropertyType(property, resolvedTag));
        }
    }

    private void EmitPropertyExpression(TextSpan span, CsxamlProjectedPropertyType propertyType)
    {
        _writer.AppendSynthetic(
            $"{CsxamlProjectionTypeNameFormatter.Format(propertyType.TypeName)} __csxamlExpr{_expressionIndex++} = ");
        if (propertyType.CoercionMethodName is null)
        {
            AppendMappedSpan(span);
        }
        else
        {
            _writer.AppendSynthetic($"{propertyType.CoercionMethodName}(");
            AppendMappedSpan(span);
            _writer.AppendSynthetic(")");
        }

        _writer.AppendSynthetic(";\n");
    }

    private CsxamlProjectedPropertyType ResolvePropertyType(PropertyNode property, CsxamlResolvedTag resolvedTag)
    {
        if (property.Name == "Key")
        {
            return CsxamlProjectedPropertyType.Plain("object");
        }

        if (property.IsAttached &&
            CsxamlAttachedPropertyResolver.TryResolve(
                property,
                _usingDirectives,
                _currentNamespace,
                out var attachedProperty))
        {
            return CreateProjectedPropertyType(attachedProperty!.ClrTypeName, attachedProperty.ValueKindHint);
        }

        if (resolvedTag.Control is not null)
        {
            var eventMetadata = resolvedTag.Control.Events.FirstOrDefault(@event => @event.ExposedName == property.Name);
            if (eventMetadata is not null)
            {
                return CsxamlProjectedPropertyType.Plain(eventMetadata.HandlerTypeName);
            }

            var propertyMetadata = resolvedTag.Control.Properties.FirstOrDefault(candidate => candidate.Name == property.Name);
            if (propertyMetadata is not null)
            {
                return CreateProjectedPropertyType(propertyMetadata.ClrTypeName, propertyMetadata.ValueKindHint);
            }
        }

        if (resolvedTag.Component is not null)
        {
            var parameter = resolvedTag.Component.Metadata.Parameters.FirstOrDefault(candidate => candidate.Name == property.Name);
            if (parameter is not null)
            {
                return CsxamlProjectedPropertyType.Plain(parameter.TypeName);
            }
        }

        return CsxamlProjectedPropertyType.Plain("object");
    }

    private static CsxamlProjectedPropertyType CreateProjectedPropertyType(string clrTypeName, ValueKindHint valueKindHint)
    {
        return valueKindHint switch
        {
            ValueKindHint.Brush => new CsxamlProjectedPropertyType(clrTypeName, "__CsxamlProjectionToBrush"),
            ValueKindHint.Style => new CsxamlProjectedPropertyType(clrTypeName, "__CsxamlProjectionToStyle"),
            ValueKindHint.Thickness => new CsxamlProjectedPropertyType(clrTypeName, "__CsxamlProjectionToThickness"),
            _ => CsxamlProjectedPropertyType.Plain(clrTypeName)
        };
    }

    private void AppendMappedSpan(TextSpan span)
    {
        _writer.AppendMapped(_source.Text.Substring(span.Start, span.Length), span.Start);
    }
}
