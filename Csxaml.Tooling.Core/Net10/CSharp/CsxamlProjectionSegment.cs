namespace Csxaml.Tooling.Core.CSharp;

internal sealed record CsxamlProjectionSegment(
    int OriginalStart,
    int ProjectedStart,
    int Length)
{
    public int OriginalEnd => OriginalStart + Length;

    public int ProjectedEnd => ProjectedStart + Length;

    public bool ContainsOriginal(int position)
    {
        return position >= OriginalStart && position <= OriginalEnd;
    }

    public bool ContainsProjected(int position)
    {
        return position >= ProjectedStart && position <= ProjectedEnd;
    }
}
