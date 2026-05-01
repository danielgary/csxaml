using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

internal static class AutomationElementQueries
{
    public static AutomationElement? FindProcessWindow(int processId)
    {
        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.ProcessIdProperty, processId),
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

        return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
    }

    public static AutomationElement? FindByAutomationId(
        this AutomationElement root,
        string automationId)
    {
        return root.FindFirst(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));
    }

    public static IReadOnlyList<AutomationElement> FindAllByAutomationId(
        this AutomationElement root,
        string automationId)
    {
        var elements = root.FindAll(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

        return elements.Cast<AutomationElement>().ToArray();
    }

    public static AutomationElement? FindButtonByName(this AutomationElement root, string name)
    {
        var condition = new AndCondition(
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
            new PropertyCondition(AutomationElement.NameProperty, name));

        return root.FindFirst(TreeScope.Descendants, condition);
    }

    public static IReadOnlyList<string> ReadVisibleText(this AutomationElement root)
    {
        var elements = root.FindAll(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));

        var text = new List<string>();
        foreach (AutomationElement element in elements)
        {
            var name = element.Current.Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                text.Add(name);
            }
        }

        return text;
    }

    public static string? FindVisibleTextStartingWith(
        this AutomationElement root,
        string prefix)
    {
        return root.ReadVisibleText()
            .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.Ordinal));
    }

    public static string? ReadValue(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
        {
            return null;
        }

        return ((ValuePattern)pattern).Current.Value;
    }

    public static double? ReadVerticalScrollPercent(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern))
        {
            return null;
        }

        return ((ScrollPattern)pattern).Current.VerticalScrollPercent;
    }

    public static ScrollInfo? ReadScrollInfo(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern))
        {
            return null;
        }

        var current = ((ScrollPattern)pattern).Current;
        return new ScrollInfo(
            current.VerticallyScrollable,
            current.VerticalScrollPercent,
            current.VerticalViewSize);
    }

    public static double? ScrollDownWithPattern(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern))
        {
            return null;
        }

        ((ScrollPattern)pattern).Scroll(ScrollAmount.NoAmount, ScrollAmount.LargeIncrement);
        Thread.Sleep(250);
        return element.ReadVerticalScrollPercent();
    }

    public static void ScrollIntoView(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern))
        {
            return;
        }

        ((ScrollItemPattern)pattern).ScrollIntoView();
        Thread.Sleep(250);
    }

    public static string Describe(this AutomationElement element)
    {
        var current = element.Current;
        return string.Join(
            ", ",
            $"Name='{current.Name}'",
            $"AutomationId='{current.AutomationId}'",
            $"ControlType='{current.ControlType.ProgrammaticName}'",
            $"Bounds='{current.BoundingRectangle}'");
    }

    public static void Invoke(this AutomationElement element)
    {
        if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
        {
            Assert.Fail($"Element '{element.Current.Name}' does not support InvokePattern.");
        }

        ((InvokePattern)pattern).Invoke();
    }

    public static string DescribeFocusedElement()
    {
        var focusedElement = AutomationElement.FocusedElement;
        if (focusedElement is null)
        {
            return "<none>";
        }

        return string.Join(
            ", ",
            $"Name='{focusedElement.Current.Name}'",
            $"AutomationId='{focusedElement.Current.AutomationId}'",
            $"ControlType='{focusedElement.Current.ControlType.ProgrammaticName}'");
    }
}

internal sealed record ScrollInfo(
    bool VerticallyScrollable,
    double VerticalScrollPercent,
    double VerticalViewSize);
