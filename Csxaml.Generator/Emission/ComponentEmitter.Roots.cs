namespace Csxaml.Generator;

internal sealed partial class ComponentEmitter
{
    private void EmitRootShell(ParsedComponent component)
    {
        var definition = component.Definition;
        var baseType = definition.Kind switch
        {
            ComponentKind.Page => "global::Microsoft.UI.Xaml.Controls.Page",
            ComponentKind.Window => "global::Microsoft.UI.Xaml.Window",
            _ => throw new InvalidOperationException($"Unsupported root kind '{definition.Kind}'.")
        };

        _writer.WriteLine($"public sealed partial class {definition.Name} : {baseType}");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("private global::Csxaml.Runtime.CsxamlRootHost? _csxamlHost;");
        _writer.WriteLine();
        EmitDefaultRootConstructor(definition);
        EmitServiceRootConstructor(component);
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private void EmitDefaultRootConstructor(ComponentDefinition definition)
    {
        _writer.WriteLine($"public {definition.Name}()");
        _writer.PushIndent();
        _writer.WriteLine(": this(null)");
        _writer.PopIndent();
        _writer.WriteLine("{");
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private void EmitServiceRootConstructor(ParsedComponent component)
    {
        var definition = component.Definition;
        _writer.WriteLine($"public {definition.Name}(global::System.IServiceProvider? services)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitPageInitializeComponent(definition);
        EmitRootPropertyAssignments(component);
        _writer.WriteLine(
            $"_csxamlHost = global::Csxaml.Runtime.CsxamlRootHost.Mount(this, new {definition.Name}Component(), services);");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitPageInitializeComponent(ComponentDefinition definition)
    {
        if (definition.Kind == ComponentKind.Page)
        {
            _writer.WriteLine("InitializeComponent();");
        }
    }

    private void EmitRootPropertyAssignments(ParsedComponent component)
    {
        if (component.Definition.Kind != ComponentKind.Window)
        {
            return;
        }

        EmitWindowTitleAssignment(component);
        EmitWindowSizeAssignment(component);
        EmitWindowBackdropAssignment(component);
    }

    private void EmitWindowTitleAssignment(ParsedComponent component)
    {
        var title = FindRootProperty(component.Definition, "Title");
        if (title is null)
        {
            return;
        }

        _writer.WriteMappedBlock(
            $"AppWindow.Title = {FormatRootPropertyValue(title)};",
            component.Source,
            title.Span,
            "root-property",
            title.Name);
    }

    private void EmitWindowSizeAssignment(ParsedComponent component)
    {
        var width = FindRootProperty(component.Definition, "Width");
        var height = FindRootProperty(component.Definition, "Height");
        if (width is null || height is null)
        {
            return;
        }

        var assignment =
            $$"""
            AppWindow.Resize(new global::Windows.Graphics.SizeInt32
            {
                Width = (int)({{FormatRootPropertyValue(width)}}),
                Height = (int)({{FormatRootPropertyValue(height)}})
            });
            """;
        _writer.WriteMappedBlock(
            assignment,
            component.Source,
            new TextSpan(width.Span.Start, height.Span.End - width.Span.Start),
            "root-property",
            "Size");
    }

    private void EmitWindowBackdropAssignment(ParsedComponent component)
    {
        var backdrop = FindRootProperty(component.Definition, "Backdrop");
        if (backdrop is null)
        {
            return;
        }

        _writer.WriteMappedBlock(
            $"SystemBackdrop = {FormatWindowBackdropValue(backdrop)};",
            component.Source,
            backdrop.Span,
            "root-property",
            backdrop.Name);
    }

    private static RootPropertyDeclaration? FindRootProperty(
        ComponentDefinition definition,
        string name)
    {
        return definition.RootProperties.FirstOrDefault(
            property => string.Equals(property.Name, name, StringComparison.Ordinal));
    }

    private static string FormatRootPropertyValue(RootPropertyDeclaration property)
    {
        return property.ValueExpression;
    }

    private static string FormatWindowBackdropValue(RootPropertyDeclaration property)
    {
        return property.ValueExpression.Trim() switch
        {
            "\"Mica\"" => "new global::Microsoft.UI.Xaml.Media.MicaBackdrop()",
            "\"Acrylic\"" => "new global::Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop()",
            "\"None\"" => "null",
            _ => property.ValueExpression
        };
    }
}
