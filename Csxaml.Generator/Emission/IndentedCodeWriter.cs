using System.Text;

namespace Csxaml.Generator;

internal sealed class IndentedCodeWriter
{
    private readonly StringBuilder _builder = new();
    private readonly List<GeneratedSourceMapEntry> _sourceMapEntries = new();
    private int _indentLevel;
    private int _lineNumber = 1;

    public IReadOnlyList<GeneratedSourceMapEntry> SourceMapEntries => _sourceMapEntries;

    public void PopIndent()
    {
        _indentLevel--;
    }

    public void PushIndent()
    {
        _indentLevel++;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }

    public void WriteLine(string text = "")
    {
        if (text.Contains('\n') || text.Contains('\r'))
        {
            WriteBlock(text);
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            _builder.AppendLine();
            _lineNumber++;
            return;
        }

        _builder.Append(' ', _indentLevel * 4);
        _builder.AppendLine(text);
        _lineNumber++;
    }

    public void WriteMappedLine(
        string text,
        SourceDocument source,
        TextSpan span,
        string kind,
        string? label = null)
    {
        var generatedStartLine = _lineNumber;
        WriteLine(text);
        var generatedEndLine = _lineNumber - 1;
        AddSourceMapEntry(source, span, kind, label, generatedStartLine, generatedEndLine);
    }

    public void WriteBlock(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        foreach (var line in normalized.Split('\n'))
        {
            WriteLine(line);
        }
    }

    public void WriteMappedBlock(
        string text,
        SourceDocument source,
        TextSpan span,
        string kind,
        string? label = null)
    {
        var generatedStartLine = _lineNumber;
        WriteBlock(text);
        var generatedEndLine = _lineNumber - 1;
        AddSourceMapEntry(source, span, kind, label, generatedStartLine, generatedEndLine);
    }

    private void AddSourceMapEntry(
        SourceDocument source,
        TextSpan span,
        string kind,
        string? label,
        int generatedStartLine,
        int generatedEndLine)
    {
        var safeEnd = span.Length == 0 ? span.Start : Math.Max(span.Start, span.End - 1);
        var sourceStart = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, span.Start);
        var sourceEnd = SourceTextCoordinateConverter.GetLineAndColumn(source.Text, safeEnd);
        _sourceMapEntries.Add(
            new GeneratedSourceMapEntry(
                kind,
                label,
                generatedStartLine,
                generatedEndLine,
                sourceStart.Line,
                sourceStart.Column,
                sourceEnd.Line,
                sourceEnd.Column + 1));
    }
}
