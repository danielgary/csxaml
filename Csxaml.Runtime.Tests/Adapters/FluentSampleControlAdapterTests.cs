using Csxaml.ExternalControls;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class FluentSampleControlAdapterTests
{
    [TestCleanup]
    public void Cleanup()
    {
        ExternalControlRegistry.ClearForTests();
    }

    [TestMethod]
    public void Render_FluentCard_AppliesContentAndKeepsFluentSurfaceState()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateFluentCardDescriptor());
            var renderer = new WinUiNodeRenderer();

            var firstCard = (FluentCard)renderer.RenderProjectedRoot(
                CreateFluentCardNode("Initial Fluent content"));
            var firstChild = (TextBlock)firstCard.Content;
            var secondCard = (FluentCard)renderer.RenderProjectedRoot(
                CreateFluentCardNode("Updated Fluent content"));
            var secondChild = (TextBlock)secondCard.Content;

            Assert.AreSame(firstCard, secondCard);
            Assert.AreSame(firstChild, secondChild);
            Assert.AreEqual("Updated Fluent content", secondChild.Text);
            Assert.IsNotNull(secondCard.Background);
            Assert.IsNotNull(secondCard.TranslationTransition);
            Assert.IsNotNull(secondCard.ScaleTransition);
            Assert.AreEqual(8, secondCard.CornerRadius.TopLeft);
        });
    }

    [TestMethod]
    public void Render_CsxamlCodePresenter_AppliesCodeAndTitleProperties()
    {
        WinUiTestEnvironment.Run(() =>
        {
            ExternalControlRegistry.Register(CreateCsxamlCodePresenterDescriptor());
            var renderer = new WinUiNodeRenderer();

            var firstPresenter = (CsxamlCodePresenter)renderer.RenderProjectedRoot(
                CreateCsxamlCodePresenterNode("Slots", "component Element Card { render <Grid />; }"));
            var secondPresenter = (CsxamlCodePresenter)renderer.RenderProjectedRoot(
                CreateCsxamlCodePresenterNode(null, "render <Button Content=\"Run\" />;"));

            Assert.AreSame(firstPresenter, secondPresenter);
            Assert.AreEqual("CSXAML", secondPresenter.Title);
            Assert.AreEqual("render <Button Content=\"Run\" />;", secondPresenter.Code);
            Assert.IsInstanceOfType<StackPanel>(secondPresenter.Content);
        });
    }

    private static NativeElementNode CreateFluentCardNode(string childText)
    {
        return new NativeElementNode(
            typeof(FluentCard).FullName!,
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    null,
                    [new NativePropertyValue("Text", childText, ValueKindHint.String)],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>())
            ]);
    }

    private static NativeElementNode CreateCsxamlCodePresenterNode(
        string? title,
        string code)
    {
        var properties = new List<NativePropertyValue>
        {
            new("Code", code, ValueKindHint.String)
        };
        if (title is not null)
        {
            properties.Add(new NativePropertyValue("Title", title, ValueKindHint.String));
        }

        return new NativeElementNode(
            typeof(CsxamlCodePresenter).FullName!,
            null,
            properties,
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }

    private static ExternalControlDescriptor CreateFluentCardDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(FluentCard),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(FluentCard).FullName!,
                typeof(FluentCard).FullName!,
                typeof(ContentControl).FullName,
                ControlChildKind.Single,
                Array.Empty<PropertyMetadata>(),
                Array.Empty<EventMetadata>()));
    }

    private static ExternalControlDescriptor CreateCsxamlCodePresenterDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(CsxamlCodePresenter),
            new Csxaml.ControlMetadata.ControlMetadata(
                typeof(CsxamlCodePresenter).FullName!,
                typeof(CsxamlCodePresenter).FullName!,
                typeof(ContentControl).FullName,
                ControlChildKind.None,
                [
                    new PropertyMetadata("Code", typeof(string).FullName!, true, false, false, true, ValueKindHint.String),
                    new PropertyMetadata("Title", typeof(string).FullName!, true, false, false, true, ValueKindHint.String)
                ],
                Array.Empty<EventMetadata>()));
    }
}
