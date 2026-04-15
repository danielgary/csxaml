using Csxaml.Generator;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpProjectionBuilder
{
    private static readonly string[] ImplicitUsings =
    [
        "using System;",
        "using System.Collections.Generic;",
        "using System.Linq;",
        "using Csxaml.Runtime;"
    ];

    private readonly Parser _parser = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    public CsxamlProjectedDocument? Build(string filePath, string text)
    {
        var source = new SourceDocument(filePath, text);
        CsxamlFileDefinition file;
        try
        {
            file = _parser.Parse(source);
        }
        catch (DiagnosticException)
        {
            return null;
        }

        var workspace = _workspaceLoader.Load(filePath, text);
        var currentNamespace = file.Namespace?.NamespaceName ?? workspace.Project.DefaultNamespace;
        var usingDirectives = file.UsingDirectives
            .Select(
                directive => new CsxamlUsingDirectiveInfo(
                    directive.NamespaceName,
                    directive.Alias,
                    directive.Span.Start,
                    directive.Span.Length))
            .ToList();

        var writer = new CsxamlProjectionWriter();
        AppendHeader(source, writer, file, workspace.Project.DefaultNamespace);
        AppendHelperBlocks(writer, source, file.HelperCodeBlocks.Where(block => block.Span.Start < file.Component.Span.Start));

        writer.AppendSynthetic($"partial class __CsxamlProjection_{file.Component.Name}\n{{\n");
        AppendComponentProperties(writer, file.Component.Parameters);
        AppendInjectFields(writer, file.Component.InjectFields);
        AppendPropertyCoercionHelpers(writer);
        AppendStateFields(writer, source, file.Component.StateFields);

        writer.AppendSynthetic("void __Render()\n{\n");
        if (file.Component.HelperCode is not null)
        {
            writer.AppendMapped(file.Component.HelperCode.CodeText, file.Component.HelperCode.Span.Start);
            writer.AppendSynthetic("\n");
        }

        new CsxamlRenderProjectionEmitter(source, writer, usingDirectives, currentNamespace, workspace)
            .Emit(file.Component.Root);

        writer.AppendSynthetic("}\n");
        writer.AppendSynthetic("}\n");
        AppendHelperBlocks(writer, source, file.HelperCodeBlocks.Where(block => block.Span.Start > file.Component.Span.Start));
        return writer.Build();
    }

    private static void AppendComponentProperties(CsxamlProjectionWriter writer, IReadOnlyList<ComponentParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            writer.AppendSynthetic(
                $"public {CsxamlProjectionTypeNameFormatter.Format(parameter.TypeName)} {parameter.Name} => default!;\n");
        }

        if (parameters.Count > 0)
        {
            writer.AppendSynthetic("\n");
        }
    }

    private static void AppendPropertyCoercionHelpers(CsxamlProjectionWriter writer)
    {
        writer.AppendSynthetic(
            """
            private static global::Microsoft.UI.Xaml.Media.Brush? __CsxamlProjectionToBrush(global::Microsoft.UI.Xaml.Media.Brush? value) => value;
            private static global::Microsoft.UI.Xaml.Media.Brush __CsxamlProjectionToBrush(global::Csxaml.Runtime.ArgbColor value) => default!;
            private static global::Microsoft.UI.Xaml.Style? __CsxamlProjectionToStyle(global::Microsoft.UI.Xaml.Style? value) => value;
            private static global::Microsoft.UI.Xaml.Style? __CsxamlProjectionToStyle(global::Csxaml.Runtime.DeferredStyle? value) => default;
            private static global::Microsoft.UI.Xaml.Thickness __CsxamlProjectionToThickness(global::Microsoft.UI.Xaml.Thickness value) => value;
            private static global::Microsoft.UI.Xaml.Thickness __CsxamlProjectionToThickness(double value) => default;
            private static global::Microsoft.UI.Xaml.Thickness __CsxamlProjectionToThickness(decimal value) => default;

            """);
    }

    private static void AppendInjectFields(
        CsxamlProjectionWriter writer,
        IReadOnlyList<InjectFieldDefinition> injectFields)
    {
        foreach (var injectField in injectFields)
        {
            writer.AppendSynthetic(
                $"private readonly {CsxamlProjectionTypeNameFormatter.Format(injectField.TypeName)} {injectField.Name} = default!;\n");
        }

        if (injectFields.Count > 0)
        {
            writer.AppendSynthetic("\n");
        }
    }

    private static void AppendHeader(
        SourceDocument source,
        CsxamlProjectionWriter writer,
        CsxamlFileDefinition file,
        string defaultNamespace)
    {
        foreach (var implicitUsing in ImplicitUsings)
        {
            writer.AppendSynthetic($"{implicitUsing}\n");
        }

        foreach (var usingDirective in file.UsingDirectives)
        {
            writer.AppendMapped(
                source.Text.Substring(usingDirective.Span.Start, usingDirective.Span.Length),
                usingDirective.Span.Start);
            writer.AppendSynthetic("\n");
        }

        var namespaceText = file.Namespace is null
            ? $"namespace {defaultNamespace};"
            : source.Text.Substring(file.Namespace.Span.Start, file.Namespace.Span.Length);
        if (!string.IsNullOrWhiteSpace(namespaceText))
        {
            if (file.Namespace is null)
            {
                writer.AppendSynthetic(namespaceText);
            }
            else
            {
                writer.AppendMapped(namespaceText, file.Namespace.Span.Start);
            }

            writer.AppendSynthetic("\n\n");
        }
    }

    private static void AppendHelperBlocks(
        CsxamlProjectionWriter writer,
        SourceDocument source,
        IEnumerable<FileHelperCodeBlock> blocks)
    {
        foreach (var block in blocks)
        {
            writer.AppendMapped(
                source.Text.Substring(block.Span.Start, block.Span.Length),
                block.Span.Start);
            writer.AppendSynthetic("\n\n");
        }
    }

    private static void AppendStateFields(
        CsxamlProjectionWriter writer,
        SourceDocument source,
        IReadOnlyList<StateFieldDefinition> stateFields)
    {
        foreach (var stateField in stateFields)
        {
            writer.AppendMapped(
                source.Text.Substring(stateField.Span.Start, stateField.Span.Length),
                stateField.Span.Start);
            writer.AppendSynthetic("\n");
        }

        if (stateFields.Count > 0)
        {
            writer.AppendSynthetic("\n");
        }
    }
}
