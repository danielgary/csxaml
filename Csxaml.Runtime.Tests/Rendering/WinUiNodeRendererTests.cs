namespace Csxaml.Runtime.Tests.Rendering;

[TestClass]
public sealed class WinUiNodeRendererTests
{
    [TestMethod]
    public void Render_Button_ReusesElementAndRebindsClickHandler()
    {
        var firstClicks = 0;
        var secondClicks = 0;
        var renderer = CreateRenderer(new FakeControlAdapter("Button", supportsChildren: false));

        var firstButton = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "Button",
                null,
                [new NativePropertyValue("Content", "Toggle")],
                [new NativeEventValue("OnClick", (Action)(() => firstClicks++))],
                Array.Empty<Node>()));

        var secondButton = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "Button",
                null,
                [new NativePropertyValue("Content", "Toggle")],
                [new NativeEventValue("OnClick", (Action)(() => secondClicks++))],
                Array.Empty<Node>()));

        Assert.AreSame(firstButton, secondButton);
        Assert.AreEqual("Toggle", secondButton.Properties["Content"]);

        secondButton.Events["OnClick"]();

        Assert.AreEqual(0, firstClicks);
        Assert.AreEqual(1, secondClicks);
    }

    [TestMethod]
    public void Render_BorderAndTextBlock_PatchPropertiesWithoutReplacingControls()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("Border"),
            new FakeControlAdapter("TextBlock", supportsChildren: false));

        var firstBorder = (FakeElement)renderer.RenderProjectedRoot(CreateBorder("Red", "Todo"));
        var firstText = firstBorder.Children.Single();

        var secondBorder = (FakeElement)renderer.RenderProjectedRoot(CreateBorder("Green", "Done"));
        var secondText = secondBorder.Children.Single();

        Assert.AreSame(firstBorder, secondBorder);
        Assert.AreSame(firstText, secondText);
        Assert.AreEqual("Green", secondBorder.Properties["Background"]);
        Assert.AreEqual("Done", secondText.Properties["Text"]);
    }

    [TestMethod]
    public void Render_ReplacesElementWhenTagChanges()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("Border"),
            new FakeControlAdapter("TextBlock", supportsChildren: false));

        var firstRoot = (FakeElement)renderer.RenderProjectedRoot(CreateBorder("Red", "Todo"));
        var secondRoot = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", "Replaced")],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        Assert.AreNotSame(firstRoot, secondRoot);
        Assert.AreEqual("TextBlock", secondRoot.TagName);
        Assert.AreEqual("Replaced", secondRoot.Properties["Text"]);
    }

    [TestMethod]
    public void Render_ReplacedElement_ClearsTrackedEventBindings()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("Button", supportsChildren: false),
            new FakeControlAdapter("TextBlock", supportsChildren: false));

        var firstButton = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "Button",
                null,
                [new NativePropertyValue("Content", "Toggle")],
                [new NativeEventValue("OnClick", (Action)(() => { }))],
                Array.Empty<Node>()));

        renderer.RenderProjectedRoot(
            new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", "Replacement")],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        Assert.IsEmpty(firstButton.Events);
    }

    [TestMethod]
    public void Render_TextBox_ReusesElementAcrossControlledUpdates()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));

        var firstTextBox = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "TextBox",
                null,
                [new NativePropertyValue("Text", "Draft", global::Csxaml.ControlMetadata.ValueKindHint.String)],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        var secondTextBox = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "TextBox",
                null,
                [new NativePropertyValue("Text", "Updated", global::Csxaml.ControlMetadata.ValueKindHint.String)],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        Assert.AreSame(firstTextBox, secondTextBox);
        Assert.AreEqual("Updated", secondTextBox.Properties["Text"]);
    }

    [TestMethod]
    public void Render_CheckBox_ReusesElementAcrossControlledUpdates()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("CheckBox", supportsChildren: false));

        var firstCheckBox = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "CheckBox",
                null,
                [new NativePropertyValue("IsChecked", false, global::Csxaml.ControlMetadata.ValueKindHint.Bool)],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        var secondCheckBox = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "CheckBox",
                null,
                [new NativePropertyValue("IsChecked", true, global::Csxaml.ControlMetadata.ValueKindHint.Bool)],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        Assert.AreSame(firstCheckBox, secondCheckBox);
        Assert.IsTrue((bool)secondCheckBox.Properties["IsChecked"]!);
    }

    private static WinUiNodeRenderer CreateRenderer(params INativeControlAdapter[] adapters)
    {
        return new WinUiNodeRenderer(new ControlAdapterRegistry(adapters));
    }

    private static NativeElementNode CreateBorder(string background, string text)
    {
        return new NativeElementNode(
            "Border",
            null,
            [new NativePropertyValue("Background", background)],
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    null,
                    [new NativePropertyValue("Text", text)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }
}
