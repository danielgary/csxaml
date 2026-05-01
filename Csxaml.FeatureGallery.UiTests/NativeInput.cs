using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;

namespace Csxaml.FeatureGallery.UiTests;

internal static class NativeInput
{
    private const uint InputMouse = 0;
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;
    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private const uint MouseEventWheel = 0x0800;
    private const ushort VirtualKeyA = 0x41;

    public static void PressA(IntPtr windowHandle)
    {
        BringWindowForward(windowHandle);

        var inputs = new[]
        {
            Keyboard(VirtualKeyA, keyUp: false),
            Keyboard(VirtualKeyA, keyUp: true)
        };

        Send(inputs);
    }

    public static void LeftClickCenter(IntPtr windowHandle, AutomationElement element)
    {
        BringWindowForward(windowHandle);
        MoveCursorToCenter(element);

        var inputs = new[]
        {
            Mouse(MouseEventLeftDown),
            Mouse(MouseEventLeftUp)
        };

        Send(inputs);
    }

    public static void WheelDownOver(IntPtr windowHandle, AutomationElement element)
    {
        BringWindowForward(windowHandle);
        MoveCursorToCenter(element);

        var inputs = new[]
        {
            MouseWheel(-120)
        };

        Send(inputs);
    }

    public static bool TryLeftClickCenter(
        IntPtr windowHandle,
        AutomationElement element,
        out string result)
    {
        BringWindowForward(windowHandle);
        if (!TryMoveCursorToCenter(element, out result))
        {
            return false;
        }

        var inputs = new[]
        {
            Mouse(MouseEventLeftDown),
            Mouse(MouseEventLeftUp)
        };

        Send(inputs);
        result = "click sent";
        return true;
    }

    public static bool TryWheelDownOver(
        IntPtr windowHandle,
        AutomationElement element,
        out string result)
    {
        BringWindowForward(windowHandle);
        if (!TryMoveCursorToCenter(element, out result))
        {
            return false;
        }

        var inputs = new[]
        {
            MouseWheel(-120)
        };

        Send(inputs);
        result = "wheel sent";
        return true;
    }

    public static string DescribeElementAtCenter(AutomationElement element)
    {
        var rectangle = element.Current.BoundingRectangle;
        if (rectangle.IsEmpty)
        {
            return $"Element '{element.Current.Name}' has no screen bounds.";
        }

        var point = new Point(
            rectangle.Left + rectangle.Width / 2,
            rectangle.Top + rectangle.Height / 2);

        return AutomationElement.FromPoint(point)?.Describe() ?? "No automation element at center.";
    }

    private static void BringWindowForward(IntPtr windowHandle)
    {
        SetForegroundWindow(windowHandle);
        Thread.Sleep(150);
    }

    private static void MoveCursorToCenter(AutomationElement element)
    {
        if (!TryMoveCursorToCenter(element, out var result))
        {
            Assert.Fail(result);
        }
    }

    private static bool TryMoveCursorToCenter(AutomationElement element, out string result)
    {
        var rectangle = element.Current.BoundingRectangle;
        if (rectangle.IsEmpty)
        {
            result = $"Element '{element.Current.Name}' has no screen bounds.";
            return false;
        }

        var x = (int)(rectangle.Left + rectangle.Width / 2);
        var y = (int)(rectangle.Top + rectangle.Height / 2);

        if (!SetCursorPos(x, y))
        {
            result = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            return false;
        }

        Thread.Sleep(150);
        result = "cursor moved";
        return true;
    }

    private static Input Keyboard(ushort virtualKey, bool keyUp)
    {
        return new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = keyUp ? KeyEventKeyUp : 0
                }
            }
        };
    }

    private static Input MouseWheel(int delta)
    {
        return new Input
        {
            Type = InputMouse,
            Data = new InputUnion
            {
                Mouse = new MouseInput
                {
                    MouseData = delta,
                    Flags = MouseEventWheel
                }
            }
        };
    }

    private static Input Mouse(uint flags)
    {
        return new Input
        {
            Type = InputMouse,
            Data = new InputUnion
            {
                Mouse = new MouseInput
                {
                    Flags = flags
                }
            }
        };
    }

    private static void Send(Input[] inputs)
    {
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (sent != inputs.Length)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        Thread.Sleep(250);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public int MouseData;
        public uint Flags;
        public uint Time;
        public nint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nint ExtraInfo;
    }
}
