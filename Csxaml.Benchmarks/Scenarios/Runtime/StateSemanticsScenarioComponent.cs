namespace Csxaml.Benchmarks;

internal sealed class StateSemanticsScenarioComponent : ComponentInstance
{
    public StateSemanticsScenarioComponent()
    {
        Version = new State<int>(0, InvalidateState, ValidateStateWrite);
    }

    public State<int> Version { get; }

    public void AssignSameValue()
    {
        Version.Value = 0;
    }

    public void TouchVersion()
    {
        Version.Touch();
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            key: null,
            properties:
            [
                new NativePropertyValue("Text", Version.Value.ToString()),
            ],
            events: Array.Empty<NativeEventValue>(),
            children: Array.Empty<Node>());
    }
}
