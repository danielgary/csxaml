namespace Csxaml.Generator;

internal sealed partial class ComponentEmitter
{
    private void EmitFileHelpers(
        ParsedComponent component,
        bool beforeComponent,
        int componentStart)
    {
        foreach (var helperCodeBlock in component.File.HelperCodeBlocks)
        {
            if ((helperCodeBlock.Span.Start < componentStart) != beforeComponent)
            {
                continue;
            }

            _writer.WriteMappedBlock(
                LineDirectiveFormatter.Wrap(component.Source, helperCodeBlock.Span, helperCodeBlock.CodeText),
                component.Source,
                helperCodeBlock.Span,
                "file-helper-code");
            _writer.WriteLine();
        }
    }

    private void EmitNamespace(FileScopedNamespaceDefinition? fileNamespace)
    {
        var namespaceName = fileNamespace?.NamespaceName ?? _compilation.Project.DefaultComponentNamespace;
        _writer.WriteLine($"namespace {namespaceName};");
        _writer.WriteLine();
    }

    private void EmitRegistrationConstructor(string className)
    {
        if (!_compilation.HasExternalControls)
        {
            return;
        }

        _writer.WriteLine($"static {className}()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine(
            $"global::{_compilation.Project.InternalGeneratedNamespace}.GeneratedExternalControlRegistration.EnsureRegistered();");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private void EmitUsings(IReadOnlyList<UsingDirectiveDefinition> usingDirectives)
    {
        _writer.WriteLine("#nullable enable");
        _writer.WriteLine("using System;");
        _writer.WriteLine("using System.Collections.Generic;");
        _writer.WriteLine("using System.Linq;");
        _writer.WriteLine("using Csxaml.Runtime;");
        foreach (var usingDirective in usingDirectives)
        {
            if (usingDirective.IsStatic)
            {
                _writer.WriteLine($"using static {usingDirective.QualifiedName};");
                continue;
            }

            if (usingDirective.Alias is null)
            {
                _writer.WriteLine($"using {usingDirective.QualifiedName};");
                continue;
            }

            _writer.WriteLine($"using {usingDirective.Alias} = {usingDirective.QualifiedName};");
        }

        _writer.WriteLine();
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
