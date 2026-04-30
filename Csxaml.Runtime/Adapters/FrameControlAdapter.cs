using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Csxaml.Runtime;

internal sealed class FrameControlAdapter : ControlAdapter<Frame>
{
    public override string TagName => "Frame";

    protected override void ApplyEvents(
        Frame control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        CommonElementEventBinder.Apply(control, node, bindingStore);
        BindNavigated(control, node, bindingStore);
        BindNavigating(control, node, bindingStore);
        BindNavigationFailed(control, node, bindingStore);
        BindNavigationStopped(control, node, bindingStore);
    }

    protected override void ApplyProperties(Frame control, NativeElementNode node)
    {
    }

    protected override void SetChildren(Frame control, IReadOnlyList<UIElement> children)
    {
        if (children.Count > 0)
        {
            throw new InvalidOperationException("Frame does not support child elements.");
        }
    }

    private static void BindNavigated(
        Frame control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<NavigationEventArgs>(
            node,
            bindingStore,
            "OnNavigated",
            handler =>
            {
                NavigatedEventHandler typedHandler = (_, args) => handler(args);
                control.Navigated += typedHandler;
                return () => control.Navigated -= typedHandler;
            });
    }

    private static void BindNavigating(
        Frame control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<NavigatingCancelEventArgs>(
            node,
            bindingStore,
            "OnNavigating",
            handler =>
            {
                NavigatingCancelEventHandler typedHandler = (_, args) => handler(args);
                control.Navigating += typedHandler;
                return () => control.Navigating -= typedHandler;
            });
    }

    private static void BindNavigationFailed(
        Frame control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<NavigationFailedEventArgs>(
            node,
            bindingStore,
            "OnNavigationFailed",
            handler =>
            {
                NavigationFailedEventHandler typedHandler = (_, args) => handler(args);
                control.NavigationFailed += typedHandler;
                return () => control.NavigationFailed -= typedHandler;
            });
    }

    private static void BindNavigationStopped(
        Frame control,
        NativeElementNode node,
        NativeEventBindingStore bindingStore)
    {
        NativeEventArgsBinder.Rebind<NavigationEventArgs>(
            node,
            bindingStore,
            "OnNavigationStopped",
            handler =>
            {
                NavigationStoppedEventHandler typedHandler = (_, args) => handler(args);
                control.NavigationStopped += typedHandler;
                return () => control.NavigationStopped -= typedHandler;
            });
    }
}
