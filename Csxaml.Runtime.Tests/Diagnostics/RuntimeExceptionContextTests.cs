namespace Csxaml.Runtime.Tests.Diagnostics;

[TestClass]
public sealed class RuntimeExceptionContextTests
{
    [TestMethod]
    public void Render_ChildComponentFailure_PreservesStageAndSourceFrames()
    {
        var coordinator = new ComponentTreeCoordinator(new HostComponent());

        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(() => coordinator.Render());

        CollectionAssert.AreEqual(
            new[] { "root render", "component render", "child component render", "component render" },
            error.Frames.Select(frame => frame.Stage).ToArray());
        Assert.AreEqual("Host", error.Frames[0].ComponentName);
        Assert.AreEqual("Host", error.Frames[1].ComponentName);
        Assert.AreEqual("Host", error.Frames[2].ComponentName);
        Assert.AreEqual("BrokenChild", error.Frames[3].ComponentName);
        Assert.AreEqual("Host.csxaml", error.Frames[0].SourceInfo?.FilePath);
        Assert.AreEqual("Host.csxaml", error.Frames[1].SourceInfo?.FilePath);
        Assert.AreEqual("Host.csxaml", error.Frames[2].SourceInfo?.FilePath);
        Assert.AreEqual("BrokenChild.csxaml", error.Frames[3].SourceInfo?.FilePath);
        StringAssert.Contains(error.Message, "Tag: BrokenChild");
        StringAssert.Contains(error.Message, "Cause: boom");
        Assert.IsInstanceOfType<InvalidOperationException>(error.InnerException);
    }

    [TestMethod]
    public void NativePropertyReadFailure_IncludesMemberAndSpanContext()
    {
        var sourceInfo = new CsxamlSourceInfo(
            "TodoCard.csxaml",
            6,
            13,
            6,
            28,
            "TodoCard",
            "TextBlock",
            "Text");
        var node = new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", new object(), ValueKindHint.String, sourceInfo)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());

        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(
            () => NativeElementReader.TryGetPropertyValue<string>(node, "Text", out _));

        Assert.AreEqual("native property read", error.Frames[0].Stage);
        Assert.AreEqual(sourceInfo, error.Frames[0].SourceInfo);
        StringAssert.Contains(error.Message, "Member: Text");
        StringAssert.Contains(error.Message, "Span: 6:13-6:28");
        Assert.IsInstanceOfType<InvalidOperationException>(error.InnerException);
    }

    [TestMethod]
    public void Render_FailingChildPass_DoesNotPublishNewTreeOrRenderLaterSiblings()
    {
        LaterChildComponent.Reset();
        var host = new ToggleableFailureHostComponent();
        var coordinator = new ComponentTreeCoordinator(host);
        var updateCount = 0;
        coordinator.TreeUpdated += _ => updateCount++;

        coordinator.Render();
        host.FailOnChild = true;

        var error = Assert.ThrowsExactly<CsxamlRuntimeException>(() => coordinator.Render());

        Assert.AreEqual(1, updateCount);
        Assert.AreEqual(1, LaterChildComponent.RenderCount);
        StringAssert.Contains(error.Message, "boom");
    }

    private sealed class HostComponent : ComponentInstance
    {
        public override string CsxamlComponentName => "Host";

        public override CsxamlSourceInfo? CsxamlSourceInfo =>
            new("Host.csxaml", 1, 1, 3, 2, "Host");

        public override Node Render()
        {
            return new ComponentNode(
                typeof(BrokenChildComponent),
                null,
                Array.Empty<Node>(),
                Array.Empty<NativeAttachedPropertyValue>(),
                "position0",
                null,
                new CsxamlSourceInfo("Host.csxaml", 2, 12, 2, 26, "Host", "BrokenChild"));
        }
    }

    private sealed class BrokenChildComponent : ComponentInstance
    {
        public override string CsxamlComponentName => "BrokenChild";

        public override CsxamlSourceInfo? CsxamlSourceInfo =>
            new("BrokenChild.csxaml", 1, 1, 3, 2, "BrokenChild");

        public override Node Render()
        {
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class ToggleableFailureHostComponent : ComponentInstance
    {
        public bool FailOnChild { get; set; }

        public override Node Render()
        {
            return new NativeElementNode(
                "StackPanel",
                null,
                Array.Empty<NativePropertyValue>(),
                Array.Empty<NativeEventValue>(),
                [
                    new ComponentNode(typeof(LabelChildComponent), null, "position0", null),
                    new ComponentNode(typeof(FailOnDemandChildComponent), FailOnChild, "position1", null),
                    new ComponentNode(typeof(LaterChildComponent), null, "position2", null)
                ]);
        }
    }

    private sealed class LabelChildComponent : ComponentInstance
    {
        public override Node Render()
        {
            return new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", "Label")],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>());
        }
    }

    private sealed class FailOnDemandChildComponent : ComponentInstance<bool>
    {
        public override Node Render()
        {
            if (Props)
            {
                throw new InvalidOperationException("boom");
            }

            return new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", "Safe")],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>());
        }
    }

    private sealed class LaterChildComponent : ComponentInstance
    {
        public static int RenderCount { get; private set; }

        public static void Reset()
        {
            RenderCount = 0;
        }

        public override Node Render()
        {
            RenderCount++;
            return new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", "Later")],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>());
        }
    }
}
