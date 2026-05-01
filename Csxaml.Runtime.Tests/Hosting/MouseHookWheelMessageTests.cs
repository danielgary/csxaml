using System.Runtime.InteropServices;

namespace Csxaml.Runtime.Tests.Hosting;

[TestClass]
public sealed class MouseHookWheelMessageTests
{
    [TestMethod]
    public void FromNative_ReadsWheelDataFromMouseHookStructEx()
    {
        var data = new MouseHookData
        {
            Point = new NativePoint { X = 320, Y = 240 },
            MouseData = 0x00780000
        };

        var message = ReadMessage(data);

        Assert.AreEqual(120, message.Delta);
        Assert.AreEqual(320, message.ScreenX);
        Assert.AreEqual(240, message.ScreenY);
    }

    [TestMethod]
    public void FromNative_ReadsNegativeWheelDataFromMouseHookStructEx()
    {
        var data = new MouseHookData
        {
            Point = new NativePoint { X = -12, Y = -30 },
            MouseData = 0xFF880000
        };

        var message = ReadMessage(data);

        Assert.AreEqual(-120, message.Delta);
        Assert.AreEqual(-12, message.ScreenX);
        Assert.AreEqual(-30, message.ScreenY);
    }

    private static MouseHookWheelMessage ReadMessage(MouseHookData data)
    {
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<MouseHookData>());
        try
        {
            Marshal.StructureToPtr(data, pointer, fDeleteOld: false);
            return MouseHookWheelMessage.FromNative(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseHookData
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
