namespace Csxaml.Generator;

internal sealed class ComponentHelperCodeParser
{
    private readonly SourceDocument _source;
    private readonly RenderReturnLocator _returnLocator;

    public ComponentHelperCodeParser(SourceDocument source)
    {
        _source = source;
        _returnLocator = new RenderReturnLocator(source);
    }

    public ComponentHelperCodeBlock? Parse(ParserContext context)
    {
        if (context.PeekIdentifier("return"))
        {
            return null;
        }

        var helperStart = context.Current.Span.Start;
        var returnStart = _returnLocator.Locate(helperStart);
        var helperSpan = CSharpTextScanner.TrimWhitespaceSpan(_source, helperStart, returnStart);
        context.SkipToPosition(returnStart);

        return helperSpan is null
            ? null
            : new ComponentHelperCodeBlock(
                _source.Text.Substring(helperSpan.Value.Start, helperSpan.Value.Length),
                helperSpan.Value);
    }
}
