using Csxaml.ExternalControls;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class ExternalControlAdapterPropertyContentTests
{
    [TestCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [TestMethod]
    public void Render_ControlExample_SetsAndClearsPropertyContent()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateControlExampleDescriptor());
            var renderer = new WinUiNodeRenderer();

            var firstExample = (ControlExample)renderer.RenderProjectedRoot(
                CreateControlExamplePropertyContentNode("Options", "Choice"));
            var firstChild = (TextBlock)firstExample.Options!;

            Assert.AreEqual("Choice", firstChild.Text);

            var secondExample = (ControlExample)renderer.RenderProjectedRoot(
                CreateControlExamplePropertyContentNode(null, null));

            Assert.AreSame(firstExample, secondExample);
            Assert.IsNull(secondExample.Options);
        });
    }

    private static ExternalControlDescriptor CreateControlExampleDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(ControlExample),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(ControlExample).FullName!,
                typeof(ControlExample).FullName!,
                typeof(Button).FullName,
                ControlChildKind.Single,
                new ControlContentMetadata(
                    "Example",
                    ControlContentKind.Single,
                    typeof(Microsoft.UI.Xaml.UIElement).FullName!,
                    null,
                    ControlContentSource.ContentPropertyAttribute),
                [
                    new PropertyMetadata(
                        "Options",
                        typeof(Microsoft.UI.Xaml.UIElement).FullName!,
                        true,
                        false,
                        false,
                        true,
                        ValueKindHint.Object)
                ],
                Array.Empty<EventMetadata>()));
    }

    private static NativeElementNode CreateControlExamplePropertyContentNode(
        string? propertyName,
        string? childText)
    {
        var propertyContent = propertyName is null
            ? Array.Empty<NativePropertyContentValue>()
            :
            [
                new NativePropertyContentValue(
                    propertyName,
                    [
                        new NativeElementNode(
                            "TextBlock",
                            null,
                            [new NativePropertyValue("Text", childText!)],
                            Array.Empty<NativeEventValue>(),
                            Array.Empty<Node>())
                    ])
            ];

        return new NativeElementNode(
            typeof(ControlExample).FullName!,
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeAttachedPropertyValue>(),
            null,
            Array.Empty<NativeEventValue>(),
            propertyContent,
            Array.Empty<Node>());
    }
}
