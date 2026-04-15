namespace Csxaml.Generator;

internal sealed class ComponentHelperCodeParser
{
    private readonly SourceDocument _source;
    private readonly RenderStatementLocator _renderStatementLocator;

    public ComponentHelperCodeParser(SourceDocument source)
    {
        _source = source;
        _renderStatementLocator = new RenderStatementLocator(source);
    }

    public ComponentHelperCodeBlock? Parse(ParserContext context)
    {
        if (context.PeekIdentifier("render"))
        {
            return null;
        }

        var helperStart = context.Current.Span.Start;
        var renderStart = _renderStatementLocator.Locate(helperStart);
        var helperSpan = CSharpTextScanner.TrimWhitespaceSpan(_source, helperStart, renderStart);
        context.SkipToPosition(renderStart);

        return helperSpan is null
            ? null
            : new ComponentHelperCodeBlock(
                _source.Text.Substring(helperSpan.Value.Start, helperSpan.Value.Length),
                helperSpan.Value);
    }
}
