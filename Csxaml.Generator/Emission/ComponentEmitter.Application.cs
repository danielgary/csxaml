namespace Csxaml.Generator;

internal sealed partial class ComponentEmitter
{
    private void EmitApplicationRoot(ParsedComponent component)
    {
        var declaration = component.Definition.Application
            ?? throw new InvalidOperationException("Application roots require application metadata.");

        _writer.WriteLine($"public sealed partial class {component.Definition.Name} : global::Microsoft.UI.Xaml.Application");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("private global::Microsoft.UI.Xaml.Window? _window;");
        _writer.WriteLine("private global::System.IServiceProvider? _services;");
        _writer.WriteLine();
        EmitApplicationConstructor(component, declaration);
        EmitApplicationHelperCode(component);
        EmitDefaultConfigureServices(component);
        EmitOnLaunched(component, declaration);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitApplicationConstructor(
        ParsedComponent component,
        ApplicationRootDeclaration declaration)
    {
        _writer.WriteLine($"public {component.Definition.Name}()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("InitializeComponent();");
        if (ShouldApplyApplicationResources(declaration.ResourcesTypeName))
        {
            _writer.WriteLine($"{declaration.ResourcesTypeName}.ApplyTo(Resources);");
        }

        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private bool ShouldApplyApplicationResources(string? resourcesTypeName)
    {
        if (resourcesTypeName is null)
        {
            return false;
        }

        var resources = _compilation.Components.FindLocalComponents()
            .FirstOrDefault(component =>
                component.Kind == Csxaml.ControlMetadata.ComponentKind.ResourceDictionary &&
                string.Equals(component.Name, resourcesTypeName, StringComparison.Ordinal));
        return resources?.LocalDefinition is null ||
            !IsDefaultWinUiResourcesOnly(resources.LocalDefinition);
    }

    private static bool IsDefaultWinUiResourcesOnly(ComponentDefinition definition)
    {
        if (definition.Root is not MarkupNode root)
        {
            return false;
        }

        var mergedDictionaries = root.PropertyContent.FirstOrDefault(
            property => string.Equals(property.PropertyName, "MergedDictionaries", StringComparison.Ordinal));
        return mergedDictionaries is null ||
            mergedDictionaries.Children
                .OfType<MarkupNode>()
                .All(node => string.Equals(node.Tag.LocalName, "XamlControlsResources", StringComparison.Ordinal));
    }

    private void EmitApplicationHelperCode(ParsedComponent component)
    {
        if (component.Definition.HelperCode is null)
        {
            return;
        }

        _writer.WriteMappedBlock(
            LineDirectiveFormatter.Wrap(
                component.Source,
                component.Definition.HelperCode.Span,
                component.Definition.HelperCode.CodeText),
            component.Source,
            component.Definition.HelperCode.Span,
            "application-helper-code",
            component.Definition.Name);
        _writer.WriteLine();
    }

    private void EmitDefaultConfigureServices(ParsedComponent component)
    {
        if (component.Definition.HelperCode?.CodeText.Contains("ConfigureServices", StringComparison.Ordinal) == true)
        {
            return;
        }

        _writer.WriteLine("private global::System.IServiceProvider? ConfigureServices()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("return null;");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private void EmitOnLaunched(
        ParsedComponent component,
        ApplicationRootDeclaration declaration)
    {
        _writer.WriteLine("protected override void OnLaunched(global::Microsoft.UI.Xaml.LaunchActivatedEventArgs args)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("_services = ConfigureServices();");
        _writer.WriteLine($"_window = new {declaration.StartupTypeName}(_services);");
        _writer.WriteLine("_window.Activate();");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }
}
