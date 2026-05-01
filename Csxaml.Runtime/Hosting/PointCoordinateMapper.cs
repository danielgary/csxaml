using System.Runtime.InteropServices;
using Windows.Foundation;

namespace Csxaml.Runtime;

internal static class PointCoordinateMapper
{
    public static Point ScreenToClient(IntPtr windowHandle, int screenX, int screenY)
    {
        var point = new NativePoint(screenX, screenY);
        ScreenToClient(windowHandle, ref point);
        return PhysicalPixelsToEffectivePixels(point.X, point.Y, GetDpiForWindow(windowHandle));
    }

    public static Point PhysicalPixelsToEffectivePixels(int x, int y, uint dpi)
    {
        if (dpi == 0 || dpi == 96)
        {
            return new Point(x, y);
        }

        var scale = 96.0 / dpi;
        return new Point(x * scale, y * scale);
    }

    [DllImport("user32.dll", EntryPoint = "ScreenToClient", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(IntPtr windowHandle, ref NativePoint point);

    [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")]
    private static extern uint GetDpiForWindow(IntPtr windowHandle);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;

        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
