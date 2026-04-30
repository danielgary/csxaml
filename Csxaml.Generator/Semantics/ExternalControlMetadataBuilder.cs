using System.Reflection;

namespace Csxaml.Generator;

internal sealed class ExternalControlMetadataBuilder
{
    public bool TryBuild(Type controlType, out ControlMetadataModel? metadata, out string? reason)
    {
        if (!SupportsControlType(controlType, out reason))
        {
            metadata = null;
            return false;
        }

        var builtInBaseControl = FindBuiltInBaseControl(controlType);
        var content = ExternalControlContentMetadataResolver.Resolve(controlType);
        metadata = new ControlMetadataModel(
            controlType.FullName ?? controlType.Name,
            controlType.FullName ?? controlType.Name,
            controlType.BaseType?.FullName,
            content.ToChildKind(),
            content,
            BuildProperties(controlType, builtInBaseControl),
            BuildEvents(controlType, builtInBaseControl));
        reason = null;
        return true;
    }

    private static IReadOnlyList<EventMetadata> BuildEvents(
        Type controlType,
        ControlMetadataModel? builtInBaseControl)
    {
        var reflectedEvents = controlType
            .GetEvents(BindingFlags.Instance | BindingFlags.Public)
            .Where(IsSupportedEvent)
            .Select(BuildEventMetadata)
            .ToList();

        return (builtInBaseControl?.Events ?? Array.Empty<EventMetadata>())
            .Concat(reflectedEvents)
            .DistinctBy(eventMetadata => eventMetadata.ExposedName, StringComparer.Ordinal)
            .OrderBy(eventMetadata => eventMetadata.ExposedName, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<PropertyMetadata> BuildProperties(
        Type controlType,
        ControlMetadataModel? builtInBaseControl)
    {
        var reflectedProperties = controlType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0)
            .Where(property => property.SetMethod is not null)
            .Select(
                property => new
                {
                    Property = property,
                    IsDependencyProperty = IsDependencyProperty(controlType, property.Name),
                    ValueKind = ExternalControlValueKindResolver.Resolve(property.PropertyType)
                })
            .Where(entry => entry.ValueKind != ValueKindHint.Unknown)
            .Where(entry => entry.IsDependencyProperty || SupportsClrProperty(entry.ValueKind, entry.Property.PropertyType))
            .Select(
                entry => new PropertyMetadata(
                    entry.Property.Name,
                    entry.Property.PropertyType.FullName ?? entry.Property.PropertyType.Name,
                    true,
                    entry.IsDependencyProperty,
                    false,
                    true,
                    entry.ValueKind))
            .ToList();

        return reflectedProperties
            .Concat(builtInBaseControl?.Properties ?? Array.Empty<PropertyMetadata>())
            .DistinctBy(property => property.Name, StringComparer.Ordinal)
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsDependencyProperty(Type controlType, string propertyName)
    {
        for (var current = controlType; current is not null; current = current.BaseType)
        {
            if (current.GetField(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is not null)
            {
                return true;
            }

            var dependencyProperty = current.GetProperty(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (string.Equals(
                    dependencyProperty?.PropertyType.FullName,
                    "Microsoft.UI.Xaml.DependencyProperty",
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSupportedEvent(EventInfo eventInfo)
    {
        var invoke = eventInfo.EventHandlerType?.GetMethod("Invoke");
        return invoke is not null &&
            string.Equals(invoke.ReturnType.FullName, typeof(void).FullName, StringComparison.Ordinal) &&
            invoke.GetParameters().All(parameter => !parameter.ParameterType.IsByRef) &&
            eventInfo.AddMethod is not null &&
            eventInfo.RemoveMethod is not null;
    }

    private static EventMetadata BuildEventMetadata(EventInfo eventInfo)
    {
        var parameters = eventInfo.EventHandlerType!.GetMethod("Invoke")!.GetParameters();
        if (parameters.Length == 2)
        {
            return new EventMetadata(
                eventInfo.Name,
                $"On{eventInfo.Name}",
                $"System.Action<{FormatTypeName(parameters[1].ParameterType)}>",
                true,
                ValueKindHint.Unknown,
                EventBindingKind.EventArgs);
        }

        return new EventMetadata(
            eventInfo.Name,
            $"On{eventInfo.Name}",
            typeof(Action).FullName ?? nameof(Action),
            true,
            ValueKindHint.Unknown,
            EventBindingKind.Direct);
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }

    private static bool SupportsControlType(Type controlType, out string? reason)
    {
        if (!IsFrameworkElement(controlType))
        {
            reason = "type does not derive from FrameworkElement";
            return false;
        }

        if (!controlType.IsVisible || controlType.IsAbstract)
        {
            reason = "type must be public and non-abstract";
            return false;
        }

        if (controlType.IsGenericTypeDefinition || controlType.ContainsGenericParameters)
        {
            reason = "generic control types are not supported";
            return false;
        }

        if (!controlType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Any(constructor => constructor.GetParameters().Length == 0))
        {
            reason = "type must have a public parameterless constructor";
            return false;
        }

        reason = null;
        return true;
    }

    private static bool IsFrameworkElement(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, "Microsoft.UI.Xaml.FrameworkElement", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SupportsClrProperty(ValueKindHint valueKind, Type propertyType)
    {
        return valueKind != ValueKindHint.Object || IsUiElement(propertyType);
    }

    private static bool IsUiElement(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, "Microsoft.UI.Xaml.UIElement", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static ControlMetadataModel? FindBuiltInBaseControl(Type controlType)
    {
        for (var current = controlType.BaseType; current is not null; current = current.BaseType)
        {
            var match = ControlMetadataRegistry.Controls.SingleOrDefault(
                candidate => string.Equals(candidate.ClrTypeName, current.FullName, StringComparison.Ordinal));
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
