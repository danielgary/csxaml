using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Csxaml.Tooling.Core.Completion;

namespace Csxaml.Tooling.Core.CSharp;

internal static class CsxamlCSharpCompletionItemFactory
{
    private static readonly SymbolDisplayFormat DetailFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    private static readonly string[] Keywords =
    [
        "base",
        "false",
        "foreach",
        "if",
        "in",
        "nameof",
        "new",
        "null",
        "return",
        "this",
        "true",
        "var"
    ];

    public static IEnumerable<CsxamlCompletionItem> CreateKeywordItems(string prefix)
    {
        return Keywords
            .Where(keyword => MatchesPrefix(keyword, prefix))
            .Select(
                keyword => new CsxamlCompletionItem(
                    keyword,
                    CsxamlCompletionItemKind.Keyword,
                    "C# keyword",
                    keyword,
                    $"0-{keyword}"));
    }

    public static CsxamlCompletionItem? CreateSymbolItem(ISymbol symbol)
    {
        if (ShouldSkip(symbol))
        {
            return null;
        }

        return new CsxamlCompletionItem(
            symbol.Name,
            MapKind(symbol),
            symbol.ToDisplayString(DetailFormat),
            symbol.Name,
            $"{GetSortPrefix(symbol)}-{symbol.Name}");
    }

    public static bool MatchesPrefix(string candidate, string prefix)
    {
        return string.IsNullOrEmpty(prefix) ||
            candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static CsxamlCompletionItemKind MapKind(ISymbol symbol)
    {
        return symbol switch
        {
            IMethodSymbol => CsxamlCompletionItemKind.Method,
            INamespaceSymbol => CsxamlCompletionItemKind.Namespace,
            ILocalSymbol or IRangeVariableSymbol => CsxamlCompletionItemKind.Variable,
            IParameterSymbol => CsxamlCompletionItemKind.Parameter,
            IEventSymbol => CsxamlCompletionItemKind.Event,
            IFieldSymbol or IPropertySymbol => CsxamlCompletionItemKind.Property,
            _ => CsxamlCompletionItemKind.Class,
        };
    }

    private static string GetSortPrefix(ISymbol symbol)
    {
        return symbol.Kind switch
        {
            SymbolKind.Local or SymbolKind.RangeVariable => "1",
            SymbolKind.Parameter => "2",
            SymbolKind.Field or SymbolKind.Property => "3",
            SymbolKind.Method => "4",
            SymbolKind.NamedType => "5",
            SymbolKind.Namespace => "6",
            _ => "7",
        };
    }

    private static bool ShouldSkip(ISymbol symbol)
    {
        return string.IsNullOrWhiteSpace(symbol.Name) ||
            symbol.Name.StartsWith("__csxaml", StringComparison.Ordinal) ||
            symbol.Name.StartsWith("__CsxamlProjection_", StringComparison.Ordinal) ||
            symbol is IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor or MethodKind.PropertyGet or MethodKind.PropertySet } ||
            symbol.CanBeReferencedByName == false;
    }
}
