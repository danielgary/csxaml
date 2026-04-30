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
}
