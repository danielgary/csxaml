namespace Csxaml.Generator;

internal sealed partial class Parser
{
    private static ComponentDefinition ParseApplicationDefinition(
        ParserContext context,
        int start,
        Token componentName,
        IReadOnlyList<ComponentParameter> parameters)
    {
        const string message = "invalid Application component declaration";
        var startup = ParseRequiredApplicationStatement(context, "startup");
        var resources = ParseOptionalApplicationStatement(context, "resources");
        var helperCode = ParseApplicationHelperCode(context);
        var closeBrace = context.ReadSymbol("}", message);
        var root = CreateApplicationPlaceholderRoot(componentName);

        return new ComponentDefinition(
            ComponentKind.Application,
            componentName.Text,
            parameters,
            Array.Empty<InjectFieldDefinition>(),
            Array.Empty<StateFieldDefinition>(),
            Array.Empty<RootPropertyDeclaration>(),
            new ApplicationRootDeclaration(
                startup.Identifier.Text,
                startup.Identifier.Span,
                resources?.Identifier.Text,
                resources?.Identifier.Span),
            helperCode,
            root,
            SupportsDefaultSlot(root),
            FindNamedSlots(root),
            new TextSpan(start, closeBrace.Span.End - start));
    }

    private static ApplicationStatement ParseRequiredApplicationStatement(
        ParserContext context,
        string statementName)
    {
        if (!context.PeekIdentifier(statementName))
        {
            throw context.CreateException($"missing '{statementName}' declaration in Application component");
        }

        return ParseApplicationStatement(context, statementName);
    }

    private static ApplicationStatement? ParseOptionalApplicationStatement(
        ParserContext context,
        string statementName)
    {
        return context.PeekIdentifier(statementName)
            ? ParseApplicationStatement(context, statementName)
            : null;
    }

    private static ApplicationStatement ParseApplicationStatement(
        ParserContext context,
        string statementName)
    {
        const string message = "invalid Application component declaration";
        context.ReadIdentifier(statementName, message);
        var identifier = context.ReadIdentifier(message);
        context.ReadSymbol(";", message);
        return new ApplicationStatement(identifier);
    }

    private static ComponentHelperCodeBlock? ParseApplicationHelperCode(ParserContext context)
    {
        if (context.PeekSymbol("}"))
        {
            return null;
        }

        var helperStart = context.Current.Span.Start;
        var bodyEnd = FindApplicationBodyEnd(context.Source, helperStart);
        var helperSpan = CSharpTextScanner.TrimWhitespaceSpan(context.Source, helperStart, bodyEnd);
        context.SkipToPosition(bodyEnd);
        return helperSpan is null
            ? null
            : new ComponentHelperCodeBlock(
                context.Source.Text.Substring(helperSpan.Value.Start, helperSpan.Value.Length),
                helperSpan.Value);
    }

    private static int FindApplicationBodyEnd(SourceDocument source, int start)
    {
        var depth = 0;
        for (var index = start; index < source.Text.Length; index++)
        {
            if (CSharpTextScanner.TrySkipCommentOrLiteral(source, index, out var nextIndex))
            {
                index = nextIndex - 1;
                continue;
            }

            if (source.Text[index] == '{')
            {
                depth++;
                continue;
            }

            if (source.Text[index] != '}')
            {
                continue;
            }

            if (depth == 0)
            {
                return index;
            }

            depth--;
        }

        throw DiagnosticFactory.FromPosition(source, start, "invalid Application component declaration");
    }

    private static MarkupNode CreateApplicationPlaceholderRoot(Token componentName)
    {
        return new MarkupNode(
            new MarkupTagName(
                "Application",
                null,
                "Application",
                componentName.Span),
            Array.Empty<PropertyNode>(),
            null,
            Array.Empty<PropertyContentNode>(),
            Array.Empty<ChildNode>(),
            componentName.Span);
    }

    private sealed record ApplicationStatement(Token Identifier);
}
