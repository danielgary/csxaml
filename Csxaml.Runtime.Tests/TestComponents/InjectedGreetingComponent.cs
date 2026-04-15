namespace Csxaml.Runtime.Tests;

internal sealed class InjectedGreetingComponent : ComponentInstance
{
    private static readonly CsxamlSourceInfo ComponentSourceInfo = new(
        "InjectedGreeting.csxaml",
        1,
        1,
        3,
        2,
        "InjectedGreeting");

    private static readonly CsxamlSourceInfo GreetingSourceInfo = new(
        "InjectedGreeting.csxaml",
        2,
        5,
        2,
        35,
        "InjectedGreeting",
        null,
        "greeting");

    private GreetingService? _greeting;

    public int ResolveCount { get; private set; }

    public override string CsxamlComponentName => "InjectedGreeting";

    public override CsxamlSourceInfo? CsxamlSourceInfo => ComponentSourceInfo;

    protected override void ResolveInjectedServices(IServiceProvider services)
    {
        ResolveCount++;
        _greeting = InjectedServiceResolver.ResolveRequired<GreetingService>(
            this,
            services,
            "greeting",
            GreetingSourceInfo);
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", $"Greeting:{_greeting!.Text}:{ResolveCount}")],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
