namespace Csxaml.Generator;

internal sealed partial class ComponentEmitter
{
    private void EmitResourceDictionaryRoot(ParsedComponent component)
    {
        _writer.WriteLine($"public sealed partial class {component.Definition.Name} : global::Microsoft.UI.Xaml.ResourceDictionary");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine($"public {component.Definition.Name}()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("MoveMergedDictionaries(Create(), this);");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
        _writer.WriteLine("public static global::Microsoft.UI.Xaml.ResourceDictionary Create()");
        _writer.WriteLine("{");
        _writer.PushIndent();
        if (CanLoadWithXamlReader(component))
        {
            EmitXamlReaderResourceDictionary(component);
        }
        else
        {
            _writer.WriteLine("var resources = new global::Microsoft.UI.Xaml.ResourceDictionary();");
            EmitMergedDictionaryAssignments(component, "resources", includeDefaultWinUiResources: true);
            _writer.WriteLine("return resources;");
        }

        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
        _writer.WriteLine("public static void ApplyTo(global::Microsoft.UI.Xaml.ResourceDictionary resources)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        EmitMergedDictionaryAssignments(component, "resources", includeDefaultWinUiResources: false);
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.WriteLine();
        _writer.WriteLine("private static void MoveMergedDictionaries(");
        _writer.PushIndent();
        _writer.WriteLine("global::Microsoft.UI.Xaml.ResourceDictionary source,");
        _writer.WriteLine("global::Microsoft.UI.Xaml.ResourceDictionary target)");
        _writer.PopIndent();
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("while (source.MergedDictionaries.Count > 0)");
        _writer.WriteLine("{");
        _writer.PushIndent();
        _writer.WriteLine("var dictionary = source.MergedDictionaries[0];");
        _writer.WriteLine("source.MergedDictionaries.RemoveAt(0);");
        _writer.WriteLine("target.MergedDictionaries.Add(dictionary);");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.PopIndent();
        _writer.WriteLine("}");
        _writer.PopIndent();
        _writer.WriteLine("}");
    }

    private void EmitMergedDictionaryAssignments(
        ParsedComponent component,
        string targetExpression,
        bool includeDefaultWinUiResources)
    {
        if (component.Definition.Root is not MarkupNode root)
        {
            return;
        }

        var mergedDictionaries = root.PropertyContent.FirstOrDefault(
            property => string.Equals(property.PropertyName, "MergedDictionaries", StringComparison.Ordinal));
        if (mergedDictionaries is null)
        {
            return;
        }

        foreach (var child in mergedDictionaries.Children.OfType<MarkupNode>())
        {
            if (!includeDefaultWinUiResources && IsXamlControlsResources(child))
            {
                continue;
            }

            _writer.WriteMappedLine(
                $"{targetExpression}.MergedDictionaries.Add(new {FormatResourceDictionaryType(child)}());",
                component.Source,
                child.Span,
                "resource-dictionary",
                child.TagName);
        }
    }

    private static string FormatResourceDictionaryType(MarkupNode node)
    {
        return string.Equals(node.Tag.LocalName, "XamlControlsResources", StringComparison.Ordinal)
            ? "global::Microsoft.UI.Xaml.Controls.XamlControlsResources"
            : node.TagName;
    }

    private static bool CanLoadWithXamlReader(ParsedComponent component)
    {
        if (component.Definition.Root is not MarkupNode root)
        {
            return false;
        }

        var mergedDictionaries = root.PropertyContent.FirstOrDefault(
            property => string.Equals(property.PropertyName, "MergedDictionaries", StringComparison.Ordinal));
        return mergedDictionaries?.Children.OfType<MarkupNode>().All(IsXamlControlsResources) == true;
    }

    private void EmitXamlReaderResourceDictionary(ParsedComponent component)
    {
        _writer.WriteLine("return (global::Microsoft.UI.Xaml.ResourceDictionary)global::Microsoft.UI.Xaml.Markup.XamlReader.Load(");
        _writer.PushIndent();
        _writer.WriteLine("$\"<ResourceDictionary xmlns=\\\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\\\" xmlns:controls=\\\"using:Microsoft.UI.Xaml.Controls\\\">\" +");
        _writer.WriteLine("\"<ResourceDictionary.MergedDictionaries>\" +");
        EmitXamlReaderMergedDictionaryTags(component);
        _writer.WriteLine("\"</ResourceDictionary.MergedDictionaries>\" +");
        _writer.WriteLine("\"</ResourceDictionary>\");");
        _writer.PopIndent();
    }

    private void EmitXamlReaderMergedDictionaryTags(ParsedComponent component)
    {
        if (component.Definition.Root is not MarkupNode root)
        {
            return;
        }

        var mergedDictionaries = root.PropertyContent.First(
            property => string.Equals(property.PropertyName, "MergedDictionaries", StringComparison.Ordinal));
        foreach (var child in mergedDictionaries.Children.OfType<MarkupNode>())
        {
            _writer.WriteMappedLine(
                "\"<controls:XamlControlsResources />\" +",
                component.Source,
                child.Span,
                "resource-dictionary",
                child.TagName);
        }
    }

    private static bool IsXamlControlsResources(MarkupNode node)
    {
        return string.Equals(node.Tag.LocalName, "XamlControlsResources", StringComparison.Ordinal);
    }
}
