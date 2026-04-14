namespace Csxaml.Generator;

internal sealed class ComponentDefinitionValidator
{
    private readonly MarkupValidator _markupValidator = new();
    private readonly SlotDefinitionValidator _slotDefinitionValidator = new();

    public void Validate(ParsedComponent component, CompilationContext compilation)
    {
        ValidateUniqueNames(
            component.Source,
            component.Definition.Parameters.Select(parameter => (parameter.Name, parameter.Span)));

        ValidateUniqueNames(
            component.Source,
            component.Definition.StateFields.Select(field => (field.Name, field.Span)));

        _slotDefinitionValidator.Validate(component.Source, component.Definition);

        if (component.Definition.Root is MarkupNode markupRoot)
        {
            _markupValidator.Validate(component.Source, component, markupRoot, compilation);
        }
    }

    private static void ValidateUniqueNames(
        SourceDocument source,
        IEnumerable<(string Name, TextSpan Span)> entries)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!names.Add(entry.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    entry.Span,
                    $"duplicate name '{entry.Name}'");
            }
        }
    }
}
