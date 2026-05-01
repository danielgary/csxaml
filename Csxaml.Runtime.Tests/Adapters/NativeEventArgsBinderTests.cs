namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class NativeEventArgsBinderTests
{
    [TestMethod]
    public void Rebind_EventArgsHandler_PassesArgsAndUnbindsPreviousHandler()
    {
        var store = new NativeEventBindingStore();
        var observed = new List<string>();
        var unbindCount = 0;
        Action<TestChangedEventArgs>? activeHandler = null;

        NativeEventArgsBinder.Rebind<TestChangedEventArgs>(
            CreateNode(args => observed.Add(args.Value)),
            store,
            "OnChanged",
            handler =>
            {
                activeHandler = handler;
                return () =>
                {
                    unbindCount++;
                    if (ReferenceEquals(activeHandler, handler))
                    {
                        activeHandler = null;
                    }
                };
            });
        activeHandler?.Invoke(new TestChangedEventArgs("first"));

        NativeEventArgsBinder.Rebind<TestChangedEventArgs>(
            CreateNode(args => observed.Add($"updated:{args.Value}")),
            store,
            "OnChanged",
            handler =>
            {
                activeHandler = handler;
                return () =>
                {
                    unbindCount++;
                    if (ReferenceEquals(activeHandler, handler))
                    {
                        activeHandler = null;
                    }
                };
            });
        activeHandler?.Invoke(new TestChangedEventArgs("second"));

        NativeEventArgsBinder.Rebind<TestChangedEventArgs>(
            CreateNode(null),
            store,
            "OnChanged",
            handler =>
            {
                activeHandler = handler;
                return () =>
                {
                    unbindCount++;
                    activeHandler = null;
                };
            });

        Assert.IsNull(activeHandler);
        Assert.AreEqual(2, unbindCount);
        CollectionAssert.AreEqual(
            new[] { "first", "updated:second" },
            observed);
    }

    [TestMethod]
    public void Rebind_SameHandler_DoesNotUnbindAndRebind()
    {
        var store = new NativeEventBindingStore();
        var bindCount = 0;
        var unbindCount = 0;
        Action<TestChangedEventArgs> handler = _ => { };

        store.Rebind(
            "OnChanged",
            handler,
            _ =>
            {
                bindCount++;
                return () => unbindCount++;
            });
        store.Rebind(
            "OnChanged",
            handler,
            _ =>
            {
                bindCount++;
                return () => unbindCount++;
            });

        Assert.AreEqual(1, bindCount);
        Assert.AreEqual(0, unbindCount);
    }

    [TestMethod]
    public void Rebind_EventArgsHandler_DispatchesInsideNativeEventScope()
    {
        var store = new NativeEventBindingStore();
        var observedScopes = new List<bool>();
        Action<TestChangedEventArgs>? activeHandler = null;

        NativeEventArgsBinder.Rebind<TestChangedEventArgs>(
            CreateNode(_ => observedScopes.Add(NativeEventDispatchScope.IsActive)),
            store,
            "OnChanged",
            handler =>
            {
                activeHandler = handler;
                return () => activeHandler = null;
            });

        Assert.IsFalse(NativeEventDispatchScope.IsActive);
        activeHandler?.Invoke(new TestChangedEventArgs("first"));
        Assert.IsFalse(NativeEventDispatchScope.IsActive);
        CollectionAssert.AreEqual(new[] { true }, observedScopes);
    }

    private static NativeElementNode CreateNode(Action<TestChangedEventArgs>? handler)
    {
        var events = handler is null
            ? Array.Empty<NativeEventValue>()
            : [new NativeEventValue("OnChanged", handler, ValueKindHint.Unknown)];
        return new NativeElementNode(
            "TestElement",
            null,
            Array.Empty<NativePropertyValue>(),
            events,
            Array.Empty<Node>());
    }

    private sealed class TestChangedEventArgs(string value)
    {
        public string Value { get; } = value;
    }
}
