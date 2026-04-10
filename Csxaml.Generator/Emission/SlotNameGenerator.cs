namespace Csxaml.Generator;

internal sealed class SlotNameGenerator
{
    private int _nextValue;

    public string Next()
    {
        return $"slot{_nextValue++}";
    }
}
