namespace Csxaml.Generator;

internal sealed class RenderPositionIdGenerator
{
    private int _nextValue;

    public string Next()
    {
        return $"position{_nextValue++}";
    }
}
