using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.CodeActions;

public sealed class CsxamlCodeActionService
{
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    public IReadOnlyList<CsxamlCodeAction> GetCodeActions(
        string filePath,
        string text,
        int startLine,
        int startCharacter,
        int endLine,
        int endCharacter)
    {
        var startOffset = GetOffset(text, startLine, startCharacter);
        var endOffset = GetOffset(text, endLine, endCharacter);
        var markup = CsxamlMarkupScanner.Scan(text);

        var element = FindElement(markup, startOffset, endOffset);
        var attributeContext = FindAttribute(markup, startOffset, endOffset);
        if (element is null && attributeContext is null)
        {
            return Array.Empty<CsxamlCodeAction>();
        }

        var workspace = _workspaceLoader.Load(filePath, text);
        var currentNamespace = markup.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;

        if (attributeContext is not null)
        {
            return GetAttributeCodeActions(
                text,
                markup,
                currentNamespace,
                workspace,
                attributeContext.Value.Element,
                attributeContext.Value.Attribute);
        }

        return GetTagCodeActions(text, markup, currentNamespace, workspace, element!);
    }

    private IReadOnlyList<CsxamlCodeAction> GetTagCodeActions(
        string text,
        CsxamlMarkupScanResult markup,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element)
    {
        var resolvedTag = _tagResolver.Resolve(element.TagName, markup.UsingDirectives, currentNamespace, workspace);
        if (resolvedTag.Kind is not CsxamlResolvedTagKind.None and not CsxamlResolvedTagKind.Ambiguous)
        {
            return Array.Empty<CsxamlCodeAction>();
        }

        var suggestion = CsxamlClosestNameSuggester.Find(
            element.TagName,
            CollectVisibleTagNames(element, markup.UsingDirectives, currentNamespace, workspace));
        if (string.IsNullOrWhiteSpace(suggestion) || string.Equals(suggestion, element.TagName, StringComparison.Ordinal))
        {
            return Array.Empty<CsxamlCodeAction>();
        }

        return
        [
            CreateReplaceAction(
                text,
                element.NameStart,
                element.NameLength,
                element.TagName,
                suggestion),
        ];
    }

    private IReadOnlyList<CsxamlCodeAction> GetAttributeCodeActions(
        string text,
        CsxamlMarkupScanResult markup,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace,
        CsxamlMarkupElementReference element,
        CsxamlMarkupAttributeReference attribute)
    {
        var resolvedTag = _tagResolver.Resolve(element.TagName, markup.UsingDirectives, currentNamespace, workspace);
        var candidates = CollectVisibleAttributeNames(resolvedTag, attribute.Name);
        var suggestion = CsxamlClosestNameSuggester.Find(attribute.Name, candidates);
        if (string.IsNullOrWhiteSpace(suggestion) || string.Equals(suggestion, attribute.Name, StringComparison.Ordinal))
        {
            return Array.Empty<CsxamlCodeAction>();
        }

        return
        [
            CreateReplaceAction(
                text,
                attribute.Start,
                attribute.Length,
                attribute.Name,
                suggestion),
        ];
    }

    private static IEnumerable<string> CollectVisibleTagNames(
        CsxamlMarkupElementReference element,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        var separatorIndex = element.TagName.IndexOf(':');
        if (separatorIndex >= 0)
        {
            var alias = element.TagName[..separatorIndex];
            var namespaceName = usingDirectives
                .FirstOrDefault(directive => !directive.IsStatic && string.Equals(directive.Alias, alias, StringComparison.Ordinal))?
                .QualifiedName;
            if (namespaceName is null)
            {
                return Array.Empty<string>();
            }

            return workspace.Components
                .Where(component => string.Equals(component.Metadata.NamespaceName, namespaceName, StringComparison.Ordinal))
                .Select(component => $"{alias}:{component.Metadata.Name}")
                .Concat(
                    workspace.ExternalControls
                        .Where(control => string.Equals(GetNamespace(control.ClrTypeName), namespaceName, StringComparison.Ordinal))
                        .Select(control => $"{alias}:{GetName(control.ClrTypeName)}"));
        }

        var names = new List<string>();
        names.AddRange(ControlMetadataRegistry.Controls.Select(control => control.TagName));
        names.AddRange(
            workspace.Components
                .Where(component => string.Equals(component.Metadata.NamespaceName, currentNamespace, StringComparison.Ordinal))
                .Select(component => component.Metadata.Name));
        foreach (var directive in usingDirectives.Where(directive => directive.Alias is null && !directive.IsStatic))
        {
            names.AddRange(
                workspace.Components
                    .Where(component => string.Equals(component.Metadata.NamespaceName, directive.QualifiedName, StringComparison.Ordinal))
                    .Select(component => component.Metadata.Name));
            names.AddRange(
                workspace.ExternalControls
                    .Where(control => string.Equals(GetNamespace(control.ClrTypeName), directive.QualifiedName, StringComparison.Ordinal))
                    .Select(control => GetName(control.ClrTypeName)));
        }

        return names;
    }

