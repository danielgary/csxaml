using Csxaml.Tooling.Core.SemanticTokens;

namespace Csxaml.Tooling.Core.Tests.Tooling;

[TestClass]
public sealed class CsxamlSemanticTokenRegressionTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    [TestMethod]
    public void Semantic_tokens_for_todo_board_do_not_overlap_and_classify_helper_return_types()
    {
        var filePath = Path.Combine(RepoRoot, "Csxaml.Demo", "Components", "TodoBoard.csxaml");
        var text = File.ReadAllText(filePath);

        var tokens = new CsxamlSemanticTokenService()
            .GetTokens(filePath, text)
            .OrderBy(token => token.Start)
            .ThenBy(token => token.Length)
            .ToList();
        var overlaps = tokens
            .Zip(tokens.Skip(1), (previous, current) => (previous, current))
            .Where(pair => pair.current.Start < pair.previous.Start + pair.previous.Length)
            .ToList();
        var returnTypeStart = text.IndexOf("TodoItemModel SelectedItem", StringComparison.Ordinal);

        Assert.IsFalse(
            overlaps.Any(),
            string.Join(
                Environment.NewLine,
                overlaps.Select(pair =>
                    $"{pair.previous.Type}@{pair.previous.Start}:{pair.previous.Length} overlaps {pair.current.Type}@{pair.current.Start}:{pair.current.Length}")));
        Assert.IsTrue(
            tokens.Any(token =>
                token.Start == returnTypeStart &&
                token.Length == "TodoItemModel".Length &&
                token.Type == CsxamlSemanticTokenType.Class),
            string.Join(", ", tokens.Select(token => $"{token.Type}@{token.Start}:{token.Length}")));
    }
}
