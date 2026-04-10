namespace Csxaml.Generator;

internal sealed class Parser
{
    public ComponentDefinition Parse(SourceDocument source)
    {
        var context = new ParserContext(source, new Tokenizer().Tokenize(source));
        var childNodeParser = new ChildNodeParser(context);
        return ParseComponentDefinition(context, childNodeParser);
    }

    private static ComponentDefinition ParseComponentDefinition(
        ParserContext context,
        ChildNodeParser childNodeParser)
    {
        const string invalidDeclaration = "invalid component declaration";
        var start = context.ReadIdentifier("component", invalidDeclaration).Span.Start;
        context.ReadIdentifier("Element", invalidDeclaration);
        var componentName = context.ReadIdentifier(invalidDeclaration);
        var parameters = ParseParameters(context);
        context.ReadSymbol("{", invalidDeclaration);
        var stateFields = ParseStateFields(context);
        context.ReadIdentifier("return", "missing return block");
        var root = childNodeParser.ParseRootStackPanel();
        context.ReadSymbol(";", "missing return block");
        var closeBrace = context.ReadSymbol("}", invalidDeclaration);
        context.ReadEndOfFile("only one component per file is supported");

        return new ComponentDefinition(
            componentName.Text,
            parameters,
            stateFields,
            root,
            new TextSpan(start, closeBrace.Span.End - start));
    }

    private static IReadOnlyList<ComponentParameter> ParseParameters(ParserContext context)
    {
        const string message = "invalid component declaration";
        if (!context.TryReadSymbol("("))
        {
            return Array.Empty<ComponentParameter>();
        }

        var parameters = new List<ComponentParameter>();
        if (context.TryReadSymbol(")"))
        {
            return parameters;
        }

        do
        {
            parameters.Add(ParseParameter(context, message));
        }
        while (context.TryReadSymbol(","));

        context.ReadSymbol(")", message);
        return parameters;
    }

    private static ComponentParameter ParseParameter(ParserContext context, string message)
    {
        var typeStart = context.Current.Span.Start;
        var genericDepth = 0;
        Token? lastTypeToken = null;

        while (!IsParameterName(context, genericDepth))
        {
            var token = context.ReadCurrent(message);
            if (token.Kind == TokenKind.Symbol && token.Text == "<")
            {
                genericDepth++;
            }
            else if (token.Kind == TokenKind.Symbol && token.Text == ">")
            {
                genericDepth--;
            }

            lastTypeToken = token;
        }

        var name = context.ReadIdentifier(message);
        var typeName = context.Source.Text[typeStart..lastTypeToken!.Value.Span.End].Trim();
        return new ComponentParameter(
            typeName,
            name.Text,
            new TextSpan(typeStart, name.Span.End - typeStart));
    }

    private static IReadOnlyList<StateFieldDefinition> ParseStateFields(ParserContext context)
    {
        var stateFields = new List<StateFieldDefinition>();
        while (context.PeekIdentifier("State"))
        {
            stateFields.Add(ParseStateField(context));
        }

        return stateFields;
    }

    private static StateFieldDefinition ParseStateField(ParserContext context)
    {
        const string message = "invalid State declaration";
        var start = context.ReadIdentifier("State", message).Span.Start;
        context.ReadSymbol("<", message);
        var typeName = ReadTypeName(context, message);
        context.ReadSymbol(">", message);
        var fieldName = context.ReadIdentifier(message);
        context.ReadSymbol("=", message);
        context.ReadIdentifier("new", message);
        context.ReadIdentifier("State", message);
        context.ReadSymbol("<", message);
        var constructorTypeName = ReadTypeName(context, message);
        context.ReadSymbol(">", message);
        if (!string.Equals(typeName, constructorTypeName, StringComparison.Ordinal))
        {
            throw context.CreateException(fieldName.Span, message);
        }

        context.ReadSymbol("(", message);
        var initialValue = context.ReadRawUntilMatching(')', message).Trim();
        context.ReadSymbol(")", message);
        var semicolon = context.ReadSymbol(";", message);
        return new StateFieldDefinition(
            typeName,
            fieldName.Text,
            initialValue,
            new TextSpan(start, semicolon.Span.End - start));
    }

    private static bool IsParameterName(ParserContext context, int genericDepth)
    {
        return genericDepth == 0 &&
               context.Current.Kind == TokenKind.Identifier &&
               context.Peek(1).Kind == TokenKind.Symbol &&
               (context.Peek(1).Text == "," || context.Peek(1).Text == ")");
    }

    private static string ReadTypeName(ParserContext context, string message)
    {
        var depth = 0;
        var start = context.Current.Span.Start;
        Token? lastToken = null;

        while (!(context.PeekSymbol(">") && depth == 0))
        {
            var token = context.ReadCurrent(message);
            if (token.Kind == TokenKind.Symbol && token.Text == "<")
            {
                depth++;
            }
            else if (token.Kind == TokenKind.Symbol && token.Text == ">")
            {
                depth--;
            }

            lastToken = token;
        }

        if (lastToken is null)
        {
            throw context.CreateException(message);
        }

        return context.Source.Text[start..lastToken!.Value.Span.End].Trim();
    }
}
