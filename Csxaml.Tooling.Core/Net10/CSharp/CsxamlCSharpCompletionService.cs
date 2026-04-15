using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Csxaml.Tooling.Core.Completion;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpCompletionService
{
    private readonly CsxamlCSharpCompilationFactory _compilationFactory = new();
    private readonly CsxamlCSharpProjectionBuilder _projectionBuilder = new();

    public IReadOnlyList<CsxamlCompletionItem> GetCompletions(string filePath, string text, int position)
    {
        var projection = _projectionBuilder.Build(filePath, text);
        if (projection is null || !projection.TryMapOriginalToProjected(position, out var projectedPosition))
        {
            return Array.Empty<CsxamlCompletionItem>();
        }

        var prefix = ReadIdentifierPrefix(projection.Text, projectedPosition, out var prefixStart);
        var compilation = _compilationFactory.Create(filePath, projection);
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);

        var items = IsMemberAccessContext(projection.Text, prefixStart)
            ? CompleteMemberAccess(semanticModel, syntaxTree, prefixStart, projectedPosition, prefix)
            : CompleteInScope(semanticModel, projectedPosition, prefix);

        return items
            .Concat(CsxamlCSharpCompletionItemFactory.CreateKeywordItems(prefix))
            .Where(item => item is not null)
            .Cast<CsxamlCompletionItem>()
            .DistinctBy(item => item.Label, StringComparer.Ordinal)
            .OrderBy(item => item.SortText, StringComparer.Ordinal)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<CsxamlCompletionItem?> CompleteInScope(
        SemanticModel semanticModel,
        int lookupPosition,
        string prefix)
    {
        return semanticModel
            .LookupSymbols(lookupPosition, includeReducedExtensionMethods: true)
            .Concat(semanticModel.LookupNamespacesAndTypes(lookupPosition))
            .Where(symbol => CsxamlCSharpCompletionItemFactory.MatchesPrefix(symbol.Name, prefix))
            .Select(CsxamlCSharpCompletionItemFactory.CreateSymbolItem);
    }

    private static IEnumerable<CsxamlCompletionItem?> CompleteMemberAccess(
        SemanticModel semanticModel,
        SyntaxTree syntaxTree,
        int prefixStart,
        int projectedPosition,
        string prefix)
    {
        var root = syntaxTree.GetRoot();
        var memberAccess = root
            .FindNode(new TextSpan(Math.Max(prefixStart - 1, 0), Math.Max(projectedPosition - Math.Max(prefixStart - 1, 0), 1)))
            .AncestorsAndSelf()
            .OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault();
        if (memberAccess is null)
        {
            return Array.Empty<CsxamlCompletionItem>();
        }

        if (semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is INamespaceSymbol namespaceSymbol)
        {
            return namespaceSymbol.GetMembers()
                .Where(symbol => CsxamlCSharpCompletionItemFactory.MatchesPrefix(symbol.Name, prefix))
                .Select(CsxamlCSharpCompletionItemFactory.CreateSymbolItem);
        }

        var leftSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        var type = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (type is null)
        {
            return Array.Empty<CsxamlCompletionItem>();
        }

        var staticContext = leftSymbol is INamedTypeSymbol;
        return EnumerateTypeMembers(type, staticContext)
            .Where(symbol => CsxamlCSharpCompletionItemFactory.MatchesPrefix(symbol.Name, prefix))
            .Select(CsxamlCSharpCompletionItemFactory.CreateSymbolItem);
    }

    private static IEnumerable<ISymbol> EnumerateTypeMembers(ITypeSymbol type, bool staticContext)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var member in current.GetMembers())
            {
                if (member.IsStatic != staticContext && member.Kind != SymbolKind.NamedType)
                {
                    continue;
                }

                yield return member;
            }
        }
    }

    private static bool IsMemberAccessContext(string text, int prefixStart)
    {
        return prefixStart > 0 && text[prefixStart - 1] == '.';
    }

    private static string ReadIdentifierPrefix(string text, int position, out int start)
    {
        start = Math.Clamp(position, 0, text.Length);
        while (start > 0 && IsIdentifierPart(text[start - 1]))
        {
            start--;
        }

        return text[start..Math.Clamp(position, 0, text.Length)];
    }

    private static bool IsIdentifierPart(char character)
    {
        return char.IsLetterOrDigit(character) || character == '_';
    }
}
