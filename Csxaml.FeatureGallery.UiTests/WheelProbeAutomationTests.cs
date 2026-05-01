using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

[TestClass]
[TestCategory("UIAutomation")]
public sealed class WheelProbeAutomationTests
{
    private static readonly WheelProbeTarget[] Targets =
    [
        new("Bare", "RefsAndEventsBareWheelList"),
        new("Border", "RefsAndEventsBorderWheelList"),
        new("Fluent", "RefsAndEventsFluentWheelList"),
        new("GallerySection Fluent", "RefsAndEventsEventList")
    ];

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public void ComparePhysicalWheelRoutingAcrossListViewWrappers()
    {
        using var app = FeatureGalleryAppSession.Launch(TestContext);
        app.NavigateToRefsAndEvents();

        foreach (var target in Targets)
        {
            ResetWheelProbe(app);
            ResetNativeWheelProbe(app);
            ProbeWheelTarget(app, target);
        }
    }

    private void ProbeWheelTarget(
        FeatureGalleryAppSession app,
        WheelProbeTarget target)
    {
        var listView = WaitForTarget(app, target);
        var before = listView.ReadVerticalScrollPercent();

        TestContext.WriteLine($"[{target.Name}] target: {listView.Describe()}");
        TestContext.WriteLine($"[{target.Name}] element at center: {NativeInput.DescribeElementAtCenter(listView)}");
        TestContext.WriteLine($"[{target.Name}] HWND at center: {NativeWindowProbe.DescribeWindowAtCenter(listView)}");
        TestContext.WriteLine($"[{target.Name}] scroll before wheel: {before:0.##}");
        TestContext.WriteLine($"[{target.Name}] native before wheel: {ReadNativeWheelMessages(app)}");

        var wheelSent = NativeInput.TryWheelDownOver(
            app.MainWindowHandle,
            WaitForTarget(app, target),
            out var wheelInputResult);
        app.RefreshWindow();

        var after = app.WaitForAutomationId(target.AutomationId).ReadVerticalScrollPercent();
        var route = ReadWheelRoute(app);
        var nativeMessages = ReadNativeWheelMessages(app);

        TestContext.WriteLine($"[{target.Name}] physical wheel input: {wheelInputResult}");
        TestContext.WriteLine($"[{target.Name}] scroll after wheel: {after:0.##}");
        TestContext.WriteLine($"[{target.Name}] route status: {route}");
        TestContext.WriteLine($"[{target.Name}] native after wheel: {nativeMessages}");

        if (wheelSent && after == before)
        {
            ResetWheelProbe(app);
            ResetNativeWheelProbe(app);
            listView = WaitForTarget(app, target);
            var clickSent = NativeInput.TryLeftClickCenter(
                app.MainWindowHandle,
                listView,
                out var clickInputResult);
            var clickWheelInputResult = "wheel skipped because click input failed";
            var clickWheelSent = clickSent &&
                NativeInput.TryWheelDownOver(
                    app.MainWindowHandle,
                    WaitForTarget(app, target),
                    out clickWheelInputResult);
            app.RefreshWindow();

            var afterClickWheel = app.WaitForAutomationId(target.AutomationId).ReadVerticalScrollPercent();
            var clickWheelRoute = ReadWheelRoute(app);
            var clickWheelNativeMessages = ReadNativeWheelMessages(app);

            TestContext.WriteLine($"[{target.Name}] physical click input: {clickInputResult}");
            TestContext.WriteLine($"[{target.Name}] physical click-wheel input: {clickWheelInputResult} (sent={clickWheelSent})");
            TestContext.WriteLine($"[{target.Name}] scroll after click plus wheel: {afterClickWheel:0.##}");
            TestContext.WriteLine($"[{target.Name}] route after click plus wheel: {clickWheelRoute}");
            TestContext.WriteLine($"[{target.Name}] native after click plus wheel: {clickWheelNativeMessages}");

            var patternAfter = app.WaitForAutomationId(target.AutomationId).ScrollDownWithPattern();
            TestContext.WriteLine($"[{target.Name}] scroll after UIA ScrollPattern.Scroll: {patternAfter:0.##}");
        }
    }

    private static void ResetWheelProbe(FeatureGalleryAppSession app)
    {
        app.WaitForAutomationId("RefsAndEventsResetWheelProbe").Invoke();
        app.RefreshWindow();
    }

    private static string ReadWheelRoute(FeatureGalleryAppSession app)
    {
        return app.WaitForAutomationId("RefsAndEventsWheelRouteStatus").Current.Name;
    }

    private static AutomationElement WaitForTarget(
        FeatureGalleryAppSession app,
        WheelProbeTarget target)
    {
        var listView = app.WaitForAutomationId(target.AutomationId);
        listView.ScrollIntoView();
        app.RefreshWindow();

        var visibleElement = app.Window
            .FindAllByAutomationId(target.AutomationId)
            .FirstOrDefault(element => !element.Current.BoundingRectangle.IsEmpty);

        return visibleElement ?? app.WaitForAutomationId(target.AutomationId);
    }

    private static void ResetNativeWheelProbe(FeatureGalleryAppSession app)
    {
        app.WaitForAutomationId("RefsAndEventsResetNativeWheelProbe").Invoke();
        app.RefreshWindow();
    }

    private static string ReadNativeWheelMessages(FeatureGalleryAppSession app)
    {
        return app.WaitForAutomationId("RefsAndEventsNativeWheelStatus").Current.Name;
    }

    private sealed record WheelProbeTarget(string Name, string AutomationId);
}
