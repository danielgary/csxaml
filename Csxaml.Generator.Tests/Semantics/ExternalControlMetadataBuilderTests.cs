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
            Assert.AreEqual(ControlContentKind.Single, metadata.Content.Kind);
            Assert.AreEqual("Content", metadata.Content.DefaultPropertyName);
            Assert.AreEqual(ControlContentSource.Convention, metadata.Content.Source);
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "BadgeText");
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "Margin");
            CollectionAssert.Contains(metadata.Properties.Select(property => property.Name).ToList(), "Style");
            Assert.AreEqual(
                ValueKindHint.Style,
                metadata.Properties.Single(property => property.Name == "Style").ValueKindHint);
            var click = metadata.Events.Single(@event => @event.ExposedName == "OnClick");
            Assert.AreEqual(typeof(Action).FullName, click.HandlerTypeName);
            Assert.AreEqual(EventBindingKind.Direct, click.BindingKind);
            var statusChanged = metadata.Events.Single(@event => @event.ExposedName == "OnStatusChanged");
            Assert.AreEqual(
                "System.Action<MyApp.Controls.StatusChangedEventArgs>",
                statusChanged.HandlerTypeName);
            Assert.AreEqual(EventBindingKind.EventArgs, statusChanged.BindingKind);
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

        [TestMethod]
        public void TryBuild_ContentPropertyAttribute_UsesNamedSingleProperty()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.ControlExample), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual(ControlChildKind.Single, metadata.ChildKind);
            Assert.AreEqual(ControlContentKind.Single, metadata.Content.Kind);
            Assert.AreEqual("Example", metadata.Content.DefaultPropertyName);
            Assert.AreEqual("Microsoft.UI.Xaml.UIElement", metadata.Content.PropertyTypeName);
            Assert.AreEqual(ControlContentSource.ContentPropertyAttribute, metadata.Content.Source);
            Assert.IsTrue(metadata.Properties.Any(property => property.Name == "Example"));
            Assert.IsTrue(metadata.Properties.Any(property => property.Name == "Options"));
        }

        [TestMethod]
        public void TryBuild_InheritedContentPropertyAttribute_UsesBaseProperty()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.DerivedControlExample), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual(ControlChildKind.Single, metadata.ChildKind);
            Assert.AreEqual("Example", metadata.Content.DefaultPropertyName);
            Assert.AreEqual(ControlContentSource.ContentPropertyAttribute, metadata.Content.Source);
        }

        [TestMethod]
        public void TryBuild_ContentPropertyAttribute_PrefersAttributeOverContentConvention()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.AttributeBeatsContentConvention), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Example", metadata.Content.DefaultPropertyName);
            Assert.AreEqual(ControlContentSource.ContentPropertyAttribute, metadata.Content.Source);
        }

        [TestMethod]
        public void TryBuild_ChildrenCollectionConvention_RecordsCollectionContent()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.ChildrenHost), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual(ControlChildKind.Multiple, metadata.ChildKind);
            Assert.AreEqual(ControlContentKind.Collection, metadata.Content.Kind);
            Assert.AreEqual("Children", metadata.Content.DefaultPropertyName);
            Assert.AreEqual("Microsoft.UI.Xaml.Controls.UIElementCollection", metadata.Content.PropertyTypeName);
            Assert.AreEqual("Microsoft.UI.Xaml.UIElement", metadata.Content.ItemTypeName);
            Assert.AreEqual(ControlContentSource.Convention, metadata.Content.Source);
        }

        [TestMethod]
        public void TryBuild_UnsupportedContentPropertyType_RecordsUnsupportedContentProperty()
        {
            var builder = new ExternalControlMetadataBuilder();

            var success = builder.TryBuild(typeof(MyApp.Controls.UnsupportedContentExample), out var metadata, out var reason);

            Assert.IsTrue(success, reason);
            Assert.IsNotNull(metadata);
            Assert.AreEqual(ControlChildKind.None, metadata.ChildKind);
            Assert.AreEqual(ControlContentKind.None, metadata.Content.Kind);
            Assert.AreEqual("UnsupportedContent", metadata.Content.DefaultPropertyName);
            Assert.AreEqual(typeof(int).FullName, metadata.Content.PropertyTypeName);
            Assert.AreEqual(ControlContentSource.ContentPropertyAttribute, metadata.Content.Source);
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

    public class UIElement
    {
    }

    public class FrameworkElement : UIElement
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
    public sealed class UIElementCollection
    {
    }

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

namespace Microsoft.UI.Xaml.Markup
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}

namespace MyApp.Controls
{
    using Microsoft.UI.Xaml.Markup;

    public sealed class InfoBanner : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public static global::Microsoft.UI.Xaml.DependencyProperty IsOpenProperty { get; } =
            new global::Microsoft.UI.Xaml.DependencyProperty();

        public bool IsOpen { get; set; }
    }

    public sealed class StatusButton : global::Microsoft.UI.Xaml.Controls.Button
    {
        public static readonly object BadgeTextProperty = new();

        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        public string? BadgeText { get; set; }

        public void RaiseStatusChanged()
        {
            StatusChanged?.Invoke(this, new StatusChangedEventArgs());
        }
    }

    public sealed class StatusChangedEventArgs : EventArgs
    {
    }

    [ContentProperty(Name = nameof(Example))]
    public sealed class ControlExample : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public global::Microsoft.UI.Xaml.UIElement? Example { get; set; }

        public global::Microsoft.UI.Xaml.UIElement? Options { get; set; }
    }

    [ContentProperty(Name = nameof(Example))]
    public class ControlExampleBase : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public global::Microsoft.UI.Xaml.UIElement? Example { get; set; }
    }

    public sealed class DerivedControlExample : ControlExampleBase
    {
    }

    [ContentProperty(Name = nameof(Example))]
    public sealed class AttributeBeatsContentConvention : global::Microsoft.UI.Xaml.Controls.ContentControl
    {
        public global::Microsoft.UI.Xaml.UIElement? Example { get; set; }
    }

    public sealed class ChildrenHost : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public global::Microsoft.UI.Xaml.Controls.UIElementCollection Children { get; } = new();
    }

    [ContentProperty(Name = nameof(UnsupportedContent))]
    public sealed class UnsupportedContentExample : global::Microsoft.UI.Xaml.FrameworkElement
    {
        public int UnsupportedContent { get; set; }
    }
}
