namespace Csxaml.Generator;

internal sealed class ComponentDefinitionValidator
{
    private readonly MarkupValidator _markupValidator = new();
    private readonly ResourceDictionaryRootValidator _resourceDictionaryRootValidator = new();
    private readonly RootKindValidator _rootKindValidator = new();
    private readonly SlotDefinitionValidator _slotDefinitionValidator = new();

    public void Validate(ParsedComponent component, CompilationContext compilation)
    {
        ValidateUniqueNames(
            component.Source,
            component.Definition.Parameters
                .Select(parameter => (parameter.Name, parameter.Span))
                .Concat(component.Definition.InjectFields.Select(field => (field.Name, field.Span)))
                .Concat(component.Definition.StateFields.Select(field => (field.Name, field.Span))));

        _slotDefinitionValidator.Validate(component.Source, component.Definition);
        _rootKindValidator.Validate(component.Source, component.Definition);

        if (component.Definition.Kind == ComponentKind.Application)
        {
            return;
        }

        if (component.Definition.Kind == ComponentKind.ResourceDictionary)
        {
            _resourceDictionaryRootValidator.Validate(component.Source, component.Definition);
            return;
        }

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
