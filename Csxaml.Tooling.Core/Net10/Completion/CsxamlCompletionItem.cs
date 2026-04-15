namespace Csxaml.Tooling.Core.Completion;

public sealed record CsxamlCompletionItem(
    string Label,
    CsxamlCompletionItemKind Kind,
    string Detail,
    string InsertText,
    string SortText,
    string? Documentation = null,
    bool IsSnippet = false);
