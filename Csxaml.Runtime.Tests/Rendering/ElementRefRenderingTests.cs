namespace Csxaml.Runtime.Tests.Rendering;

[TestClass]
public sealed class ElementRefRenderingTests
{
    [TestMethod]
    public void Render_AssignsRefOnInitialProjection()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));
        var reference = new ElementRef<FakeElement>();

        var textBox = (FakeElement)renderer.RenderProjectedRoot(CreateTextBox(reference: reference));

        Assert.AreSame(textBox, reference.Current);
        Assert.IsTrue(reference.TryGet(out var current));
        Assert.AreSame(textBox, current);
    }

    [TestMethod]
    public void Render_RetainedElementKeepsRefWithoutReassignment()
    {
        var adapter = new FakeControlAdapter("TextBox", supportsChildren: false);
        var renderer = CreateRenderer(adapter);
        var reference = new ElementRef<FakeElement>();

        var firstTextBox = (FakeElement)renderer.RenderProjectedRoot(CreateTextBox("todo-1", reference));
        var secondTextBox = (FakeElement)renderer.RenderProjectedRoot(CreateTextBox("todo-1", reference));

        Assert.AreSame(firstTextBox, secondTextBox);
        Assert.AreSame(secondTextBox, reference.Current);
        Assert.AreEqual(1, adapter.CreateCount);
    }

    [TestMethod]
    public void Render_ReplacedElementUpdatesRef()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));
        var reference = new ElementRef<FakeElement>();

        var firstTextBox = (FakeElement)renderer.RenderProjectedRoot(CreateTextBox("todo-1", reference));
        var secondTextBox = (FakeElement)renderer.RenderProjectedRoot(CreateTextBox("todo-2", reference));

        Assert.AreNotSame(firstTextBox, secondTextBox);
        Assert.AreSame(secondTextBox, reference.Current);
    }

    [TestMethod]
    public void Render_RemovedChildClearsRef()
    {
        var renderer = CreateRenderer(
            new FakeControlAdapter("StackPanel"),
            new FakeControlAdapter("TextBox", supportsChildren: false));
        var reference = new ElementRef<FakeElement>();

        renderer.RenderProjectedRoot(CreateHost([CreateTextBox(reference: reference)]));
        renderer.RenderProjectedRoot(CreateHost(Array.Empty<Node>()));

        Assert.IsNull(reference.Current);
    }

    [TestMethod]
    public void Dispose_ClearsRootRef()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));
        var reference = new ElementRef<FakeElement>();

        renderer.RenderProjectedRoot(CreateTextBox(reference: reference));
        renderer.Dispose();

        Assert.IsNull(reference.Current);
    }

    [TestMethod]
    public void Render_IncompatibleRefTypeThrowsContextualRuntimeException()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("TextBox", supportsChildren: false));
        var reference = new ElementRef<string>();

        var exception = Assert.ThrowsExactly<CsxamlRuntimeException>(
            () => renderer.RenderProjectedRoot(CreateTextBox(reference: reference, sourceInfo: CreateSourceInfo())));

        StringAssert.Contains(exception.Message, "ElementRef expected");
        Assert.IsTrue(exception.Frames.Any(frame => frame.Stage == "ref assignment"));
        Assert.IsTrue(exception.Frames.Any(frame => frame.SourceInfo?.MemberName == "Ref"));
    }

    [TestMethod]
    public void Render_RefWorksForExternallyRegisteredTag()
    {
        var renderer = CreateRenderer(new FakeControlAdapter("DemoControls:StatusButton", supportsChildren: false));
        var reference = new ElementRef<FakeElement>();

        var button = (FakeElement)renderer.RenderProjectedRoot(
            new NativeElementNode(
                "DemoControls:StatusButton",
                null,
                Array.Empty<NativePropertyValue>(),
                Array.Empty<NativeAttachedPropertyValue>(),
                new NativeElementRefValue(reference),
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>()));

        Assert.AreSame(button, reference.Current);
    }

    private static CsxamlSourceInfo CreateSourceInfo()
    {
        return new CsxamlSourceInfo(
            "ElementRefProbe.csxaml",
            3,
            18,
            3,
            33,
            "ElementRefProbe",
            "TextBox",
            "Ref");
    }

    private static NativeElementNode CreateHost(IReadOnlyList<Node> children)
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            children);
    }

    private static NativeElementNode CreateTextBox(
        string? key = null,
        ElementRef? reference = null,
        CsxamlSourceInfo? sourceInfo = null)
    {
        return new NativeElementNode(
            "TextBox",
            key,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            reference is null ? null : new NativeElementRefValue(reference, sourceInfo),
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>(),
            sourceInfo);
    }

    private static WinUiNodeRenderer CreateRenderer(params INativeControlAdapter[] adapters)
    {
        return new WinUiNodeRenderer(new ControlAdapterRegistry(adapters));
    }
}
