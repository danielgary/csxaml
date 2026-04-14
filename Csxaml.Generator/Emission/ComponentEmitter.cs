namespace Csxaml.Generator;

internal sealed class ComponentEmitter
{
    private readonly CompilationContext _compilation;
    private readonly IndentedCodeWriter _writer;

    public ComponentEmitter(IndentedCodeWriter writer, CompilationContext compilation)
    {
        _writer = writer;
        _compilation = compilation;
    }

    public void Emit(ParsedComponent component)
    {
        EmitUsings(component.File.UsingDirectives);
        EmitNamespace(component.File.Namespace);
        EmitFileHelpers(component.File.HelperCodeBlocks, beforeComponent: true, component.Definition.Span.Start);
        EmitPropsRecord(component.Definition);
        EmitComponentClass(component);
        EmitFileHelpers(component.File.HelperCodeBlocks, beforeComponent: false, component.Definition.Span.Start);
    }

    private void EmitComponentClass(ParsedComponent component)
    {
        var definition = component.Definition;
        var baseType = definition.Parameters.Count == 0
            ? "ComponentInstance"
            : $"ComponentInstance<{definition.Name}Props>";

        _writer.WriteLine($"public sealed class {definition.Name}Component : {baseType}");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitStateFields(definition);
        EmitPropAccessors(definition);
        EmitRegistrationConstructor(definition);
        EmitConstructor(definition);
        EmitRenderMethod(component);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitConstructor(ComponentDefinition component)
    {
        if (component.StateFields.Count == 0)
        {
            return;
        }

        _writer.WriteLine($"public {component.Name}Component()");
        _writer.WriteLine("{");
        _writer.PushIndent();

        foreach (var stateField in component.StateFields)
        {
            _writer.WriteLine(
                $"{stateField.Name} = new State<{stateField.TypeName}>({stateField.InitialValueExpression}, () => RequestRender?.Invoke());");
        }

        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }

    private void EmitNamespace(FileScopedNamespaceDefinition? fileNamespace)
    {
        var namespaceName = fileNamespace?.NamespaceName ?? _compilation.Project.DefaultComponentNamespace;
        _writer.WriteLine($"namespace {namespaceName};");
        _writer.WriteLine();
    }

    private void EmitPropAccessors(ComponentDefinition component)
    {
        foreach (var parameter in component.Parameters)
        {
            _writer.WriteLine($"private {parameter.TypeName} {parameter.Name} => Props.{parameter.Name};");
        }

        if (component.Parameters.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitPropsRecord(ComponentDefinition component)
    {
        if (component.Parameters.Count == 0)
        {
            return;
        }

        var parameterText = string.Join(
            ", ",
            component.Parameters.Select(parameter => $"{parameter.TypeName} {parameter.Name}"));

        _writer.WriteLine($"public sealed record {component.Name}Props({parameterText});");
        _writer.WriteLine();
    }

    private void EmitRenderMethod(ParsedComponent component)
    {
        _writer.WriteLine("public override Node Render()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        if (component.Definition.HelperCode is not null)
        {
            _writer.WriteBlock(component.Definition.HelperCode.CodeText);
            _writer.WriteLine();
        }

        new ChildNodeEmitter(_writer, component, _compilation).EmitRenderBody(component.Definition.Root);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitStateFields(ComponentDefinition component)
    {
        foreach (var stateField in component.StateFields)
        {
            _writer.WriteLine($"public State<{stateField.TypeName}> {stateField.Name};");
        }

        if (component.StateFields.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitUsings(IReadOnlyList<UsingDirectiveDefinition> usingDirectives)
    {
        _writer.WriteLine("using System;");
        _writer.WriteLine("using System.Collections.Generic;");
        _writer.WriteLine("using System.Linq;");
        _writer.WriteLine("using Csxaml.Runtime;");
        foreach (var usingDirective in usingDirectives)
        {
            if (usingDirective.Alias is null)
            {
                _writer.WriteLine($"using {usingDirective.NamespaceName};");
                continue;
            }

            _writer.WriteLine($"using {usingDirective.Alias} = {usingDirective.NamespaceName};");
        }
        _writer.WriteLine();
    }

    private void EmitFileHelpers(
        IReadOnlyList<FileHelperCodeBlock> helperCodeBlocks,
        bool beforeComponent,
        int componentStart)
    {
        foreach (var helperCodeBlock in helperCodeBlocks)
        {
            if ((helperCodeBlock.Span.Start < componentStart) != beforeComponent)
            {
                continue;
            }

            _writer.WriteBlock(helperCodeBlock.CodeText);
            _writer.WriteLine();
        }
    }

    private void EmitRegistrationConstructor(ComponentDefinition component)
    {
        if (!_compilation.HasExternalControls)
        {
            return;
        }

        _writer.WriteLine($"static {component.Name}Component()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine(
            $"global::{_compilation.Project.InternalGeneratedNamespace}.GeneratedExternalControlRegistration.EnsureRegistered();");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
    }
}
