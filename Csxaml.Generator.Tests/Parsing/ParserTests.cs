namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class ParserTests
{
    [TestMethod]
    public void Parse_TodoCardDeclaration_ProducesTypedParametersAndConditional()
    {
        const string sourceText = """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                return <StackPanel>
                    <TextBlock Text={Title} />
                    if (IsDone) {
                        <TextBlock Text="Done" />
                    }
                    <Button Content="Toggle" OnClick={OnToggle} />
                </StackPanel>;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoCard.csxaml", sourceText).Definition;

        Assert.AreEqual("TodoCard", component.Name);
        Assert.HasCount(3, component.Parameters);
        Assert.AreEqual("string", component.Parameters[0].TypeName);
        Assert.AreEqual("Title", component.Parameters[0].Name);
        Assert.AreEqual("bool", component.Parameters[1].TypeName);
        Assert.AreEqual("IsDone", component.Parameters[1].Name);
        Assert.AreEqual("Action", component.Parameters[2].TypeName);
        Assert.AreEqual("OnToggle", component.Parameters[2].Name);
        Assert.IsInstanceOfType<IfBlockNode>(component.Root.Children[1]);
    }

    [TestMethod]
    public void Parse_TodoBoardComposition_ProducesForeachAndComponentUsage()
    {
        const string sourceText = """
            component Element TodoBoard {
                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateItems());

                return <StackPanel>
                    <TextBlock Text="Todo Board" />
                    foreach (var item in Items.Value) {
                        <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} OnToggle={OnToggle} />
                    }
                </StackPanel>;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;
        var loop = (ForEachBlockNode)component.Root.Children[1];
        var child = (MarkupNode)loop.Children[0];

        Assert.HasCount(1, component.StateFields);
        Assert.AreEqual("Items", component.StateFields[0].Name);
        Assert.AreEqual("item", loop.ItemName);
        Assert.AreEqual("Items.Value", loop.CollectionExpression);
        Assert.AreEqual("TodoCard", child.TagName);
        Assert.HasCount(4, child.Properties);
        Assert.AreEqual("Key", child.Properties[0].Name);
    }
}
