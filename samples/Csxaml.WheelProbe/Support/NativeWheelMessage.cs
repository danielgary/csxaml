namespace Csxaml.Samples.WheelProbe;

internal readonly record struct NativeWheelMessage(
    IntPtr Hwnd,
    int Message,
    IntPtr WParam,
    IntPtr LParam)
{
    public string Name => Message switch
    {
        0x020A => "WM_MOUSEWHEEL",
        0x020E => "WM_MOUSEHWHEEL",
        0x024E => "WM_POINTERWHEEL",
        0x024F => "WM_POINTERHWHEEL",
        _ => $"0x{Message:X4}"
    };

    public int ScreenX => unchecked((short)((long)LParam & 0xFFFF));

    public int ScreenY => unchecked((short)(((long)LParam >> 16) & 0xFFFF));

    public int WheelDelta => unchecked((short)(((long)WParam >> 16) & 0xFFFF));
}
