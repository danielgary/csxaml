namespace Csxaml.Generator;

internal sealed class PageXamlEmitter
{
    public string Emit(ParsedComponent component, CompilationContext compilation)
    {
        var page = FindPage(component, compilation);
        var pageType = $"{page.NamespaceName}.{page.Name}";

        return
            $"""
            <Page
                x:Class="{pageType}"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
            </Page>
            """;
    }

    private static ComponentCatalogEntry FindPage(
        ParsedComponent component,
        CompilationContext compilation)
    {
        return compilation.Components
            .FindLocalComponents()
            .Single(entry => ReferenceEquals(entry.LocalDefinition, component.Definition));
    }
}
