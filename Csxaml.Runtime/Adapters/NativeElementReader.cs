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

            throw new InvalidOperationException(
                $"Native event '{name}' on '{node.TagName}' expected handler type '{typeof(TDelegate).Name}'.");
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

            throw new InvalidOperationException(
                $"Native property '{name}' on '{node.TagName}' expected value type '{typeof(T).Name}'.");
        }

        value = default!;
        return false;
    }
}
