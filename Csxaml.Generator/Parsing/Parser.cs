namespace Csxaml.Generator;

internal sealed partial class Parser
{
    public CsxamlFileDefinition Parse(SourceDocument source)
    {
        var context = new ParserContext(source, new Tokenizer().Tokenize(source));
        var childNodeParser = new ChildNodeParser(context);
        var usingDirectives = new UsingDirectiveParser(context).ParseDirectives();
        var @namespace = new NamespaceDirectiveParser(context).ParseDirective();
        var fileScan = new FileMemberBoundaryScanner(source).Scan(context.Current.Span.Start);
        context.SkipToPosition(fileScan.ComponentSpan.Start);
        var component = ParseComponentDefinition(context, childNodeParser);
        return new CsxamlFileDefinition(
            usingDirectives,
            @namespace,
            fileScan.HelperCodeBlocks,
            component,
            new TextSpan(0, source.Text.Length));
    }

    private static ComponentDefinition ParseComponentDefinition(
        ParserContext context,
        ChildNodeParser childNodeParser)
    {
        const string invalidDeclaration = "invalid component declaration";
        const string missingRenderStatement = "missing final render statement in component body";
        var start = context.ReadIdentifier("component", invalidDeclaration).Span.Start;
        var kind = ParseComponentKind(context, invalidDeclaration);
        var componentName = context.ReadIdentifier(invalidDeclaration);
        var parameters = ParseParameters(context);
        context.ReadSymbol("{", invalidDeclaration);
        if (kind == ComponentKind.Application)
        {
            return ParseApplicationDefinition(context, start, componentName, parameters);
        }

        var injectFields = new List<InjectFieldDefinition>();
        var stateFields = new List<StateFieldDefinition>();
        ParsePrologueMembers(context, injectFields, stateFields);
        var rootProperties = ParseRootProperties(context, kind);
        var helperCode = new ComponentHelperCodeParser(context.Source).Parse(context);
        MisplacedComponentPrologueDetector.Validate(context.Source, helperCode);
        context.ReadIdentifier("render", missingRenderStatement);
        if (!context.PeekSymbol("<"))
        {
            throw context.CreateException("render statement must contain a single markup root");
        }

        var root = childNodeParser.ParseRootNode();
        if (context.PeekSymbol("<") || context.PeekIdentifier("if") || context.PeekIdentifier("foreach"))
        {
            throw context.CreateException("render statement must contain exactly one markup root");
        }

        context.ReadSymbol(";", "missing ';' after render statement");
        var closeBrace = context.ReadSymbol("}", invalidDeclaration);

        return new ComponentDefinition(
            kind,
            componentName.Text,
            parameters,
            injectFields,
            stateFields,
            rootProperties,
            null,
            helperCode,
            root,
            SupportsDefaultSlot(root),
            FindNamedSlots(root),
            new TextSpan(start, closeBrace.Span.End - start));
    }

    private static ComponentKind ParseComponentKind(ParserContext context, string message)
    {
        var token = context.ReadIdentifier(message);
        return token.Text switch
        {
            "Element" => ComponentKind.Element,
            "Page" => ComponentKind.Page,
            "Window" => ComponentKind.Window,
            "Application" => ComponentKind.Application,
            "ResourceDictionary" => ComponentKind.ResourceDictionary,
            _ => throw context.CreateException(token.Span, message)
        };
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

    private static void ParsePrologueMembers(
        ParserContext context,
        List<InjectFieldDefinition> injectFields,
        List<StateFieldDefinition> stateFields)
    {
        while (true)
        {
            if (context.PeekIdentifier("inject"))
            {
                injectFields.Add(ParseInjectField(context));
                continue;
            }

            if (context.PeekIdentifier("State"))
            {
                stateFields.Add(ParseStateField(context));
                continue;
            }

            break;
        }
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
        var initialValueStart = context.Current.Span.Start;
        var initialValue = context.ReadRawUntilMatching(')', message).Trim();
        context.ReadSymbol(")", message);
        var semicolon = context.ReadSymbol(";", message);
        return new StateFieldDefinition(
            typeName,
            fieldName.Text,
            initialValue,
            new TextSpan(initialValueStart, Math.Max(initialValue.Length, 0)),
            new TextSpan(start, semicolon.Span.End - start));
    }

    private static InjectFieldDefinition ParseInjectField(ParserContext context)
    {
        const string message = "invalid inject declaration";
        var start = context.ReadIdentifier("inject", message).Span.Start;
        var (typeName, fieldName) = ReadDeclarationTypeAndName(context, message, ";");
        var semicolon = context.ReadSymbol(";", message);
        return new InjectFieldDefinition(
            typeName,
            fieldName.Text,
            new TextSpan(start, semicolon.Span.End - start));
    }

    private static (string TypeName, Token Name) ReadDeclarationTypeAndName(
        ParserContext context,
        string message,
        string terminator)
    {
        var typeStart = context.Current.Span.Start;
        var genericDepth = 0;
        Token? lastTypeToken = null;

        while (!IsDeclarationName(context, genericDepth, terminator))
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
        return (typeName, name);
    }

    private static bool IsParameterName(ParserContext context, int genericDepth)
    {
        return IsDeclarationName(context, genericDepth, ",", ")");
    }

    private static bool IsDeclarationName(
        ParserContext context,
        int genericDepth,
        params string[] terminators)
    {
        return genericDepth == 0 &&
               context.Current.Kind == TokenKind.Identifier &&
               context.Peek(1).Kind == TokenKind.Symbol &&
               terminators.Contains(context.Peek(1).Text, StringComparer.Ordinal);
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
