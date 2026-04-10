namespace Csxaml.Generator;

internal sealed class LocalNameGenerator
{
    private int _nextValue;

    public string Next(string prefix)
    {
        return $"{prefix}{_nextValue++}";
    }
}
