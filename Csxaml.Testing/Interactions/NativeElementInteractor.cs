namespace Csxaml.Testing;

internal static class NativeElementInteractor
{
    public static void Click(NativeElementNode node)
    {
        GetEventHandler<Action>(node, "OnClick")();
    }

    public static void EnterText(NativeElementNode node, string text)
    {
        GetEventHandler<Action<string>>(node, "OnTextChanged")(text);
    }

    public static void SetChecked(NativeElementNode node, bool value)
    {
        GetEventHandler<Action<bool>>(node, "OnCheckedChanged")(value);
    }

    private static TDelegate GetEventHandler<TDelegate>(NativeElementNode node, string name)
        where TDelegate : Delegate
    {
        var handler = node.Events.SingleOrDefault(eventValue => eventValue.Name == name)?.Handler;
        if (handler is TDelegate typedHandler)
        {
            return typedHandler;
        }

        throw new InvalidOperationException(
            $"Node '{node.TagName}' does not expose a '{name}' handler compatible with '{typeof(TDelegate).Name}'.");
    }
}
