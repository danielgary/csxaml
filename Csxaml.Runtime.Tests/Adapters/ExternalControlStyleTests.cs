using Csxaml.ExternalControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ExternalControlStyleTests
{
    [TestCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [TestMethod]
    public void Render_StatusButton_AppliesAndClearsStyle()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateStatusButtonDescriptor());
            var renderer = new WinUiNodeRenderer();
            var style = new Style { TargetType = typeof(StatusButton) };

            var firstButton = (StatusButton)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    typeof(StatusButton).FullName!,
                    null,
                    [
                        new NativePropertyValue("BadgeText", "todo-1", ValueKindHint.String),
                        new NativePropertyValue("Style", style, ValueKindHint.Unknown)
                    ],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));
            var secondButton = (StatusButton)renderer.RenderProjectedRoot(
                new NativeElementNode(
                    typeof(StatusButton).FullName!,
                    null,
                    [new NativePropertyValue("BadgeText", "todo-1", ValueKindHint.String)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()));

            Assert.AreSame(firstButton, secondButton);
            Assert.AreSame(style, firstButton.Style);
            Assert.IsNull(secondButton.Style);
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
                    new Csxaml.ControlMetadata.PropertyMetadata("BadgeText", typeof(string).FullName!, true, true, false, true, ValueKindHint.String),
                    new Csxaml.ControlMetadata.PropertyMetadata("Style", typeof(Style).FullName!, true, true, false, true, ValueKindHint.Unknown)
                ],
                Array.Empty<EventMetadata>()));
    }
}
