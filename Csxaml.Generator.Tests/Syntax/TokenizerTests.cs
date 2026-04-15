namespace Csxaml.Generator.Tests.Syntax;

[TestClass]
public sealed class TokenizerTests
{
    [TestMethod]
    public void Tokenize_BoardSource_CapturesExpectedIdentifiers()
    {
        const string sourceText = """
            component Element TodoBoard {
                render <StackPanel>
                    foreach (var item in Items.Value) {
                        <TodoCard Key={item.Id} Title={item.Title} />
                    }
                </StackPanel>;
            }
            """;

        var tokens = new Tokenizer().Tokenize(
            new SourceDocument("TodoBoard.csxaml", GeneratorTestHarness.Normalize(sourceText)));

        var identifiers = tokens
            .Where(token => token.Kind == TokenKind.Identifier)
            .Select(token => token.Text)
            .ToList();

        CollectionAssert.Contains(identifiers, "component");
        CollectionAssert.Contains(identifiers, "TodoBoard");
        CollectionAssert.Contains(identifiers, "foreach");
        CollectionAssert.Contains(identifiers, "TodoCard");
    }
}
