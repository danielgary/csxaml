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
        EmitFileHelpers(component, beforeComponent: true, component.Definition.Span.Start);
        EmitPropsRecord(component);
        EmitComponentClass(component);
        EmitFileHelpers(component, beforeComponent: false, component.Definition.Span.Start);
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
        EmitComponentMetadata(component);
        EmitStateFields(component);
        EmitInjectFields(component);
        EmitPropAccessors(component);
        EmitRegistrationConstructor(definition);
        EmitResolveInjectedServicesMethod(component);
        EmitRenderMethod(component);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitComponentMetadata(ParsedComponent component)
    {
        _writer.WriteLine($"public override string CsxamlComponentName => \"{EscapeString(component.Definition.Name)}\";");
        _writer.WriteLine(
            $"public override CsxamlSourceInfo? CsxamlSourceInfo => {SourceInfoEmitter.Emit(component.Source, component.Definition.Name, component.Definition.Span)};");
        _writer.WriteLine();
    }

    private void EmitNamespace(FileScopedNamespaceDefinition? fileNamespace)
    {
        var namespaceName = fileNamespace?.NamespaceName ?? _compilation.Project.DefaultComponentNamespace;
        _writer.WriteLine($"namespace {namespaceName};");
        _writer.WriteLine();
    }

    private void EmitPropAccessors(ParsedComponent component)
    {
        foreach (var parameter in component.Definition.Parameters)
        {
            var accessor =
                $$"""
                {{LineDirectiveFormatter.Wrap(component.Source, parameter.Span, $"private {parameter.TypeName} {parameter.Name} => Props.{parameter.Name};")}}
                """;
            _writer.WriteMappedBlock(
                accessor,
                component.Source,
                parameter.Span,
                "component-parameter",
                parameter.Name);
        }

        if (component.Definition.Parameters.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitInjectFields(ParsedComponent component)
    {
        foreach (var injectField in component.Definition.InjectFields)
        {
            _writer.WriteMappedBlock(
                LineDirectiveFormatter.Wrap(
                    component.Source,
                    injectField.Span,
                    $$"""
                    private {{injectField.TypeName}}? _{{injectField.Name}};
                    private {{injectField.TypeName}} {{injectField.Name}} => _{{injectField.Name}} ?? throw new global::System.InvalidOperationException("Injected service '{{EscapeString(injectField.Name)}}' was not initialized.");
                    """),
                component.Source,
                injectField.Span,
                "inject-field",
                injectField.Name);
        }

        if (component.Definition.InjectFields.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitPropsRecord(ParsedComponent component)
    {
        if (component.Definition.Parameters.Count == 0)
        {
            return;
        }

        var parameterText = string.Join(
            ", ",
            component.Definition.Parameters.Select(parameter => $"{parameter.TypeName} {parameter.Name}"));
        var recordText = $"public sealed record {component.Definition.Name}Props({parameterText});";
        _writer.WriteMappedBlock(
            LineDirectiveFormatter.Wrap(component.Source, component.Definition.Span, recordText),
            component.Source,
            component.Definition.Span,
            "props-record",
            component.Definition.Name);
        _writer.WriteLine();
    }

    private void EmitRenderMethod(ParsedComponent component)
    {
        _writer.WriteLine("public override Node Render()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitStateInitialization(component);
        if (component.Definition.HelperCode is not null)
        {
            _writer.WriteMappedBlock(
                LineDirectiveFormatter.Wrap(
                    component.Source,
                    component.Definition.HelperCode.Span,
                    component.Definition.HelperCode.CodeText),
                component.Source,
                component.Definition.HelperCode.Span,
                "helper-code",
                component.Definition.Name);
            _writer.WriteLine();
        }

        new ChildNodeEmitter(_writer, component, _compilation).EmitRenderBody(component.Definition.Root);
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitStateInitialization(ParsedComponent component)
    {
        foreach (var stateField in component.Definition.StateFields)
        {
            _writer.WriteLine($"{stateField.Name} ??= Create{stateField.Name}State();");
        }

        if (component.Definition.StateFields.Count > 0)
        {
            _writer.WriteLine();
        }

        foreach (var stateField in component.Definition.StateFields)
        {
            var statement =
                $$"""
                State<{{stateField.TypeName}}> Create{{stateField.Name}}State()
                {
                    return new State<{{stateField.TypeName}}>(
                {{CodeBlockFormatter.Indent(LineDirectiveFormatter.Wrap(component.Source, stateField.InitialValueSpan, stateField.InitialValueExpression), 8)}}
                        ,
                        InvalidateState,
                        ValidateStateWrite);
                }
                """;
            _writer.WriteMappedBlock(
                statement,
                component.Source,
                stateField.Span,
                "state-initializer",
                stateField.Name);
        }

        if (component.Definition.StateFields.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitStateFields(ParsedComponent component)
    {
        foreach (var stateField in component.Definition.StateFields)
        {
            var stateFieldText =
                $$"""
                {{LineDirectiveFormatter.Wrap(component.Source, stateField.Span, $"public State<{stateField.TypeName}> {stateField.Name} = null!;")}}
                """;
            _writer.WriteMappedBlock(
                stateFieldText,
                component.Source,
                stateField.Span,
                "state-field",
                stateField.Name);
        }

        if (component.Definition.StateFields.Count > 0)
        {
            _writer.WriteLine();
        }
    }

    private void EmitResolveInjectedServicesMethod(ParsedComponent component)
    {
        if (component.Definition.InjectFields.Count == 0)
        {
            return;
        }

        _writer.WriteLine("protected override void ResolveInjectedServices(IServiceProvider services)");
        _writer.WriteLine("{");
        _writer.PushIndent();

        foreach (var injectField in component.Definition.InjectFields)
        {
            var statement =
                $$"""
                _{{injectField.Name}} = global::Csxaml.Runtime.InjectedServiceResolver.ResolveRequired<{{injectField.TypeName}}>(
                    this,
                    services,
                    "{{EscapeString(injectField.Name)}}",
                    {{SourceInfoEmitter.Emit(component.Source, component.Definition.Name, injectField.Span, memberName: injectField.Name)}});
                """;
            _writer.WriteMappedBlock(
                LineDirectiveFormatter.Wrap(component.Source, injectField.Span, statement),
                component.Source,
                injectField.Span,
                "inject-resolution",
                injectField.Name);
        }

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

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
