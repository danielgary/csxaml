namespace Csxaml.Runtime;

public static class ExternalControlRegistry
{
    private static readonly object Gate = new();
    private static readonly Dictionary<string, ExternalControlDescriptor> Descriptors =
        new(StringComparer.Ordinal);

    public static void Register(ExternalControlDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        lock (Gate)
        {
            if (Descriptors.TryGetValue(descriptor.TagName, out var existing))
            {
                if (existing.ControlType == descriptor.ControlType)
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"External control '{descriptor.TagName}' is already registered.");
            }

            Descriptors.Add(descriptor.TagName, descriptor);
        }
    }

    internal static bool TryGet(string tagName, out ExternalControlDescriptor? descriptor)
    {
        lock (Gate)
        {
            return Descriptors.TryGetValue(tagName, out descriptor);
        }
    }

    internal static void ClearForTests()
    {
        lock (Gate)
        {
            Descriptors.Clear();
        }
    }
}
