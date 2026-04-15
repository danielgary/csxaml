namespace Csxaml.Runtime;

internal static class ComponentDisposer
{
    public static void Dispose(ComponentInstance component)
    {
        if (!component.TryBeginDispose())
        {
            return;
        }

        component.ChildComponents.DisposeAll();

        switch (component)
        {
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    public static async ValueTask DisposeAsync(ComponentInstance component)
    {
        if (!component.TryBeginDispose())
        {
            return;
        }

        await component.ChildComponents.DisposeAllAsync();

        switch (component)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
