# Supported Feature Matrix

Status legend:

- `Supported`: covered by Milestone 13 behavior and regression tests
- `Experimental`: available, but not yet part of the stable Milestone 13 promise
- `Not supported`: intentionally outside the Milestone 13 contract

| Area | Status | Notes |
| --- | --- | --- |
| Component parameters | Supported | Public prop surface only. |
| `State<T>` declarations | Supported | Component-local mutable UI state. |
| `inject Type name;` | Supported | Required typed services resolved once per component instance. |
| Conditional markup (`if`) | Supported | Covered by generator/runtime regression tests. |
| Repeated markup (`foreach`) | Supported | Includes keyed child retention behavior. |
| Slots | Supported | Covered by parser/validation/runtime tests. |
| Attached properties | Supported | Includes semantic automation metadata. |
| Built-in control property/event binding | Supported | Covered by generator/runtime tests. |
| External controls from referenced assemblies | Supported | Covered by validation, emission, runtime, and demo interop tests. |
| Solution-local external controls | Supported | Covered by external-control validation and runtime regression paths. |
| Root instance host path | Supported | `CsxamlHost(panel, instance)` and hostless coordinator/test flows remain valid. |
| Root type activation with services | Supported | `CsxamlHost(panel, typeof(Component), services)` and hostless test rendering support this path. |
| `IServiceProvider` activation boundary | Supported | Public DI boundary for runtime/testing. |
| `ActivatorUtilities` constructor activation | Supported | Used by the default activator when creating component instances. |
| `OnMounted()` | Supported | Runtime hook for handwritten `ComponentInstance` types; `.csxaml` source does not yet have dedicated lifecycle syntax. |
| `IDisposable` cleanup | Supported | Removed components and disposed roots are cleaned up once; handwritten component types can implement cleanup directly. |
| `IAsyncDisposable` cleanup | Supported | Async root disposal is supported; sync removal paths block on async disposal when needed. |
| Post-unmount state invalidation no-op | Supported | Stale component instances do not rerender the tree after removal/disposal. |
| `Csxaml.Testing` root-instance render | Supported | Hostless logical-tree rendering. |
| `Csxaml.Testing` root-type render | Supported | Supports DI activation. |
| `Csxaml.Testing` service overrides | Supported | `Action<ServiceCollection>` overloads are available. |
| `Csxaml.Testing` query by automation id/name | Supported | Preferred semantic query surface. |
| `Csxaml.Testing` query by text/content | Supported | Works over logical tree properties. |
| `Csxaml.Testing` click/text/checked interactions | Supported | Designed for common component workflows. |
| Generator/runtime cross-version mixing | Not supported | Upgrade generator, runtime, and testing together. |
| Keyed/named/optional DI syntax | Not supported | Outside Milestone 13. |
| Property injection or markup injection | Not supported | Use explicit `inject` declarations instead. |
| Per-component DI scopes | Not supported | Host services are authoritative. |
| Subtree service-provider overrides | Not supported | Outside Milestone 13. |
| `OnUpdated` / effect hooks / dependency arrays | Not supported | Lifecycle remains intentionally small. |
| Full visual/UI automation testing framework | Experimental | WinUI projection tests exist, but hostless logical-tree testing is the supported Milestone 13 story. |
