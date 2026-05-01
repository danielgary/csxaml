using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

internal static class NativeWindowProbe
{
    public static string DescribeWindowAtCenter(AutomationElement element)
    {
        var rectangle = element.Current.BoundingRectangle;
        if (rectangle.IsEmpty)
        {
            return $"Element '{element.Current.Name}' has no screen bounds.";
        }

        var point = new Point(
            rectangle.Left + rectangle.Width / 2,
            rectangle.Top + rectangle.Height / 2);

        var hwnd = WindowFromPoint(new NativePoint((int)point.X, (int)point.Y));
        return DescribeWindow(hwnd);
    }

    private static string DescribeWindow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return "<none>";
        }

        var root = GetAncestor(hwnd, 2);
        return string.Join(
            ", ",
            $"hwnd=0x{hwnd.ToInt64():X}",
            $"class='{ReadClassName(hwnd)}'",
            $"title='{ReadTitle(hwnd)}'",
            $"root=0x{root.ToInt64():X}",
            $"rootClass='{ReadClassName(root)}'");
    }

    private static string ReadClassName(IntPtr hwnd)
    {
        var builder = new StringBuilder(256);
        _ = GetClassName(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string ReadTitle(IntPtr hwnd)
    {
        var builder = new StringBuilder(256);
        _ = GetWindowText(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(NativePoint point);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hwnd, StringBuilder className, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int maxCount);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativePoint
    {
        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly int X;
        public readonly int Y;
    }
}
