namespace Csxaml.Runtime;

/// <summary>
/// Resolves required services for generated component injection members.
/// </summary>
public static class InjectedServiceResolver
{
    /// <summary>
    /// Resolves a required service or throws a CSXAML runtime exception with source context.
    /// </summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <param name="component">The component that owns the injected member.</param>
    /// <param name="services">The service provider used for resolution.</param>
    /// <param name="memberName">The component member that receives the service.</param>
    /// <param name="sourceInfo">Source-location metadata for the injection site, when available.</param>
    /// <returns>The resolved service instance.</returns>
    /// <exception cref="CsxamlRuntimeException">Thrown when the service cannot be resolved.</exception>
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
