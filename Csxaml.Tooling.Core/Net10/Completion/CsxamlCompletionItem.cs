namespace Csxaml.Tooling.Core.Completion;

/// <summary>
/// Describes a completion item returned by the CSXAML tooling services.
/// </summary>
/// <param name="Label">The text displayed for the completion item.</param>
/// <param name="Kind">The semantic kind of completion item.</param>
/// <param name="Detail">Short detail text describing the item.</param>
/// <param name="InsertText">The text inserted when the completion is accepted.</param>
/// <param name="SortText">The stable sort key for ordering completion items.</param>
/// <param name="Documentation">Optional documentation shown with the completion item.</param>
/// <param name="IsSnippet">A value indicating whether <paramref name="InsertText"/> uses snippet syntax.</param>
public sealed record CsxamlCompletionItem(
    string Label,
    CsxamlCompletionItemKind Kind,
    string Detail,
    string InsertText,
    string SortText,
    string? Documentation = null,
    bool IsSnippet = false);
