using Microsoft.UI.Dispatching;

namespace Csxaml.Samples.FeatureGallery;

internal static class NativeWheelMessageProbe
{
    private static NativeWheelMessageProbeSession? session;
    private static Action<string>? report;

    public static string Status => session?.Status ?? "Native wheel messages: waiting";

    public static void Attach(DispatcherQueue? dispatcher, Action<string> update)
    {
        report = update;
        if (dispatcher?.TryEnqueue(AttachNow) == true)
        {
            return;
        }

        AttachNow();
    }

    public static void Reset()
    {
        session?.Reset();
        Publish();
    }

    private static void AttachNow()
    {
        var root = NativeWheelWindowFinder.FindMainWindow();
        var child = root == IntPtr.Zero
            ? IntPtr.Zero
            : NativeWheelWindowFinder.FindChildByClass(root, "Microsoft.UI.Content.DesktopChildSiteBridge");

        if (root == IntPtr.Zero || child == IntPtr.Zero)
        {
            report?.Invoke("Native wheel messages: HWND not found");
            return;
        }

        if (session?.Matches(root, child) != true)
        {
            session?.Dispose();
            session = new NativeWheelMessageProbeSession(root, child, Publish);
        }

        Publish();
    }

    private static void Publish()
    {
        report?.Invoke(Status);
    }
}
