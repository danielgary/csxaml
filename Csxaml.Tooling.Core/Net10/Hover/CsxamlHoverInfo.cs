namespace Csxaml.Tooling.Core.Hover;

public sealed record CsxamlHoverInfo(
    int Start,
    int Length,
    string Markdown);
