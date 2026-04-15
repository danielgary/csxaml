namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class ParserTests
{
    [TestMethod]
    public void Parse_TodoCardDeclaration_ProducesTypedParametersAndConditional()
    {
        const string sourceText = """
            component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
                render <Border Background={TodoColors.DoneBackground} Padding={TodoColors.CardPadding}>
                    <StackPanel Spacing={8}>
                    <TextBlock Text={Title} Foreground={TodoColors.CardForeground} />
                    if (IsDone) {
                        <TextBlock Text="Done" />
                    }
                    <Button Content="Toggle" OnClick={OnToggle} />
                    </StackPanel>
                </Border>;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoCard.csxaml", sourceText).Definition;
        var root = TestAstAssertions.RequireMarkup(component.Root);
        var rootChild = TestAstAssertions.RequireMarkup(root.Children[0]);

        Assert.AreEqual("TodoCard", component.Name);
        Assert.HasCount(3, component.Parameters);
        Assert.AreEqual("string", component.Parameters[0].TypeName);
        Assert.AreEqual("Title", component.Parameters[0].Name);
        Assert.AreEqual("bool", component.Parameters[1].TypeName);
        Assert.AreEqual("IsDone", component.Parameters[1].Name);
        Assert.AreEqual("Action", component.Parameters[2].TypeName);
        Assert.AreEqual("OnToggle", component.Parameters[2].Name);
        Assert.AreEqual("Border", root.TagName);
        Assert.AreEqual("Background", root.Properties[0].Name);
        Assert.AreEqual("StackPanel", rootChild.TagName);
        Assert.AreEqual("Spacing", rootChild.Properties[0].Name);
        Assert.IsInstanceOfType<IfBlockNode>(rootChild.Children[1]);
    }

    [TestMethod]
    public void Parse_TodoBoardComposition_ProducesForeachAndComponentUsage()
    {
        const string sourceText = """
            component Element TodoBoard {
                State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateItems());

                render <StackPanel Spacing={12}>
                    <TextBlock Text="Todo Board" Foreground={TodoColors.BoardForeground} />
                    foreach (var item in Items.Value) {
                        <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} OnToggle={OnToggle} />
                    }
                </StackPanel>;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;
        var root = TestAstAssertions.RequireMarkup(component.Root);
        var loop = (ForEachBlockNode)root.Children[1];
        var child = (MarkupNode)loop.Children[0];

        Assert.HasCount(1, component.StateFields);
        Assert.AreEqual("Items", component.StateFields[0].Name);
        Assert.AreEqual("item", loop.ItemName);
        Assert.AreEqual("Items.Value", loop.CollectionExpression);
        Assert.AreEqual("Spacing", root.Properties[0].Name);
        Assert.AreEqual("TodoCard", child.TagName);
        Assert.HasCount(4, child.Properties);
        Assert.AreEqual("Key", child.Properties[0].Name);
    }

    [TestMethod]
    public void Parse_AliasQualifiedTag_PreservesPrefixAndLocalName()
    {
        const string sourceText = """
            using WinUi = Microsoft.UI.Xaml.Controls;

            component Element TodoBoard {
                render <WinUi:InfoBar IsOpen={true}></WinUi:InfoBar>;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.AreEqual("WinUi:InfoBar", root.TagName);
        Assert.AreEqual("WinUi", root.Tag.Prefix);
        Assert.AreEqual("InfoBar", root.Tag.LocalName);
    }

    [TestMethod]
    public void Parse_RenderStatementWithCommentsBeforeMarkup_IsValid()
    {
        const string sourceText = """
            component Element TodoBoard {
                render /* a */ <StackPanel />;
            }
            """;

        var component = GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText).Definition;

        Assert.AreEqual("StackPanel", TestAstAssertions.RequireMarkup(component.Root).TagName);
    }

    [TestMethod]
    public void Parse_BareMarkupReturn_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard {
                return <StackPanel />;
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "render <Root />;");
    }

    [TestMethod]
    public void Parse_ParenthesizedMarkupReturn_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard {
                return ( <StackPanel /> );
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "render <Root />;");
    }

    [TestMethod]
    public void Parse_NonMarkupRenderStatementPayload_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard(string Title) {
                render Title;
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "single markup root");
    }

    [TestMethod]
    public void Parse_MissingRenderStatement_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard {
                var title = "Todo";
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "missing final render statement");
    }

    [TestMethod]
    public void Parse_MultipleRenderRoots_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard {
                render <TextBlock /><Button />;
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "exactly one markup root");
    }

    [TestMethod]
    public void Parse_MissingRenderStatementSemicolon_ProducesTargetedDiagnostic()
    {
        const string sourceText = """
            component Element TodoBoard {
                render <StackPanel />
            }
            """;

        var exception = Assert.ThrowsExactly<DiagnosticException>(
            () => GeneratorTestHarness.Parse("TodoBoard.csxaml", sourceText));

        StringAssert.Contains(exception.Message, "missing ';' after render statement");
    }
}
