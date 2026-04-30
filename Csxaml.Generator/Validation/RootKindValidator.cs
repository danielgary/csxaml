namespace Csxaml.Generator;

internal sealed class RootKindValidator
{
    private static readonly HashSet<string> WindowProperties = new(StringComparer.Ordinal)
    {
        "Title",
        "Width",
        "Height",
        "Backdrop"
    };

    public void Validate(SourceDocument source, ComponentDefinition definition)
    {
        ValidateParameters(source, definition);
        ValidateSlots(source, definition);
        ValidateRootProperties(source, definition);
    }

    private static void ValidateParameters(SourceDocument source, ComponentDefinition definition)
    {
        if (definition.Kind == ComponentKind.Element || definition.Parameters.Count == 0)
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            definition.Parameters[0].Span,
            $"component {definition.Kind} '{definition.Name}' does not support component parameters");
    }

    private static void ValidateSlots(SourceDocument source, ComponentDefinition definition)
    {
        if (definition.Kind == ComponentKind.Element ||
            (!definition.SupportsDefaultSlot && definition.NamedSlots.Count == 0))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            definition.Root.Span,
            $"component {definition.Kind} '{definition.Name}' does not support slots");
    }

    private static void ValidateRootProperties(SourceDocument source, ComponentDefinition definition)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in definition.RootProperties)
        {
            ValidateRootPropertyName(source, definition, property);
            if (!seen.Add(property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"root property '{property.Name}' is assigned more than once");
            }
        }

        ValidateWindowSizePair(source, definition);
    }

    private static void ValidateRootPropertyName(
        SourceDocument source,
        ComponentDefinition definition,
        RootPropertyDeclaration property)
    {
        if (definition.Kind == ComponentKind.Window && WindowProperties.Contains(property.Name))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            $"root property '{property.Name}' is not supported on component {definition.Kind} '{definition.Name}'");
    }

    private static void ValidateWindowSizePair(SourceDocument source, ComponentDefinition definition)
    {
        if (definition.Kind != ComponentKind.Window)
        {
            return;
        }

        var hasWidth = definition.RootProperties.Any(property => property.Name == "Width");
        var hasHeight = definition.RootProperties.Any(property => property.Name == "Height");
        if (hasWidth == hasHeight)
        {
            return;
        }

        var property = definition.RootProperties.First(candidate => candidate.Name is "Width" or "Height");
        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            "Window root properties 'Width' and 'Height' must be assigned together");
    }
}
