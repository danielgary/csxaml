namespace Csxaml.Runtime;

internal static class NativeEventArgsBinder
{
    public static void Rebind<TArgs>(
        NativeElementNode node,
        NativeEventBindingStore bindingStore,
        string name,
        Func<Action<TArgs>, Action> bind)
        where TArgs : class
    {
        NativeElementReader.TryGetEventHandler<Action<TArgs>>(node, name, out var handler);
        bindingStore.Rebind(
            name,
            handler,
            currentHandler =>
            {
                void ScopedHandler(TArgs args)
                {
                    NativeEventDispatchScope.Invoke(() => currentHandler(args));
                }

                return bind(ScopedHandler);
            });
    }
}
