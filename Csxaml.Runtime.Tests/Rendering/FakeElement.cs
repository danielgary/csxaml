namespace Csxaml.Runtime.Tests.Rendering;

internal sealed class FakeElement
{
    public FakeElement(string tagName)
    {
        TagName = tagName;
    }

    public List<FakeElement> Children { get; } = [];

    public Dictionary<string, Action> Events { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, object?> Properties { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, List<FakeElement>> PropertyContent { get; } = new(StringComparer.Ordinal);

    public string TagName { get; }
}
