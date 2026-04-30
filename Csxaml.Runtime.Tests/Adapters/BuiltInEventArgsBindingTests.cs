using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Csxaml.Runtime.Tests.Adapters;

[TestClass]
[DoNotParallelize]
public sealed class BuiltInEventArgsBindingTests
{
    [TestMethod]
    public void Render_Slider_BindsValueChangedEventArgsHandler()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();
            var observed = new List<double>();

            var firstSlider = (Slider)renderer.RenderProjectedRoot(
                CreateSliderNode(args => observed.Add(args.NewValue)));
            firstSlider.Value = 10;

            var secondSlider = (Slider)renderer.RenderProjectedRoot(
                CreateSliderNode(args => observed.Add(args.NewValue + 1)));
            secondSlider.Value = 20;

            renderer.RenderProjectedRoot(CreateSliderNode(null));
            secondSlider.Value = 30;

            CollectionAssert.AreEqual(new[] { 10d, 21d }, observed);
            Assert.AreSame(firstSlider, secondSlider);
        });
    }

    [TestMethod]
    public void Render_Slider_DoesNotDispatchValueChangedForRenderedValue()
    {
        WinUiTestEnvironment.Run(() =>
        {
            var renderer = new WinUiNodeRenderer();
            var observed = new List<double>();

            var slider = (Slider)renderer.RenderProjectedRoot(
                CreateSliderNode(args => observed.Add(args.NewValue), value: 10));
            var retained = (Slider)renderer.RenderProjectedRoot(
                CreateSliderNode(args => observed.Add(args.NewValue), value: 20));

            Assert.AreEqual(20d, retained.Value);
            retained.Value = 30;

            Assert.AreSame(slider, retained);
            Assert.AreEqual(30d, retained.Value);
            CollectionAssert.AreEqual(new[] { 30d }, observed);
        });
    }

    private static NativeElementNode CreateSliderNode(
        Action<RangeBaseValueChangedEventArgs>? handler,
        double? value = null)
    {
        var events = handler is null
            ? Array.Empty<NativeEventValue>()
            : [new NativeEventValue("OnValueChanged", handler, ValueKindHint.Unknown)];
        var properties = value is null
            ? Array.Empty<NativePropertyValue>()
            : [new NativePropertyValue("Value", value.Value, ValueKindHint.Double)];

        return new NativeElementNode(
            "Slider",
            null,
            properties,
            events,
            Array.Empty<Node>());
    }
}
