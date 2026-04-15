# Lifecycle and Async Behavior

## Design goal

Milestone 13 keeps lifecycle small on purpose.

CSXAML should answer the important questions clearly:

- when does a component become mounted?
- when is cleanup guaranteed?
- what happens if old async work finishes after the component is gone?

It should not introduce an effect framework just to answer them.

## Initialization order

For a component instance created through the runtime activation path:

1. the instance is created
2. injected services are resolved once
3. the first render runs
4. after a successful render/commit, the instance becomes mounted
5. `OnMounted()` runs once for that instance

Generated components resolve `inject` services before their first render.

## Mount behavior

`OnMounted()` is the only built-in lifecycle hook added in Milestone 13.

Use it for:

- one-time subscriptions
- one-time startup work
- kicking off async work owned by the component

Do not expect:

- update hooks
- dependency tracking
- multiple mount notifications for retained instances

Retained keyed components keep their identity and do not mount again on ordinary rerenders.

## Unmount and disposal behavior

A component leaves the active tree when:

- its parent stops rendering it
- it loses keyed retention and is replaced
- the root coordinator or host is disposed

When that happens:

- the component stops participating in render invalidation
- the component subtree is disposed once
- `IDisposable` and `IAsyncDisposable` cleanup are honored

Coordinator and host disposal tear down retained child component subtrees as well as the root component.

## Sync and async cleanup

Milestone 13 supports both:

- `IDisposable`
- `IAsyncDisposable`

Notes:

- async root disposal is available through `DisposeAsync()`
- component removal during a normal synchronous render still happens on a synchronous path; async disposal is awaited/blocking there so cleanup is not skipped

## Async-after-unmount behavior

This is the most important rule to remember:

**state updates on an unmounted component do not rerender the tree**

The stale instance may still receive a late async completion and mutate its old fields or `State<T>` objects, but its render invalidation callback is inert after unmount/disposal.

That means:

- old work does not resurrect removed components
- stale completions do not trigger surprise rerenders
- component authors still own cancellation when work should stop early

## Recommended async pattern

For long-running component-owned work, prefer normal C# cancellation/disposal patterns.

Example for a handwritten runtime component:

```csharp
internal sealed class WeatherCardComponent : ComponentInstance
{
    private readonly IWeatherService _weatherService;
    private readonly State<string> _summary;

    CancellationTokenSource? _loadCts;

    public WeatherCardComponent(IWeatherService weatherService)
    {
        _weatherService = weatherService;
        _summary = new State<string>("Loading...", () => RequestRender?.Invoke());
    }

    protected override void OnMounted()
    {
        _loadCts = new CancellationTokenSource();
        _ = LoadAsync(_loadCts.Token);
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        _summary.Value = await _weatherService.GetSummaryAsync(cancellationToken);
    }

    public void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", _summary.Value)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
```

The framework guarantees the late completion will not rerender a removed component.

The component remains responsible for cancelling work that should stop early.

## Non-goals

Milestone 13 does not add:

- `OnUpdated`
- effect hooks
- dependency arrays
- automatic cancellation token injection
- per-component DI scopes
- dedicated lifecycle authoring syntax inside `.csxaml` source
