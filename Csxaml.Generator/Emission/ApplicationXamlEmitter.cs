namespace Csxaml.Generator;

internal sealed class ApplicationXamlEmitter
{
    public string Emit(CompilationContext compilation)
    {
        var application = FindApplication(compilation);
        var applicationType = $"{application.NamespaceName}.{application.Name}";

        return
            $"""
            <Application
                x:Class="{applicationType}"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:controls="using:Microsoft.UI.Xaml.Controls">
                <Application.Resources>
                    <ResourceDictionary>
                        <ResourceDictionary.MergedDictionaries>
                            <controls:XamlControlsResources />
                        </ResourceDictionary.MergedDictionaries>
                    </ResourceDictionary>
                </Application.Resources>
            </Application>
            """;
    }

    private static ComponentCatalogEntry FindApplication(CompilationContext compilation)
    {
        return compilation.Components
            .FindLocalComponents()
            .Single(component => component.Kind == Csxaml.ControlMetadata.ComponentKind.Application);
    }
}
