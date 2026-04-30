using Csxaml.ControlMetadata;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ExternalEventBinderTests
{
    [TestMethod]
    public void Bind_EventArgsHandler_ProjectsSenderlessArgsAndUnbinds()
    {
        var source = new TestEventSource();
        var binder = ExternalEventBinder.Create(
            typeof(TestEventSource),
            new EventMetadata(
                "Changed",
                "OnChanged",
                $"System.Action<{typeof(TestChangedEventArgs).FullName}>",
                true,
                ValueKindHint.Unknown,
                EventBindingKind.EventArgs));
        var store = new NativeEventBindingStore();
        var observed = new List<string>();

        store.Rebind<Action<TestChangedEventArgs>>(
            "OnChanged",
            args => observed.Add(args.Value),
            handler => binder.Bind(source, handler));
        source.Raise("first");

        store.Rebind<Action<TestChangedEventArgs>>(
            "OnChanged",
            args => observed.Add($"updated:{args.Value}"),
            handler => binder.Bind(source, handler));
        source.Raise("second");

        store.Rebind<Action<TestChangedEventArgs>>(
            "OnChanged",
            null,
            handler => binder.Bind(source, handler));
        source.Raise("ignored");

        CollectionAssert.AreEqual(
            new[] { "first", "updated:second" },
            observed);
    }

    private sealed class TestEventSource
    {
        public event EventHandler<TestChangedEventArgs>? Changed;

        public void Raise(string value)
        {
            Changed?.Invoke(this, new TestChangedEventArgs(value));
        }
    }

    private sealed class TestChangedEventArgs(string value) : EventArgs
    {
        public string Value { get; } = value;
    }
}
