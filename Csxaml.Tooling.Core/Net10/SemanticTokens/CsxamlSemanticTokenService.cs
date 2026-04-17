using System.Text.RegularExpressions;
using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Completion;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.SemanticTokens;

public sealed partial class CsxamlSemanticTokenService
{
    private readonly CsxamlCSharpSemanticTokenService _csharpSemanticTokenService = new();
    private readonly CsxamlTagSymbolResolver _tagResolver = new();
    private readonly CsxamlWorkspaceLoader _workspaceLoader = new();

    public IReadOnlyList<CsxamlSemanticToken> GetTokens(string filePath, string text)
    {
        var workspace = _workspaceLoader.Load(filePath, text);
        var scan = CsxamlMarkupScanner.Scan(text);
        var currentNamespace = scan.NamespaceDirective?.NamespaceName ?? workspace.Project.DefaultNamespace;
        var tokens = new List<CsxamlSemanticToken>();

        EmitKeywordTokens(tokens, text);
        EmitComponentDeclarationTokens(tokens, scan.Components);
        EmitMarkupTokens(tokens, scan, currentNamespace, workspace);
        tokens.AddRange(_csharpSemanticTokenService.GetTokens(filePath, text));

        return NormalizeTokens(tokens);
    }

    private static void EmitKeywordTokens(ICollection<CsxamlSemanticToken> tokens, string text)
    {
        foreach (Match match in KeywordPattern().Matches(text))
        {
            tokens.Add(new CsxamlSemanticToken(match.Index, match.Length, CsxamlSemanticTokenType.Keyword));
        }
    }

    private static void EmitComponentDeclarationTokens(
        ICollection<CsxamlSemanticToken> tokens,
        IReadOnlyList<CsxamlComponentSignature> components)
    {
        foreach (var component in components)
        {
            tokens.Add(new CsxamlSemanticToken(component.NameStart, component.NameLength, CsxamlSemanticTokenType.Class, IsDeclaration: true));
            foreach (var parameter in component.Parameters)
            {
                tokens.Add(new CsxamlSemanticToken(parameter.Start, parameter.Length, CsxamlSemanticTokenType.Parameter, IsDeclaration: true));
            }
        }
    }

    private void EmitMarkupTokens(
        ICollection<CsxamlSemanticToken> tokens,
        CsxamlMarkupScanResult scan,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        foreach (var element in scan.Elements)
        {
            var resolvedTag = _tagResolver.Resolve(element.TagName, scan.UsingDirectives, currentNamespace, workspace);
            tokens.Add(CreateTagToken(element, resolvedTag));

            foreach (var attribute in element.Attributes)
            {
                EmitAttributeToken(tokens, attribute, resolvedTag, scan.UsingDirectives, currentNamespace);
            }
        }
    }

    private static CsxamlSemanticToken CreateTagToken(
        CsxamlMarkupElementReference element,
        CsxamlResolvedTag resolvedTag)
    {
        return resolvedTag.Kind switch
        {
            CsxamlResolvedTagKind.Native or CsxamlResolvedTagKind.External => new CsxamlSemanticToken(
                element.NameStart,
                element.NameLength,
                CsxamlSemanticTokenType.Class,
                IsDefaultLibrary: resolvedTag.Kind == CsxamlResolvedTagKind.Native),
            _ => new CsxamlSemanticToken(
                element.NameStart,
                element.NameLength,
                CsxamlSemanticTokenType.Class),
        };
    }

    private static void EmitAttributeToken(
        ICollection<CsxamlSemanticToken> tokens,
        CsxamlMarkupAttributeReference attribute,
        CsxamlResolvedTag resolvedTag,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace)
    {
        if (attribute.Name == "Key")
        {
            tokens.Add(new CsxamlSemanticToken(attribute.Start, attribute.Length, CsxamlSemanticTokenType.Property, IsReadOnly: true));
            return;
        }

        var attachedPropertySeparator = attribute.Name.IndexOf('.');
        if (attachedPropertySeparator > 0 &&
            CsxamlAttachedPropertyResolver.TryResolve(
                attribute.Name[..attachedPropertySeparator],
                attribute.Name[(attachedPropertySeparator + 1)..],
                usingDirectives,
                currentNamespace,
                out _))
        {
            tokens.Add(new CsxamlSemanticToken(attribute.Start, attribute.Length, CsxamlSemanticTokenType.Property, IsDefaultLibrary: true));
            return;
        }

        if (resolvedTag.Control is not null)
        {
            if (resolvedTag.Control.Events.Any(@event => @event.ExposedName == attribute.Name))
            {
                tokens.Add(new CsxamlSemanticToken(attribute.Start, attribute.Length, CsxamlSemanticTokenType.Event, IsDefaultLibrary: true));
                return;
            }

            if (resolvedTag.Control.Properties.Any(property => property.Name == attribute.Name))
            {
                tokens.Add(new CsxamlSemanticToken(attribute.Start, attribute.Length, CsxamlSemanticTokenType.Property, IsDefaultLibrary: true));
                return;
            }
        }

        if (resolvedTag.Component?.Metadata.Parameters.Any(parameter => parameter.Name == attribute.Name) == true)
        {
            tokens.Add(new CsxamlSemanticToken(attribute.Start, attribute.Length, CsxamlSemanticTokenType.Parameter));
        }
    }

    private static IReadOnlyList<CsxamlSemanticToken> NormalizeTokens(IEnumerable<CsxamlSemanticToken> tokens)
    {
        var exactSpanTokens = tokens
            .GroupBy(token => (token.Start, token.Length))
            .Select(ChoosePreferredToken)
            .OrderBy(token => token.Start)
            .ThenBy(token => token.Length)
            .ToList();

        var normalized = new List<CsxamlSemanticToken>(exactSpanTokens.Count);
        foreach (var token in exactSpanTokens)
        {
            if (normalized.Count > 0 &&
                token.Start < normalized[^1].Start + normalized[^1].Length)
            {
                continue;
            }

            normalized.Add(token);
        }

        return normalized;
    }

    private static CsxamlSemanticToken ChoosePreferredToken(IEnumerable<CsxamlSemanticToken> candidates)
    {
        return candidates
            .OrderByDescending(GetTokenPriority)
            .ThenByDescending(token => token.IsDeclaration)
            .ThenByDescending(token => token.IsReadOnly)
            .ThenByDescending(token => token.IsDefaultLibrary)
            .First();
    }

    private static int GetTokenPriority(CsxamlSemanticToken token)
    {
        return token.Type switch
        {
            CsxamlSemanticTokenType.Keyword => 7,
            CsxamlSemanticTokenType.Method => 6,
            CsxamlSemanticTokenType.Property => 5,
            CsxamlSemanticTokenType.Event => 4,
            CsxamlSemanticTokenType.Parameter => 3,
            CsxamlSemanticTokenType.Variable => 2,
            CsxamlSemanticTokenType.Interface => 1,
            CsxamlSemanticTokenType.Class => 0,
            _ => 0,
        };
    }

    [GeneratedRegex(@"\b(component|Element|State|inject|render|if|foreach|var|in|using|namespace)\b", RegexOptions.CultureInvariant)]
    private static partial Regex KeywordPattern();
}
