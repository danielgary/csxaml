namespace Csxaml.Benchmarks;

internal static class RuntimeScenarioFactory
{
    // Runtime scenarios are handwritten ComponentInstance types on purpose.
    // They let us isolate reconciliation and state costs without mixing in
    // parser/generator work or live WinUI projection.
    public static RuntimeScenario<FlatListScenarioComponent> CreateFlatListScenario(int itemCount)
    {
        return new RuntimeScenario<FlatListScenarioComponent>(new FlatListScenarioComponent(itemCount));
    }

    public static RuntimeScenario<KeyedBoardScenarioComponent> CreateKeyedBoardScenario(int itemCount)
    {
        return new RuntimeScenario<KeyedBoardScenarioComponent>(new KeyedBoardScenarioComponent(itemCount));
    }

    public static RuntimeScenario<ListDetailScenarioComponent> CreateListDetailScenario(int itemCount)
    {
        return new RuntimeScenario<ListDetailScenarioComponent>(new ListDetailScenarioComponent(itemCount));
    }

    public static RuntimeScenario<StateSemanticsScenarioComponent> CreateStateSemanticsScenario()
    {
        return new RuntimeScenario<StateSemanticsScenarioComponent>(new StateSemanticsScenarioComponent());
    }
}
