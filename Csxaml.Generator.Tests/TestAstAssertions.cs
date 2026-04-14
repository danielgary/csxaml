namespace Csxaml.Generator.Tests;

internal static class TestAstAssertions
{
    public static MarkupNode RequireMarkup(ChildNode node)
    {
        return node as MarkupNode
            ?? throw new AssertFailedException(
                $"Expected a markup node but found '{node.GetType().Name}'.");
    }
}
