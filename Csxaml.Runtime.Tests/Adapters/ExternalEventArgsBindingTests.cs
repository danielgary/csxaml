using Csxaml.ExternalControls;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ExternalEventArgsBindingTests
{
    [TestCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [TestMethod]
    public void Render_StatusButton_BindsSenderlessEventArgsHandler()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateStatusButtonDescriptor());
            var renderer = new WinUiNodeRenderer();
            var observed = new List<string>();

            var firstButton = (StatusButton)renderer.RenderProjectedRoot(
                CreateStatusButtonNode(args => observed.Add(args.Status)));
            firstButton.RaiseStatusChangedForTests("ready");

            var secondButton = (StatusButton)renderer.RenderProjectedRoot(
                CreateStatusButtonNode(args => observed.Add($"updated:{args.Status}")));
            secondButton.RaiseStatusChangedForTests("done");

            renderer.RenderProjectedRoot(CreateStatusButtonNode(null));
            secondButton.RaiseStatusChangedForTests("ignored");

            CollectionAssert.AreEqual(
                new[] { "ready", "updated:done" },
                observed);
            Assert.AreSame(firstButton, secondButton);
        });
    }

    private static ExternalControlDescriptor CreateStatusButtonDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(StatusButton),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(StatusButton).FullName!,
                typeof(StatusButton).FullName!,
                typeof(Button).FullName,
                ControlChildKind.Single,
                [
                    new PropertyMetadata("BadgeText", typeof(string).FullName!, true, true, false, true, ValueKindHint.String)
                ],
                [
                    new EventMetadata(
                        "StatusChanged",
                        "OnStatusChanged",
                        $"System.Action<{typeof(StatusChangedEventArgs).FullName}>",
                        true,
                        ValueKindHint.Unknown,
                        EventBindingKind.EventArgs)
                ]));
    }

    private static NativeElementNode CreateStatusButtonNode(Action<StatusChangedEventArgs>? handler)
    {
        var events = handler is null
            ? Array.Empty<NativeEventValue>()
            : [new NativeEventValue("OnStatusChanged", handler, ValueKindHint.Unknown)];
        return new NativeElementNode(
            typeof(StatusButton).FullName!,
            null,
            Array.Empty<NativePropertyValue>(),
            events,
            Array.Empty<Node>());
    }
}