    private static IEnumerable<string> CollectVisibleAttributeNames(CsxamlResolvedTag resolvedTag, string currentName)
    {
        var names = new List<string>();
        if (resolvedTag.Control is not null)
        {
            names.AddRange(resolvedTag.Control.Properties.Select(property => property.Name));
            names.AddRange(resolvedTag.Control.Events.Select(@event => @event.ExposedName));
        }

        if (resolvedTag.Component is not null)
        {
            names.AddRange(resolvedTag.Component.Metadata.Parameters.Select(parameter => parameter.Name));
        }

        names.Add("Key");
        names.AddRange(AttachedPropertyMetadataRegistry.Properties.Select(property => property.QualifiedName));
        return names.Where(name => !string.Equals(name, currentName, StringComparison.Ordinal));
    }

    private static CsxamlCodeAction CreateReplaceAction(
        string text,
        int start,
        int length,
        string actualName,
        string suggestedName)
    {
        var edit = CreateEdit(text, start, length, suggestedName);
        return new CsxamlCodeAction(
            $"Replace '{actualName}' with '{suggestedName}'",
            new[] { edit });
    }

    private static CsxamlCodeActionEdit CreateEdit(string text, int start, int length, string newText)
    {
        var startPosition = GetLineAndCharacter(text, start);
        var endPosition = GetLineAndCharacter(text, start + length);
        return new CsxamlCodeActionEdit(
            startPosition.Line,
            startPosition.Character,
            endPosition.Line,
            endPosition.Character,
            newText);
    }

    private static CsxamlMarkupElementReference? FindElement(
        CsxamlMarkupScanResult markup,
        int startOffset,
        int endOffset)
    {
        return markup.Elements.FirstOrDefault(
            candidate => RangesIntersect(candidate.NameStart, candidate.NameStart + candidate.NameLength, startOffset, endOffset));
    }

    private static (CsxamlMarkupElementReference Element, CsxamlMarkupAttributeReference Attribute)? FindAttribute(
        CsxamlMarkupScanResult markup,
        int startOffset,
        int endOffset)
    {
        foreach (var element in markup.Elements.Where(candidate => !candidate.IsClosing))
        {
            var attribute = element.Attributes.FirstOrDefault(
                candidate => RangesIntersect(candidate.Start, candidate.Start + candidate.Length, startOffset, endOffset));
            if (attribute is not null)
            {
                return (element, attribute);
            }
        }

        return null;
    }

    private static bool RangesIntersect(int leftStart, int leftEnd, int rightStart, int rightEnd)
    {
        return leftStart <= rightEnd && rightStart <= leftEnd;
    }

    private static int GetOffset(string text, int line, int character)
    {
        var currentLine = 0;
        var index = 0;
        while (index < text.Length && currentLine < line)
        {
            if (text[index] == '\n')
            {
                currentLine++;
            }

            index++;
        }

        return Math.Min(index + character, text.Length);
    }

    private static (int Line, int Character) GetLineAndCharacter(string text, int offset)
    {
        var line = 0;
        var character = 0;
        for (var index = 0; index < offset && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
                character = 0;
                continue;
            }

            if (text[index] != '\r')
            {
                character++;
            }
        }

        return (line, character);
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
}
