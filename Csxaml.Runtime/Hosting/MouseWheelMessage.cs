namespace Csxaml.Runtime;

internal readonly record struct MouseWheelMessage(int Delta, int ScreenX, int ScreenY)
{
    public static MouseWheelMessage FromNative(UIntPtr wParam, IntPtr lParam)
    {
        var wParamValue = (ulong)wParam;
        var lParamValue = lParam.ToInt64();
        var delta = unchecked((short)((long)(wParamValue >> 16) & 0xffff));
        var screenX = unchecked((short)(lParamValue & 0xffff));
        var screenY = unchecked((short)((lParamValue >> 16) & 0xffff));
        return new MouseWheelMessage(delta, screenX, screenY);
    }
}
