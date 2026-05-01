using Csxaml.ExternalControls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
public sealed class ExternalChildSetterTests
{
    [TestMethod]
    public void Set_UnsupportedContentMetadata_FailsWithContentPropertyName()
    {
        var setter = ExternalChildSetter.Create(CreateUnsupportedContentDescriptor());

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => setter.Set(null!, new UIElement[] { null! }));

        StringAssert.Contains(exception.Message, "UnsupportedContent");
        StringAssert.Contains(exception.Message, "unsupported type");
    }

    [TestMethod]
    public void Set_SingleContent_SkipsSameChildInstance()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var setter = ExternalChildSetter.Create(CreateSingleContentDescriptor());
            var control = new CountingContentControl();
            var child = new TextBlock();

            setter.Set(control, [child]);
            setter.Set(control, [child]);

            Assert.AreEqual(1, control.ContentSetCount);
            Assert.AreSame(child, control.Content);
        });
    }

    private static ExternalControlDescriptor CreateUnsupportedContentDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(StatusButton),
            new Csxaml.ControlMetadata.ControlMetadata(
                "UnsupportedContentStatusButton",
                typeof(StatusButton).FullName!,
                typeof(Button).FullName,
                ControlChildKind.None,
                new ControlContentMetadata(
                    "UnsupportedContent",
                    ControlContentKind.None,
                    typeof(int).FullName!,
                    null,
                    ControlContentSource.ContentPropertyAttribute),
                Array.Empty<Csxaml.ControlMetadata.PropertyMetadata>(),
                Array.Empty<EventMetadata>()));
    }

    private static ExternalControlDescriptor CreateSingleContentDescriptor()
    {
        return new ExternalControlDescriptor(
            typeof(CountingContentControl),
            new Csxaml.ControlMetadata.ControlMetadata(
                "CountingContentControl",
                typeof(CountingContentControl).FullName!,
                typeof(ContentControl).FullName,
                ControlChildKind.Single,
                new ControlContentMetadata(
                    "Content",
                    ControlContentKind.Single,
                    typeof(object).FullName!,
                    null,
                    ControlContentSource.BuiltInMetadata),
                Array.Empty<Csxaml.ControlMetadata.PropertyMetadata>(),
                Array.Empty<EventMetadata>()));
    }

    private sealed class CountingContentControl : ContentControl
    {
        public int ContentSetCount { get; private set; }

        public new object? Content
        {
            get => base.Content;
            set
            {
                ContentSetCount++;
                base.Content = value;
            }
        }
    }
}
