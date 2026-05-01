using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

/// <summary>
/// Handles physical wheel input that Windows routes through the focused window
/// instead of the XAML island HWND currently under the pointer.
/// </summary>
internal sealed class ThreadMouseWheelBridge : IDisposable
{
    private const int MouseHookId = 7;
    private const uint MouseWheelMessageId = 0x020A;
    private const uint PointerWheelMessageId = 0x024E;

    private readonly IntPtr _windowHandle;
    private readonly Func<UIElement?> _getRootElement;
    private readonly MouseHookProcedure _hookProcedure;
    private IntPtr _hookHandle;
    private bool _isDisposed;

    private ThreadMouseWheelBridge(Window window, Func<UIElement?> getRootElement)
    {
        _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _getRootElement = getRootElement;
        _hookProcedure = HandleMouseHook;
        var threadId = GetWindowThreadProcessId(_windowHandle, out _);
        _hookHandle = SetWindowsHookEx(MouseHookId, _hookProcedure, IntPtr.Zero, threadId);
    }

    public static ThreadMouseWheelBridge Attach(Window window, Func<UIElement?> getRootElement)
    {
        return new ThreadMouseWheelBridge(window, getRootElement);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    private IntPtr HandleMouseHook(int code, UIntPtr wParam, IntPtr lParam)
    {
        if (code >= 0 && IsWheelMessage((uint)wParam))
        {
            ScheduleFallback(lParam);
        }

        return CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private void ScheduleFallback(IntPtr lParam)
    {
        var rootElement = _getRootElement();
        if (rootElement is null)
        {
            return;
        }

        var message = MouseHookWheelMessage.FromNative(lParam);
        var scroller = MouseWheelFallback.FindTarget(
            rootElement,
            _windowHandle,
            message.ScreenX,
            message.ScreenY);
        if (scroller is null)
        {
            return;
        }

        var previousOffset = scroller.VerticalOffset;
        rootElement.DispatcherQueue.TryEnqueue(() =>
            MouseWheelFallback.TryScrollIfUnchanged(
                scroller,
                previousOffset,
                message.Delta));
    }

    private static bool IsWheelMessage(uint message)
    {
        return message is MouseWheelMessageId or PointerWheelMessageId;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowsHookExW", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int hookId,
        MouseHookProcedure hookProcedure,
        IntPtr moduleHandle,
        uint threadId);

    [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hookHandle);

    [DllImport("user32.dll", EntryPoint = "CallNextHookEx")]
    private static extern IntPtr CallNextHookEx(
        IntPtr hookHandle,
        int code,
        UIntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
    private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr MouseHookProcedure(int code, UIntPtr wParam, IntPtr lParam);
}
