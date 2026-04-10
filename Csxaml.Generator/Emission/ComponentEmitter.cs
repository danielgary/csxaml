namespace Csxaml.Generator;

internal sealed class ComponentEmitter
{
    private readonly ComponentCatalog _catalog;
    private readonly IndentedCodeWriter _writer;

    public ComponentEmitter(IndentedCodeWriter writer, ComponentCatalog catalog)
    {
        _writer = writer;
        _catalog = catalog;
    }

    public void Emit(ComponentDefinition component)
    {
        EmitUsings();
        EmitNamespace();
        EmitPropsRecord(component);
        EmitComponentClass(component);
    }

    private void EmitComponentClass(ComponentDefinition component)
    {
        var baseType = component.Parameters.Count == 0
            ? "ComponentInstance"
            : $"ComponentInstance<{component.Name}Props>";

        _writer.WriteLine($"public sealed class {component.Name}Component : {baseType}");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitStateFields(component);
        EmitPropAccessors(component);
        EmitConstructor(component);
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

    private void EmitNamespace()
    {
        _writer.WriteLine("namespace GeneratedCsxaml;");
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

    private void EmitRenderMethod(ComponentDefinition component)
    {
        _writer.WriteLine("public override Node Render()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        new ChildNodeEmitter(_writer, _catalog).EmitRenderBody(component.Root);
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

    private void EmitUsings()
    {
        _writer.WriteLine("using System;");
        _writer.WriteLine("using System.Collections.Generic;");
        _writer.WriteLine("using System.Linq;");
        _writer.WriteLine("using Csxaml.Runtime;");
        _writer.WriteLine();
    }
}
