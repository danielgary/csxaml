namespace Csxaml.Runtime;

public static class InjectedServiceResolver
{
    public static T ResolveRequired<T>(
        ComponentInstance component,
        IServiceProvider services,
        string memberName,
        CsxamlSourceInfo? sourceInfo)
    {
        if (services.GetService(typeof(T)) is T value)
        {
            return value;
        }

        throw CsxamlRuntimeExceptionBuilder.Wrap(
            new InvalidOperationException(
                $"Failed to resolve required service '{typeof(T).FullName ?? typeof(T).Name}' for '{component.CsxamlComponentName}.{memberName}'."),
            "service resolution",
            component,
            sourceInfo,
            $"{component.CsxamlComponentName}.{memberName}");
    }
}
