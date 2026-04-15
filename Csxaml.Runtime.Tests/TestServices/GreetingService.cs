namespace Csxaml.Runtime.Tests;

internal sealed class GreetingService
{
    public GreetingService(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
