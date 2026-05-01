using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Csxaml.Samples.WheelProbe;

internal sealed class NativeWheelWindowSubclass : IDisposable
{
    private const int WindowProcedureIndex = -4;

    private readonly IntPtr hwnd;
    private readonly Action<NativeWheelMessage> messageReceived;
    private readonly WndProc replacement;
    private readonly IntPtr previous;
    private bool disposed;

    public NativeWheelWindowSubclass(IntPtr hwnd, Action<NativeWheelMessage> messageReceived)
    {
        this.hwnd = hwnd;
        this.messageReceived = messageReceived;
        replacement = HandleMessage;
        previous = SetWindowLongPtr(hwnd, WindowProcedureIndex, Marshal.GetFunctionPointerForDelegate(replacement));
        if (previous == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        SetWindowLongPtr(hwnd, WindowProcedureIndex, previous);
    }

    private IntPtr HandleMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam)
    {
        messageReceived(new NativeWheelMessage(hwnd, message, wParam, lParam));
        return CallWindowProc(previous, hwnd, message, wParam, lParam);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtr64(hwnd, index, value);
        }

        return new IntPtr(SetWindowLong32(hwnd, index, value.ToInt32()));
    }

    private delegate IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(
        IntPtr previous,
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hwnd, int index, int value);
}
