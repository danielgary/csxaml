namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class AttachedPropertyParserTests
{
    [TestMethod]
    public void Parse_AttachedProperties_TrackOwnerAndLocalName()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoBoard.csxaml",
            """
            component Element TodoBoard {
                render <Grid RowDefinitions="Auto,*">
                    <TextBlock Grid.Row={0} AutomationProperties.Name="Task Editor" Text="Todo Board" />
                </Grid>;
            }
            """);

        var root = TestAstAssertions.RequireMarkup(component.Definition.Root);
        var title = TestAstAssertions.RequireMarkup(root.Children[0]);

        Assert.AreEqual("Grid", root.TagName);
        Assert.AreEqual("Grid.Row", title.Properties[0].Name);
        Assert.AreEqual("Grid", title.Properties[0].OwnerName);
        Assert.AreEqual("Row", title.Properties[0].PropertyName);
        Assert.IsTrue(title.Properties[0].IsAttached);
        Assert.AreEqual("AutomationProperties.Name", title.Properties[1].Name);
        Assert.AreEqual("AutomationProperties", title.Properties[1].OwnerName);
        Assert.AreEqual("Name", title.Properties[1].PropertyName);
        Assert.IsTrue(title.Properties[1].IsAttached);
    }
}
