namespace Csxaml.Runtime;

internal static class NativeAttachedPropertyValueConverter
{
    public static bool TryConvert<T>(NativeAttachedPropertyValue property, out T value)
    {
        return NativePropertyValueConverter.TryConvert(
            new NativePropertyValue(property.QualifiedName, property.Value, property.ValueKindHint),
            out value);
    }
}
