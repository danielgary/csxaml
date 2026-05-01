namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class PropertyContentParserTests
{
    [TestMethod]
    public void Parse_PropertyContent_StoresSeparateFromChildren()
    {
        var component = GeneratorTestHarness.Parse(
            "ButtonHost.csxaml",
            """
            component Element ButtonHost {
                render <Button>
                    <Button.Flyout>
                        <Flyout />
                    </Button.Flyout>
                    <TextBlock />
                </Button>;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);
        var propertyContent = root.PropertyContent.Single();

        Assert.AreEqual("Button", propertyContent.OwnerName);
        Assert.AreEqual("Flyout", propertyContent.PropertyName);
        Assert.HasCount(1, propertyContent.Children);
        Assert.AreEqual("Flyout", TestAstAssertions.RequireMarkup(propertyContent.Children[0]).TagName);
        Assert.HasCount(1, root.Children);
        Assert.AreEqual("TextBlock", TestAstAssertions.RequireMarkup(root.Children[0]).TagName);
    }

    [TestMethod]
    public void Parse_AliasQualifiedPropertyContent_PreservesOwner()
    {
        var component = GeneratorTestHarness.Parse(
            "ControlExampleHost.csxaml",
            """
            using Widgets = MyApp.Controls;

            component Element ControlExampleHost {
                render <Widgets:ControlExample>
                    <Widgets:ControlExample.Example>
                        <Button />
                    </Widgets:ControlExample.Example>
                </Widgets:ControlExample>;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);
        var propertyContent = root.PropertyContent.Single();

        Assert.AreEqual("Widgets:ControlExample", propertyContent.OwnerName);
        Assert.AreEqual("Example", propertyContent.PropertyName);
        Assert.HasCount(1, propertyContent.Children);
    }

    [TestMethod]
    public void Parse_DottedRenderRoot_RemainsMarkupNode()
    {
        var component = GeneratorTestHarness.Parse(
            "DottedRoot.csxaml",
            """
            component Element DottedRoot {
                render <Controls.Primitive />;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.AreEqual("Controls.Primitive", root.TagName);
        Assert.HasCount(0, root.PropertyContent);
    }
}
