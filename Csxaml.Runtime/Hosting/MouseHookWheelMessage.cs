using System.Runtime.InteropServices;

namespace Csxaml.Runtime;

internal readonly record struct MouseHookWheelMessage(int Delta, int ScreenX, int ScreenY)
{
    public static MouseHookWheelMessage FromNative(IntPtr lParam)
    {
        var data = Marshal.PtrToStructure<MouseHookStructEx>(lParam);
        var delta = unchecked((short)((data.MouseData >> 16) & 0xffff));
        return new MouseHookWheelMessage(delta, data.Point.X, data.Point.Y);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseHookStructEx
    {
        public NativePoint Point;
        public IntPtr WindowHandle;
        public uint HitTestCode;
        public UIntPtr ExtraInfo;
        public uint MouseData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }
}
