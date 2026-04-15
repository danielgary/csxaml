namespace Csxaml.Runtime;

internal static class NativeElementReader
{
    public static bool TryGetEventHandler<TDelegate>(
        NativeElementNode node,
        string name,
        out TDelegate? handler)
        where TDelegate : Delegate
    {
        foreach (var eventValue in node.Events)
        {
            if (!string.Equals(eventValue.Name, name, StringComparison.Ordinal))
            {
                continue;
            }

            if (eventValue.Handler is TDelegate typedHandler)
            {
                handler = typedHandler;
                return true;
            }

            throw CsxamlRuntimeExceptionBuilder.Wrap(
                new InvalidOperationException(
                    $"Native event '{name}' on '{node.TagName}' expected handler type '{typeof(TDelegate).Name}'."),
                "native event read",
                sourceInfo: eventValue.SourceInfo ?? node.SourceInfo,
                detail: eventValue.Name);
        }

        handler = null;
        return false;
    }

    public static bool TryGetPropertyValue<T>(
        NativeElementNode node,
        string name,
        out T value)
    {
        foreach (var property in node.Properties)
        {
            if (!string.Equals(property.Name, name, StringComparison.Ordinal))
            {
                continue;
            }

            if (NativePropertyValueConverter.TryConvert(property, out value))
            {
                return true;
            }

            throw CsxamlRuntimeExceptionBuilder.Wrap(
                new InvalidOperationException(
                    $"Native property '{name}' on '{node.TagName}' expected value type '{typeof(T).Name}'."),
                "native property read",
                sourceInfo: property.SourceInfo ?? node.SourceInfo,
                detail: property.Name);
        }

        value = default!;
        return false;
    }

    public static bool TryGetAttachedPropertyValue<T>(
        NativeElementNode node,
        string ownerName,
        string propertyName,
        out T value)
    {
        foreach (var property in node.AttachedProperties)
        {
            if (!string.Equals(property.OwnerName, ownerName, StringComparison.Ordinal) ||
                !string.Equals(property.PropertyName, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            if (NativeAttachedPropertyValueConverter.TryConvert(property, out value))
            {
                return true;
            }

            throw CsxamlRuntimeExceptionBuilder.Wrap(
                new InvalidOperationException(
                    $"Attached property '{property.QualifiedName}' on '{node.TagName}' expected value type '{typeof(T).Name}'."),
                "attached property read",
                sourceInfo: property.SourceInfo ?? node.SourceInfo,
                detail: property.QualifiedName);
        }

        value = default!;
        return false;
    }
}
