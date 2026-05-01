using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Csxaml.Samples.WheelProbe;

internal static class NativeWheelWindowFinder
{
    private const string WinUiWindowClass = "WinUIDesktopWin32WindowClass";

    public static IntPtr FindMainWindow()
    {
        using var process = Process.GetCurrentProcess();
        process.Refresh();
        if (process.MainWindowHandle != IntPtr.Zero)
        {
            return process.MainWindowHandle;
        }

        var processId = process.Id;
        var match = IntPtr.Zero;
        EnumWindows((hwnd, _) =>
        {
            GetWindowThreadProcessId(hwnd, out var ownerProcessId);
            if (ownerProcessId == processId && ReadClassName(hwnd) == WinUiWindowClass)
            {
                match = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return match;
    }

    public static IntPtr FindChildByClass(IntPtr root, string className)
    {
        var match = IntPtr.Zero;
        EnumChildWindows(root, (hwnd, _) =>
        {
            if (ReadClassName(hwnd) == className)
            {
                match = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return match;
    }

    private static string ReadClassName(IntPtr hwnd)
    {
        var builder = new StringBuilder(256);
        _ = GetClassName(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hwnd, EnumWindowProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hwnd, StringBuilder className, int maxCount);
}
