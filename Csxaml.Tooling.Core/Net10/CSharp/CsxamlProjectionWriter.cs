using System.Text;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlProjectionWriter
{
    private readonly StringBuilder _builder = new();
    private readonly List<CsxamlProjectionSegment> _segments = [];

    public void AppendMapped(string text, int originalStart)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var projectedStart = _builder.Length;
        _builder.Append(text);
        AddSegment(new CsxamlProjectionSegment(originalStart, projectedStart, text.Length));
    }

    public void AppendSynthetic(string text)
    {
        _builder.Append(text);
    }

    public CsxamlProjectedDocument Build()
    {
        return new CsxamlProjectedDocument(_builder.ToString(), _segments.ToList());
    }

    private void AddSegment(CsxamlProjectionSegment segment)
    {
        if (_segments.Count > 0)
        {
            var previous = _segments[^1];
            if (previous.OriginalEnd == segment.OriginalStart &&
                previous.ProjectedEnd == segment.ProjectedStart)
            {
                _segments[^1] = previous with { Length = previous.Length + segment.Length };
                return;
            }
        }

        _segments.Add(segment);
    }
}
