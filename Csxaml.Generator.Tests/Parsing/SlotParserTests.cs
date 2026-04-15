namespace Csxaml.Generator.Tests.Parsing;

[TestClass]
public sealed class SlotParserTests
{
    [TestMethod]
    public void Parse_DefaultSlotOutlet_ProducesSlotNode()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            component Element TodoCard {
                render <Border>
                    <Slot />
                </Border>;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);

        Assert.HasCount(1, root.Children);
        Assert.IsInstanceOfType<SlotOutletNode>(root.Children[0]);
        Assert.IsTrue(component.SupportsDefaultSlot);
    }

    [TestMethod]
    public void Parse_PrefixedSlotTag_RemainsMarkupNode()
    {
        var component = GeneratorTestHarness.Parse(
            "TodoCard.csxaml",
            """
            using Widgets = Demo.Controls;

            component Element TodoCard {
                render <Border>
                    <Widgets:Slot />
                </Border>;
            }
            """).Definition;

        var root = TestAstAssertions.RequireMarkup(component.Root);
        var child = TestAstAssertions.RequireMarkup(root.Children[0]);

        Assert.AreEqual("Widgets:Slot", child.TagName);
        Assert.IsFalse(component.SupportsDefaultSlot);
    }
}
