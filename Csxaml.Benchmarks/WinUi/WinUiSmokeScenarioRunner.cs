using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Benchmarks;

internal sealed class WinUiSmokeScenarioRunner
{
    private readonly int _iterations;
    private readonly Window _window;

    public WinUiSmokeScenarioRunner(Window window, int iterations)
    {
        _window = window;
        _iterations = iterations;
    }

    public WinUiSmokeReport Run()
    {
        return new WinUiSmokeReport(
            DateTimeOffset.UtcNow,
            _iterations,
            "completed",
            null,
            [
                MeasureControlledTextBoxUpdates(),
                MeasureSiblingListChurn(),
                MeasureRetainedTextBoxPatch(),
                MeasureTextBoxReplacement()
            ]);
    }

    private WinUiSmokeScenarioResult MeasureControlledTextBoxUpdates()
    {
        using var renderer = new WinUiNodeRenderer();
        var initialRoot = RenderEditorRoot(renderer, "editor", "Draft plan 00");
        var editor = (TextBox)initialRoot.Children[0];
        if (!TryFocus(editor))
        {
            return CreateUnavailable("controlled_textbox_updates", "Unable to focus the retained TextBox.");
        }

        editor.SelectionStart = 2;
        editor.SelectionLength = 4;

        var lastRoot = initialRoot;
        var lastEditor = editor;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < _iterations; i++)
        {
            lastRoot = RenderEditorRoot(renderer, "editor", $"Draft plan {i % 100:00}");
            lastEditor = (TextBox)lastRoot.Children[0];
        }

        stopwatch.Stop();
        return new WinUiSmokeScenarioResult(
            "controlled_textbox_updates",
            "passed",
            GetAverageMicroseconds(stopwatch),
            ReferenceEquals(initialRoot, lastRoot),
            ReferenceEquals(editor, lastEditor),
            IsFocused(lastEditor),
            lastEditor.SelectionStart == 2 && lastEditor.SelectionLength == 4,
            null);
    }

    private WinUiSmokeScenarioResult MeasureSiblingListChurn()
    {
        using var renderer = new WinUiNodeRenderer();
        var orderedKeys = CreateOrderedKeys(100);
        var reversedKeys = orderedKeys.Reverse().ToArray();
        var initialRoot = RenderBoardRoot(renderer, orderedKeys, "editor", "Draft plan 00");
        var editor = (TextBox)initialRoot.Children[1];
        if (!TryFocus(editor))
        {
            return CreateUnavailable("sibling_list_churn", "Unable to focus the retained editor.");
        }

        editor.SelectionStart = 1;
        editor.SelectionLength = 5;

        var lastRoot = initialRoot;
        var lastEditor = editor;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < _iterations; i++)
        {
            var keys = i % 2 == 0 ? reversedKeys : orderedKeys;
            lastRoot = RenderBoardRoot(renderer, keys, "editor", "Draft plan 00");
            lastEditor = (TextBox)lastRoot.Children[1];
        }

        stopwatch.Stop();
        return new WinUiSmokeScenarioResult(
            "sibling_list_churn",
            "passed",
            GetAverageMicroseconds(stopwatch),
            ReferenceEquals(initialRoot, lastRoot),
            ReferenceEquals(editor, lastEditor),
            IsFocused(lastEditor),
            lastEditor.SelectionStart == 1 && lastEditor.SelectionLength == 5,
            null);
    }

    private WinUiSmokeScenarioResult MeasureRetainedTextBoxPatch()
    {
        using var renderer = new WinUiNodeRenderer();
        var initialRoot = RenderEditorRoot(renderer, "editor", "Draft plan 00");
        var editor = (TextBox)initialRoot.Children[0];
        var lastRoot = initialRoot;
        var lastEditor = editor;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < _iterations; i++)
        {
            lastRoot = RenderEditorRoot(renderer, "editor", $"Draft plan {i % 100:00}");
            lastEditor = (TextBox)lastRoot.Children[0];
        }

        stopwatch.Stop();
        return new WinUiSmokeScenarioResult(
            "retained_textbox_patch",
            "passed",
            GetAverageMicroseconds(stopwatch),
            ReferenceEquals(initialRoot, lastRoot),
            ReferenceEquals(editor, lastEditor),
            false,
            false,
            null);
    }

    private WinUiSmokeScenarioResult MeasureTextBoxReplacement()
    {
        using var renderer = new WinUiNodeRenderer();
        var initialRoot = RenderEditorRoot(renderer, "editor-0", "Draft plan 00");
        var editor = (TextBox)initialRoot.Children[0];
        var lastRoot = initialRoot;
        var lastEditor = editor;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < _iterations; i++)
        {
            lastRoot = RenderEditorRoot(renderer, $"editor-{i + 1}", $"Draft plan {i % 100:00}");
            lastEditor = (TextBox)lastRoot.Children[0];
        }

        stopwatch.Stop();
        var replacementNote = ReferenceEquals(editor, lastEditor)
            ? "Editor was unexpectedly retained."
            : null;
        return new WinUiSmokeScenarioResult(
            "textbox_replacement",
            "passed",
            GetAverageMicroseconds(stopwatch),
            ReferenceEquals(initialRoot, lastRoot),
            ReferenceEquals(editor, lastEditor),
            false,
            false,
            replacementNote);
    }

    private static string[] CreateOrderedKeys(int count)
    {
        var keys = new string[count];
        for (var i = 0; i < count; i++)
        {
            keys[i] = $"todo-{i:0000}";
        }

        return keys;
    }

    private WinUiSmokeScenarioResult CreateUnavailable(string name, string note)
    {
        return new WinUiSmokeScenarioResult(name, "unavailable", 0, false, false, false, false, note);
    }

    private StackPanel RenderBoardRoot(
        WinUiNodeRenderer renderer,
        IReadOnlyList<string> itemKeys,
        string editorKey,
        string editorText)
    {
        return RenderRoot(
            renderer,
            WinUiProjectionScenarioNodes.CreateBoardNode(itemKeys, editorKey, editorText));
    }

    private StackPanel RenderEditorRoot(WinUiNodeRenderer renderer, string editorKey, string editorText)
    {
        return RenderRoot(
            renderer,
            WinUiProjectionScenarioNodes.CreateEditorHostNode(editorKey, editorText));
    }

    private StackPanel RenderRoot(WinUiNodeRenderer renderer, NativeElementNode node)
    {
        var root = (StackPanel)renderer.RenderProjectedRoot(node);
        _window.Content = root;
        _window.Activate();
        root.UpdateLayout();
        return root;
    }

    private double GetAverageMicroseconds(Stopwatch stopwatch)
    {
        return stopwatch.Elapsed.TotalMilliseconds * 1000d / _iterations;
    }

    private static bool IsFocused(Control control)
    {
        return control.FocusState != FocusState.Unfocused;
    }

    private static bool TryFocus(Control control)
    {
        return control.Focus(FocusState.Programmatic);
    }
}
