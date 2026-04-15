namespace Csxaml.Runtime;

internal sealed class NullServiceProvider : IServiceProvider
{
    public static NullServiceProvider Instance { get; } = new();

    private NullServiceProvider()
    {
    }

    public object? GetService(Type serviceType)
    {
        return null;
    }
}
