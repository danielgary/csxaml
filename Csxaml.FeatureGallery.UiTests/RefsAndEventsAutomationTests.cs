using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

[TestClass]
[TestCategory("UIAutomation")]
public sealed class RefsAndEventsAutomationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public void TextBoxReportsPhysicalKeyPressWithoutReloadingPage()
    {
        using var app = FeatureGalleryAppSession.Launch(TestContext);
        app.NavigateToRefsAndEvents();

        var textBox = app.WaitForAutomationId("FeatureGallerySearchBox");
        app.WriteVisibleText("Before key input:");
        TestContext.WriteLine($"TextBox target: {textBox.Describe()}");
        TestContext.WriteLine($"Element under TextBox center: {NativeInput.DescribeElementAtCenter(textBox)}");
        TestContext.WriteLine($"TextBox runtime id before key: {FormatRuntimeId(textBox)}");

        NativeInput.LeftClickCenter(app.MainWindowHandle, textBox);
        TestContext.WriteLine($"Focused before key: {AutomationElementQueries.DescribeFocusedElement()}");

        NativeInput.PressA(app.MainWindowHandle);
        app.RefreshWindow();

        var keyStatus = WaitForVisibleTextStartingWith(app, "KeyDown:");
        var afterTextBox = app.WaitForAutomationId("FeatureGallerySearchBox");
        var textBoxValue = afterTextBox.ReadValue();

        app.WriteVisibleText("After key input:");
        TestContext.WriteLine($"Focused after key: {AutomationElementQueries.DescribeFocusedElement()}");
        TestContext.WriteLine($"TextBox value after key: '{textBoxValue}'.");
        TestContext.WriteLine($"TextBox runtime id after key: {FormatRuntimeId(afterTextBox)}");

        Assert.AreEqual("KeyDown: A", keyStatus);
        Assert.AreEqual(
            "FeatureGallerySearchBox",
            AutomationElement.FocusedElement.Current.AutomationId,
            "The TextBox should keep focus after a physical keypress.");
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public void ListViewReceivesPhysicalClickInsideFluentCard()
    {
        using var app = FeatureGalleryAppSession.Launch(TestContext);
        app.NavigateToRefsAndEvents();

        var listView = app.WaitForAutomationId("RefsAndEventsEventList");
        TestContext.WriteLine($"ListView target: {listView.Describe()}");
        TestContext.WriteLine($"Element under ListView center: {NativeInput.DescribeElementAtCenter(listView)}");

        NativeInput.LeftClickCenter(app.MainWindowHandle, listView);
        app.RefreshWindow();

        var listStatus = WaitForVisibleTextStartingWith(app, "SelectionChanged:") ??
            WaitForVisibleTextStartingWith(app, "ItemClick:");

        app.WriteVisibleText("Visible text after ListView click:");
        Assert.IsNotNull(listStatus, "Expected a physical click inside the ListView to update the list event status.");
    }

    [TestMethod]
    [Timeout(60000, CooperativeCancellation = true)]
    public void ListViewScrollsWhenPhysicalMouseWheelTargetsItInsideFluentCard()
    {
        using var app = FeatureGalleryAppSession.Launch(TestContext);
        app.NavigateToRefsAndEvents();

        var listView = app.WaitForAutomationId("RefsAndEventsEventList");
        var before = listView.ReadVerticalScrollPercent();
        var beforeInfo = listView.ReadScrollInfo();
        Assert.IsNotNull(before, "Refs and events ListView does not expose ScrollPattern.");

        TestContext.WriteLine($"ListView target: {listView.Describe()}");
        TestContext.WriteLine($"Element under ListView center: {NativeInput.DescribeElementAtCenter(listView)}");
        TestContext.WriteLine($"ListView scroll info before wheel: {beforeInfo}");
        TestContext.WriteLine($"ListView vertical scroll before wheel: {before:0.##}");
        NativeInput.WheelDownOver(app.MainWindowHandle, listView);
        app.RefreshWindow();

        var scrolled = AutomationWait.UntilTrue(
            () =>
            {
                app.RefreshWindow();
                var currentListView = app.Window.FindByAutomationId("RefsAndEventsEventList");
                if (currentListView is null)
                {
                    return false;
                }

                var current = currentListView.ReadVerticalScrollPercent();
                return current.HasValue && current.Value > before.Value;
            },
            TimeSpan.FromSeconds(3));

        var after = app.WaitForAutomationId("RefsAndEventsEventList").ReadVerticalScrollPercent();
        TestContext.WriteLine($"ListView vertical scroll after wheel: {after:0.##}");
        TestContext.WriteLine($"ListView scroll info after wheel: {app.WaitForAutomationId("RefsAndEventsEventList").ReadScrollInfo()}");

        if (!scrolled)
        {
            NativeInput.LeftClickCenter(app.MainWindowHandle, app.WaitForAutomationId("RefsAndEventsEventList"));
            NativeInput.WheelDownOver(app.MainWindowHandle, app.WaitForAutomationId("RefsAndEventsEventList"));
            var afterFocusedWheel = app.WaitForAutomationId("RefsAndEventsEventList").ReadVerticalScrollPercent();
            TestContext.WriteLine($"ListView vertical scroll after click plus wheel: {afterFocusedWheel:0.##}");

            var patternAfter = app.WaitForAutomationId("RefsAndEventsEventList").ScrollDownWithPattern();
            TestContext.WriteLine($"ListView vertical scroll after UIA ScrollPattern.Scroll: {patternAfter:0.##}");
        }

        app.WriteVisibleText("Visible text after wheel:");

        Assert.IsTrue(scrolled, $"Expected the ListView scroll percent to increase after a physical mouse wheel. Before: {before:0.##}; after: {after:0.##}.");
    }

    private static string? WaitForVisibleTextStartingWith(
        FeatureGalleryAppSession app,
        string prefix)
    {
        string? found = null;

        AutomationWait.UntilTrue(
            () =>
            {
                app.RefreshWindow();
                found = app.Window.FindVisibleTextStartingWith(prefix);
                return found is not null;
            },
            TimeSpan.FromSeconds(3));

        return found;
    }

    private static string FormatRuntimeId(AutomationElement element)
    {
        return string.Join(".", element.GetRuntimeId());
    }
}
