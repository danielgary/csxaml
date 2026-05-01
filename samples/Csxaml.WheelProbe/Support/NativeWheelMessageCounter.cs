namespace Csxaml.Samples.WheelProbe;

internal sealed class NativeWheelMessageCounter(string name)
{
    private const int MouseWheel = 0x020A;
    private const int MouseHWheel = 0x020E;
    private const int PointerWheel = 0x024E;
    private const int PointerHWheel = 0x024F;

    private int mouseHWheelCount;
    private int mouseWheelCount;
    private int pointerHWheelCount;
    private int pointerWheelCount;

    public NativeWheelMessage? LastMessage { get; private set; }

    public bool TryRecord(NativeWheelMessage message)
    {
        switch (message.Message)
        {
            case MouseWheel:
                mouseWheelCount++;
                LastMessage = message;
                return true;
            case MouseHWheel:
                mouseHWheelCount++;
                LastMessage = message;
                return true;
            case PointerWheel:
                pointerWheelCount++;
                LastMessage = message;
                return true;
            case PointerHWheel:
                pointerHWheelCount++;
                LastMessage = message;
                return true;
            default:
                return false;
        }
    }

    public void Reset()
    {
        mouseWheelCount = 0;
        mouseHWheelCount = 0;
        pointerWheelCount = 0;
        pointerHWheelCount = 0;
        LastMessage = null;
    }

    public override string ToString()
    {
        return $"{name} mouse={mouseWheelCount}, hmouse={mouseHWheelCount}, pointer={pointerWheelCount}, hpointer={pointerHWheelCount}";
    }
}
