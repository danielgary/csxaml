namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlProjectedDocument
{
    public CsxamlProjectedDocument(string text, IReadOnlyList<CsxamlProjectionSegment> segments)
    {
        Text = text;
        Segments = segments;
    }

    public IReadOnlyList<CsxamlProjectionSegment> Segments { get; }

    public string Text { get; }

    public bool TryMapOriginalToProjected(int originalPosition, out int projectedPosition)
    {
        foreach (var segment in Segments)
        {
            if (!segment.ContainsOriginal(originalPosition))
            {
                continue;
            }

            projectedPosition = segment.ProjectedStart + (originalPosition - segment.OriginalStart);
            return true;
        }

        projectedPosition = -1;
        return false;
    }

    public bool TryMapProjectedToOriginal(int projectedPosition, out int originalPosition)
    {
        foreach (var segment in Segments)
        {
            if (!segment.ContainsProjected(projectedPosition))
            {
                continue;
            }

            originalPosition = segment.OriginalStart + (projectedPosition - segment.ProjectedStart);
            return true;
        }

        originalPosition = -1;
        return false;
    }

    public bool TryMapProjectedRange(int projectedStart, int projectedEnd, out int originalStart, out int originalEnd)
    {
        if (!TryMapProjectedToOriginal(projectedStart, out originalStart) ||
            !TryMapProjectedToOriginal(projectedEnd, out originalEnd))
        {
            originalStart = -1;
            originalEnd = -1;
            return false;
        }

        return true;
    }
}
