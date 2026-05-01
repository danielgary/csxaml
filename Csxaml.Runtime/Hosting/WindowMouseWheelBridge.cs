using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

internal sealed class WindowMouseWheelBridge : IDisposable
{
    private const uint MouseWheelMessageId = 0x020A;
    private const uint PointerWheelMessageId = 0x024E;

    private static long s_nextSubclassId;

    private readonly IntPtr _windowHandle;
    private readonly Func<UIElement?> _getRootElement;
    private readonly WindowSubclassProcedure _subclassProcedure;
    private readonly Dictionary<IntPtr, UIntPtr> _subclassedWindows = new();
    private bool _isDisposed;

    private WindowMouseWheelBridge(Window window, Func<UIElement?> getRootElement)
    {
        _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _getRootElement = getRootElement;
        _subclassProcedure = HandleWindowMessage;
        RefreshTargets();
    }

    public static WindowMouseWheelBridge Attach(Window window, Func<UIElement?> getRootElement)
    {
        return new WindowMouseWheelBridge(window, getRootElement);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        foreach (var item in _subclassedWindows)
        {
            RemoveWindowSubclass(item.Key, _subclassProcedure, item.Value);
        }

        _subclassedWindows.Clear();
    }

    public void RefreshTargets()
    {
        if (_isDisposed)
        {
            return;
        }

        EnsureSubclass(_windowHandle);
        EnumChildWindows(
            _windowHandle,
            (childWindowHandle, _) =>
            {
                EnsureSubclass(childWindowHandle);
                return true;
            },
            IntPtr.Zero);
    }

    private IntPtr HandleWindowMessage(
        IntPtr windowHandle,
        uint message,
        UIntPtr wParam,
        IntPtr lParam,
        UIntPtr subclassId,
        UIntPtr referenceData)
    {
        if (!IsWheelMessage(message))
        {
            return DefSubclassProc(windowHandle, message, wParam, lParam);
        }

        return HandleMouseWheel(windowHandle, message, wParam, lParam);
    }

    private IntPtr HandleMouseWheel(
        IntPtr windowHandle,
        uint message,
        UIntPtr wParam,
        IntPtr lParam)
    {
        var rootElement = _getRootElement();
        if (rootElement is null)
        {
            return DefSubclassProc(windowHandle, message, wParam, lParam);
        }

        var wheel = MouseWheelMessage.FromNative(wParam, lParam);
        var scroller = MouseWheelFallback.FindTarget(
            rootElement,
            _windowHandle,
            wheel.ScreenX,
            wheel.ScreenY);
        if (scroller is null)
        {
            return DefSubclassProc(windowHandle, message, wParam, lParam);
        }

        var previousOffset = scroller.VerticalOffset;
        var result = DefSubclassProc(windowHandle, message, wParam, lParam);
        if (!ScrollViewerWheelScroller.OffsetsMatch(previousOffset, scroller.VerticalOffset))
        {
            return result;
        }

        return MouseWheelFallback.TryScrollIfUnchanged(scroller, previousOffset, wheel.Delta)
            ? IntPtr.Zero
            : result;
    }

    private static UIntPtr CreateSubclassId()
    {
        var id = Interlocked.Increment(ref s_nextSubclassId);
        return (UIntPtr)(ulong)id;
    }

    private static bool IsWheelMessage(uint message)
    {
        return message is MouseWheelMessageId or PointerWheelMessageId;
    }

    private void EnsureSubclass(IntPtr windowHandle)
    {
        if (_subclassedWindows.ContainsKey(windowHandle))
        {
            return;
        }

        var subclassId = CreateSubclassId();
        if (SetWindowSubclass(windowHandle, _subclassProcedure, subclassId, UIntPtr.Zero))
        {
            _subclassedWindows.Add(windowHandle, subclassId);
        }
    }

    [DllImport("comctl32.dll", EntryPoint = "SetWindowSubclass", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(
        IntPtr windowHandle,
        WindowSubclassProcedure subclassProcedure,
        UIntPtr subclassId,
        UIntPtr referenceData);

    [DllImport("comctl32.dll", EntryPoint = "RemoveWindowSubclass", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(
        IntPtr windowHandle,
        WindowSubclassProcedure subclassProcedure,
        UIntPtr subclassId);

    [DllImport("comctl32.dll", EntryPoint = "DefSubclassProc")]
    private static extern IntPtr DefSubclassProc(
        IntPtr windowHandle,
        uint message,
        UIntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "EnumChildWindows")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(
        IntPtr parentWindowHandle,
        EnumChildWindowProcedure enumProcedure,
        IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WindowSubclassProcedure(
        IntPtr windowHandle,
        uint message,
        UIntPtr wParam,
        IntPtr lParam,
        UIntPtr subclassId,
        UIntPtr referenceData);

    private delegate bool EnumChildWindowProcedure(IntPtr windowHandle, IntPtr lParam);
}
