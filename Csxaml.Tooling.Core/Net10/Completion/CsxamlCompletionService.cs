using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.CSharp;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Completion;

/// <summary>
/// Provides CSXAML and embedded C# completion items.
/// </summary>
public sealed partial class CsxamlCompletionService
{
    private readonly CsxamlCSharpCompletionService _csharpCompletionService = new();
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    /// <summary>
    /// Gets completion items for a source position.
    /// </summary>
    /// <param name="filePath">The CSXAML file path.</param>
    /// <param name="text">The current CSXAML source text.</param>
    /// <param name="position">The zero-based source offset where completions are requested.</param>
    /// <returns>The completion items available at the position.</returns>
    public IReadOnlyList<CsxamlCompletionItem> GetCompletions(
        string filePath,
        string text,
        int position)
    {
        var markup = CsxamlMarkupScanner.Scan(text);
        var context = CsxamlMarkupContextAnalyzer.Analyze(text, position);
        if (context.Kind is CsxamlMarkupContextKind.TagName or CsxamlMarkupContextKind.AttributeName
            && !CsxamlMarkupExpressionDetector.IsInsideExpression(text, position))
        {
            return GetMarkupCompletions(filePath, text, markup, context);
        }

        var csharpItems = _csharpCompletionService.GetCompletions(filePath, text, position);
        if (csharpItems.Count > 0)
        {
            return csharpItems;
        }

        return Array.Empty<CsxamlCompletionItem>();
    }

    private IReadOnlyList<CsxamlCompletionItem> GetMarkupCompletions(
        string filePath,
        string text,
        CsxamlMarkupScanResult markup,
        CsxamlMarkupContext context)
    {
        var workspace = _workspaceLoader.Load(filePath, text);
        var currentNamespace = markup.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;

        return context.Kind switch
        {
            CsxamlMarkupContextKind.TagName => CompleteTags(context, markup.UsingDirectives, currentNamespace, workspace),
            CsxamlMarkupContextKind.AttributeName => CompleteAttributes(context, markup.UsingDirectives, currentNamespace, workspace),
            _ => Array.Empty<CsxamlCompletionItem>(),
        };
    }

    private static IReadOnlyList<CsxamlCompletionItem> CompleteTags(
        CsxamlMarkupContext context,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        var items = new List<CsxamlCompletionItem>();
        var prefix = context.PrefixText;
        if (context.PropertyContentOwner is not null)
        {
            return CompletePropertyContentTags(
                context,
                usingDirectives,
                currentNamespace,
                workspace);
        }

        if (context.Qualifier is null)
        {
            items.AddRange(
                ControlMetadataRegistry.Controls
                    .Where(control => MatchesPrefix(control.TagName, prefix))
                    .Select(
                        control => CsxamlMarkupCompletionItemFactory.CreateTagItem(
                            control.TagName,
                            "WinUI control",
                            control.ChildKind != ControlChildKind.None)));

            var namespaces = usingDirectives
                .Where(directive => directive.Alias is null && !directive.IsStatic)
                .Select(directive => directive.QualifiedName)
                .Append(currentNamespace)
                .Distinct(StringComparer.Ordinal);
            foreach (var namespaceName in namespaces)
            {
                items.AddRange(
                    workspace.Components
                        .Where(component => component.Metadata.NamespaceName == namespaceName)
                        .Where(component => MatchesPrefix(component.Metadata.Name, prefix))
                        .Select(
                            component => CsxamlMarkupCompletionItemFactory.CreateTagItem(
                                component.Metadata.Name,
                                $"Component from {component.Metadata.NamespaceName}",
                                component.Metadata.SupportsDefaultSlot)));

                items.AddRange(
                    workspace.ExternalControls
                        .Where(control => GetNamespace(control.ClrTypeName) == namespaceName)
                        .Where(control => MatchesPrefix(GetName(control.ClrTypeName), prefix))
                        .Select(
                            control => CsxamlMarkupCompletionItemFactory.CreateTagItem(
                                GetName(control.ClrTypeName),
                                $"External control from {namespaceName}",
                                control.ChildKind != ControlChildKind.None)));
            }
        }
        else
        {
            var namespaceName = usingDirectives
                .FirstOrDefault(directive => !directive.IsStatic && string.Equals(directive.Alias, context.Qualifier, StringComparison.Ordinal))?
                .QualifiedName;
            if (namespaceName is null)
            {
                return Array.Empty<CsxamlCompletionItem>();
            }

            items.AddRange(
                workspace.Components
                    .Where(component => component.Metadata.NamespaceName == namespaceName)
                    .Where(component => MatchesPrefix(component.Metadata.Name, prefix))
                    .Select(
                            component => CsxamlMarkupCompletionItemFactory.CreateTagItem(
                            $"{context.Qualifier}:{component.Metadata.Name}",
                            $"Component from {namespaceName}",
                            component.Metadata.SupportsDefaultSlot)));

            items.AddRange(
                workspace.ExternalControls
                    .Where(control => GetNamespace(control.ClrTypeName) == namespaceName)
                    .Where(control => MatchesPrefix(GetName(control.ClrTypeName), prefix))
                    .Select(
                            control => CsxamlMarkupCompletionItemFactory.CreateTagItem(
                            $"{context.Qualifier}:{GetName(control.ClrTypeName)}",
                            $"External control from {namespaceName}",
                            control.ChildKind != ControlChildKind.None)));
        }

        return Order(items);
    }

