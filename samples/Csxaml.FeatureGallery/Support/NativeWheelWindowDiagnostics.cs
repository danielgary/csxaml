using System.Runtime.InteropServices;
using System.Text;

namespace Csxaml.Samples.FeatureGallery;

internal static class NativeWheelWindowDiagnostics
{
    public static string Describe(
        NativeWheelMessage message,
        IntPtr rootHwnd,
        IntPtr childHwnd)
    {
        var rootDpi = GetDpiForWindow(rootHwnd);
        var childDpi = GetDpiForWindow(childHwnd);
        var screenPoint = new NativePoint(message.ScreenX, message.ScreenY);
        _ = GetCursorPos(out var cursorPoint);
        var rootClient = ToClient(rootHwnd, screenPoint);
        var childClient = ToClient(childHwnd, screenPoint);
        var cursorRootClient = ToClient(rootHwnd, cursorPoint);
        var cursorChildClient = ToClient(childHwnd, cursorPoint);
        var hwndAtPoint = WindowFromPoint(screenPoint);
        var hwndAtCursor = WindowFromPoint(cursorPoint);

        return string.Join(
            ", ",
            $"last={message.Name}",
            $"delta={message.WheelDelta}",
            $"screen=({message.ScreenX},{message.ScreenY})",
            $"rootPx=({rootClient.X},{rootClient.Y})",
            $"rootDip={ToDip(rootClient, rootDpi)}",
            $"childPx=({childClient.X},{childClient.Y})",
            $"childDip={ToDip(childClient, childDpi)}",
            $"cursor=({cursorPoint.X},{cursorPoint.Y})",
            $"cursorRootDip={ToDip(cursorRootClient, rootDpi)}",
            $"cursorChildDip={ToDip(cursorChildClient, childDpi)}",
            $"dpi root/child={rootDpi}/{childDpi}",
            $"hwndAtPoint='{ReadClassName(hwndAtPoint)}'",
            $"hwndAtCursor='{ReadClassName(hwndAtCursor)}'");
    }

    private static NativePoint ToClient(IntPtr hwnd, NativePoint screenPoint)
    {
        var clientPoint = screenPoint;
        _ = ScreenToClient(hwnd, ref clientPoint);
        return clientPoint;
    }

    private static string ToDip(NativePoint point, uint dpi)
    {
        if (dpi == 0)
        {
            return "(?,?)";
        }

        var scale = dpi / 96.0;
        return $"({point.X / scale:0.##},{point.Y / scale:0.##})";
    }

    private static string ReadClassName(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return "<none>";
        }

        var builder = new StringBuilder(256);
        _ = GetClassName(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(NativePoint point);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hwnd, ref NativePoint point);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hwnd, StringBuilder className, int maxCount);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint(int x, int y)
    {
        public int X = x;

        public int Y = y;
    }
}
