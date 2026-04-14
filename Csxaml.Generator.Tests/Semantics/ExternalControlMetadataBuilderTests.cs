namespace Csxaml.Generator.Tests.Semantics
{
    [TestClass]
    public sealed class ExternalControlMetadataBuilderTests
    {
        [TestMethod]
        public void TryBuild_ControlDerivedFromBuiltInBase_InheritsSupportedBuiltInSurface()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.StatusButton), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual(ControlChildKind.Single, metadata.ChildKind);
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "BadgeText");
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "Margin");
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "Style");
            Assert.AreEqual(
                ValueKindHint.Style,
                metadata.Properties.Single(property => property.Name == "Style").ValueKindHint);
            CollectionAssert.Contains(metadata.Events.Select(@event => @event.ExposedName).ToList(), "OnClick");
        }

        [TestMethod]
        public void TryBuild_ControlWithDependencyPropertyIdentifierProperty_IncludesPropertyMetadata()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.InfoBanner), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "IsOpen");
        }
    }
}

namespace Microsoft.UI.Xaml
{
    public sealed class DependencyProperty
    {
    }

    public sealed class Style
    {
    }

    public class FrameworkElement
    {
        public static readonly object MarginProperty = new();
        public static readonly object StyleProperty = new();

        public Thickness Margin { get; set; }
        public Style? Style { get; set; }
    }

    public readonly struct Thickness
    {
    }
}

namespace Microsoft.UI.Xaml.Controls
{
    public class ContentControl : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public static readonly object ContentProperty = new();

        public object? Content { get; set; }
    }

    public class Button : ContentControl
    {
        public event Action? Click;

        public void RaiseClick()
        {
            Click?.Invoke();
        }
    }
}

namespace MyApp.Controls
{
    public sealed class InfoBanner : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public static global::Microsoft.UI.Xaml.DependencyProperty IsOpenProperty { get; } =
            new global::Microsoft.UI.Xaml.DependencyProperty();

        public bool IsOpen { get; set; }
    }

    public sealed class StatusButton : global::Microsoft.UI.Xaml.Controls.Button
    {
        public static readonly object BadgeTextProperty = new();

        public string? BadgeText { get; set; }
    }
}