    private IReadOnlyList<CsxamlCompletionItem> CompleteAttributes(
        CsxamlMarkupContext context,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        if (context.TagName is null)
        {
            return Array.Empty<CsxamlCompletionItem>();
        }

        var resolvedTag = _tagResolver.Resolve(context.TagName, usingDirectives, currentNamespace, workspace);
        var duplicateSet = new HashSet<string>(context.ExistingAttributeNames, StringComparer.Ordinal);
        duplicateSet.Remove(context.PrefixText);

        var items = new List<CsxamlCompletionItem>();
        if (!duplicateSet.Contains("Key") && MatchesPrefix("Key", context.PrefixText))
        {
            items.Add(
                new CsxamlCompletionItem(
                    "Key",
                    CsxamlCompletionItemKind.Property,
                    "Framework attribute",
                    "Key=\"${1}\"",
                    "0-Key",
                    IsSnippet: true));
        }

        items.AddRange(
            AttachedPropertyMetadataRegistry.Properties
                .Where(property => !duplicateSet.Contains(property.QualifiedName))
                .Where(property => MatchesPrefix(property.QualifiedName, context.PrefixText))
                .Select(CsxamlMarkupCompletionItemFactory.CreateAttachedPropertyItem));

        if (resolvedTag.Control is not null)
        {
            if (!duplicateSet.Contains("Ref") && MatchesPrefix("Ref", context.PrefixText))
            {
                items.Add(CsxamlMarkupCompletionItemFactory.CreateRefItem(resolvedTag.Control));
            }

            items.AddRange(
                resolvedTag.Control.Properties
                    .Where(property => !duplicateSet.Contains(property.Name))
                    .Where(property => MatchesPrefix(property.Name, context.PrefixText))
                    .Select(CsxamlMarkupCompletionItemFactory.CreatePropertyItem));
            items.AddRange(
                resolvedTag.Control.Events
                    .Where(@event => !duplicateSet.Contains(@event.ExposedName))
                    .Where(@event => MatchesPrefix(@event.ExposedName, context.PrefixText))
                    .Select(CsxamlMarkupCompletionItemFactory.CreateEventItem));
        }

        if (resolvedTag.Component is not null)
        {
            items.AddRange(
                resolvedTag.Component.Metadata.Parameters
                    .Where(parameter => !duplicateSet.Contains(parameter.Name))
                    .Where(parameter => MatchesPrefix(parameter.Name, context.PrefixText))
                    .Select(CsxamlMarkupCompletionItemFactory.CreateComponentParameterItem));
        }

        return Order(items);
    }

    private static IReadOnlyList<CsxamlCompletionItem> Order(IEnumerable<CsxamlCompletionItem> items)
    {
        return items
            .DistinctBy(item => item.Label, StringComparer.Ordinal)
            .OrderBy(item => item.SortText, StringComparer.Ordinal)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetNamespace(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0 ? string.Empty : clrTypeName[..separatorIndex];
    }

    private static string GetName(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0 ? clrTypeName : clrTypeName[(separatorIndex + 1)..];
    }

    private static bool MatchesPrefix(string candidate, string prefix)
    {
        return string.IsNullOrEmpty(prefix)
            || candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

}
