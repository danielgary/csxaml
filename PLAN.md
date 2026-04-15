# Milestone 13 Plan - Stabilization, Compatibility, DI, Lifecycle, and Test Hardening

## Status

- Drafted: 2026-04-15
- Roadmap target: Milestone 13 in [ROADMAP.md](./ROADMAP.md)
- Roadmap status: not started
- Document purpose: give an implementation-ready plan for making CSXAML dependable enough for real team use, with explicit DI, explicit lifecycle/disposal behavior, C#-first testing APIs, and documented compatibility boundaries

Milestone 12 made failures legible.

Milestone 13 has a different job: make the system feel boring to depend on.

At the end of this milestone, a developer should be able to answer all of these without guesswork:

- what is a stable source-level promise and what is still experimental?
- how do components receive services without blurring props, state, and ambient app dependencies?
- when does a component mount, unmount, and clean up?
- what happens if async work completes after a component is gone?
- how do I test a CSXAML component from ordinary C# without manually spelunking generated types and child indices?

Milestone 13 should make those answers explicit in code, tests, and docs.

---

## 1. Outcome

At the end of Milestone 13, CSXAML should provide a coherent stability story across five paths:

1. compatibility policy and supported feature matrix
2. explicit component-level DI through `inject`
3. explicit lifecycle, unmount, and cleanup behavior
4. hostless C#-first component testing APIs
5. broader regression coverage around the above contracts

Concrete user-visible outcomes:

- the language has a documented compatibility policy instead of an implied one
- the repo publishes a supported feature matrix that says what v1 actually promises
- components can declare required app services with `inject Type name;`
- services resolve once per component instance from an ambient `IServiceProvider`
- missing required services fail in source terms, not raw container jargon alone
- unmounted components stop participating in render invalidation
- cleanup and async-after-unmount behavior are explicit and testable
- component tests can render, query, and interact with logical trees from ordinary C#

Milestone 13 is not done if DI technically works but feels like a service locator, if cleanup still depends on tribal knowledge, or if component testing still requires direct knowledge of generated code or brittle child indices.

---

## 2. Day-To-Day Experience Bar

This is the real product bar for the milestone.

### 2.1 Service dependencies are visible

When a component needs app services, the source should say so up front:

```csharp
component Element TodoBoard {
    inject ITodoService todoService;
    inject ILogger<TodoBoard> logger;

    State<List<TodoItemModel>> items = new State<List<TodoItemModel>>(new());

    return <StackPanel>...</StackPanel>;
}
```

The developer should not have to infer service dependencies from:

- component parameter lists
- special markup attributes
- arbitrary `GetRequiredService()` calls inside helper code

### 2.2 Host integration stays simple

When an app does not use DI, the current root-instance path should still work.

When an app does use DI, the host should accept a normal `IServiceProvider` boundary without forcing the app into a CSXAML-owned container model.

The public story should be:

- simple demos/tests can still mount a root instance directly
- apps/tests that need DI can provide an `IServiceProvider`
- no one needs to rewrite their entire app boot path just to try CSXAML

### 2.3 Lifecycle and async behavior are explicit

When a component mounts, unmounts, or cleans up work, the behavior should be:

- explicit
- small
- easy to test
- free of hook-style call-order magic

When async work completes after unmount, the result should not resurrect the component or trigger confusing rerenders.

### 2.4 Component tests feel like ordinary C#

When writing tests, the developer should be able to:

- render a component with optional services
- query by text or automation metadata
- trigger common interactions like click, text input, and checked-state changes
- assert on meaningful UI semantics rather than tree positions

The developer should not need to:

- construct generated props types by hand unless they explicitly want to
- manually wire a full WinUI window for logical-tree tests
- invoke handlers through hand-rolled private test helpers scattered across the repo

### 2.5 Stability promises are written down

The repo should clearly document:

- syntax compatibility expectations
- runtime/generator compatibility expectations
- supported language/runtime/tooling/testing slices
- known limitations that are still outside the v1 promise

---

## 3. Scope

### In scope

- explicit DI syntax implementation for `inject Type name;`
- service-aware component activation built on `IServiceProvider`
- explicit root-host/service-provider plumbing
- small lifecycle and cleanup model
- async-after-unmount behavior definition and implementation
- hostless C#-first component testing harness
- semantic query helpers and interaction helpers
- expanded golden/regression coverage for DI, lifecycle, and testing
- compatibility policy documentation
- supported feature matrix documentation

### Explicitly out of scope

- hook-style DI APIs as the primary language model
- markup-based service injection
- attribute-scanned injection such as `[Inject]` as the primary `.csxaml` model
- keyed, named, or optional DI syntax
- subtree service-provider overrides in markup
- per-component child scopes
- a custom effect framework
- a broad new lifecycle DSL in `.csxaml`
- full debugger/test-recorder tooling
- performance optimization work beyond what is necessary to keep the new seams clean

### Practical boundary

Milestone 13 should stay centered on:

- stability
- explicitness
- narrow DI
- narrow lifecycle
- testability
- documented promises

It should not drift into:

- framework magic
- container feature parity wars
- a second language layered on top of C#
- a large testing DSL

---

## 4. Acceptance Scenarios To Freeze Before Coding

These scenarios should be written down first and then converted into tests as the milestone lands.

They are the milestone's contract surface, not optional examples.

### 4.1 DI authoring and resolution

- rendering a component with no `inject` declarations and no services behaves exactly as it does today
- rendering a component with `inject` declarations and a valid `IServiceProvider` resolves each declared service once per component instance
- injected services are available in helper code, event handlers, and ordinary expression islands
- a missing required service fails with a CSXAML-specific message that names the component and injected member
- injected services do not appear in generated props or markup usage surfaces

### 4.2 DI diagnostics

- malformed `inject` declarations fail with parser or validator diagnostics tied to `.csxaml` source
- duplicate injected names fail deterministically
- collisions between props/state/injected-service names fail deterministically
- invalid placement of `inject` declarations fails deterministically

### 4.3 Activation, retention, and disposal

- a simple root-instance host path still works without DI
- a service-aware root host path works with `IServiceProvider`
- child components are activated through the explicit activator rather than raw reflection calls
- retained keyed children preserve component identity and do not re-resolve injected services
- removed child components are disposed once
- disposing the host disposes the root component and any retained children exactly once

### 4.4 Async-after-unmount behavior

- state updates after unmount do not schedule rerender
- async completions after unmount do not resurrect removed components
- cleanup guidance for long-running work is documented and test-backed

### 4.5 Testing-harness behavior

- tests can render components with and without services
- tests can query by automation id, automation name, and visible text where represented in logical properties
- tests can drive click, text input, and checked-state interactions without bespoke per-test helpers
- tests can provide service overrides without changing component props

### 4.6 Interop and compatibility protection

- supported external-control interop still works after activation changes
- docs clearly distinguish supported, specified-but-not-implemented, and intentionally unsupported features

---

## 5. Current Baseline

The repo already has useful ingredients to build on:

- generated component props are explicit and strongly typed
- runtime rendering already flows through a logical tree plus a coordinator
- hostless logical-tree tests already exist in `Csxaml.Runtime.Tests`
- semantic automation metadata support already exists through attached properties
- runtime exception context from Milestone 12 already provides a good place to attach DI/lifecycle failures

The current gaps are equally clear:

- `CsxamlHost` only accepts a `Panel` plus a prebuilt root component instance
- `ComponentTreeCoordinator` only knows about a root instance, not an activation/service context
- `ChildComponentStore` creates child components with raw `Activator.CreateInstance`
- component instances have no explicit mount/unmount or disposal contract
- child component instances are retained/replaced, but their cleanup semantics are not explicit
- `State<T>` always invalidates through an action callback and has no concept of component mount state
- tests already prove hostless logical rendering, but the testing APIs are still repo-internal helper code rather than a deliberate user-facing harness
- compatibility policy and supported feature matrix are not yet published

Milestone 13 should close those gaps with small, explicit seams rather than a sweeping rewrite.

---

## 6. Core Design Decisions

These should be treated as milestone constraints.

### 6.1 Keep props, state, and services distinct

This is the most important semantic boundary in the milestone.

Rules:

- component parameters remain the public prop surface
- `State<T>` remains local mutable UI state
- `inject` declares ambient app services
- injected services never appear as markup attributes
- DI must not become a second ambient props channel

If this boundary gets blurry, the milestone is failing even if the code works.

### 6.2 Public DI boundary is `IServiceProvider`

CSXAML should work with the ordinary .NET `IServiceProvider` story.

That means:

- public host/test APIs should accept `IServiceProvider`
- CSXAML should not require a library-specific container type at the boundary
- `ActivatorUtilities` may be used internally, but it must not become the user-facing contract

This keeps the compatibility story honest: CSXAML works with any DI setup that can provide or adapt to `IServiceProvider`.

### 6.3 `inject` is the only DI language syntax in v1

The language spec now chooses:

```csharp
inject IFoo foo;
```

That means Milestone 13 should not spend time hedging among several syntaxes.

Do not add:

- `UseService<T>()` hook-style APIs as the primary model
- DI through component parameters
- markup-level service expressions
- attribute-scanned injection

### 6.4 Component activation must become an explicit seam

Today, child activation is a hidden `Activator.CreateInstance` call in `ChildComponentStore`.

Milestone 13 should replace that with a small explicit activator seam so:

- service-aware activation has one obvious home
- root and child activation follow the same model
- manual root-instance activation remains supported
- future testing and lifecycle logic have one place to attach

### 6.5 Generated components should stay boring

The language feature is `inject`, not constructor archaeology.

Preferred generated shape:

- keep generated components simple and predictable
- resolve injected services once per instance
- cache them in private fields or equivalent write-once instance members
- avoid resolving services during each render

The generator should target a small runtime hook in `ComponentInstance`, not force arbitrary semantic scanning of helper code.

### 6.6 Lifecycle should be smaller than an effect framework

Milestone 13 needs lifecycle semantics, but it does not need hooks, dependency arrays, or effect DSLs.

The smallest acceptable model is:

- a mount notification
- explicit cleanup/disposal on unmount
- clear behavior for async completions after unmount

Anything larger needs a very strong justification and likely belongs in a later milestone.

### 6.7 Unmounted invalidation should no-op

When async work completes after unmount, the old component instance should not schedule a rerender.

The preferred behavior is:

- state mutation may still happen on the old instance object
- render invalidation after unmount becomes a no-op
- cleanup/cancellation remains explicit user code, typically through disposal patterns

This is boring and unsurprising.

### 6.8 Hostless testing should sit over the logical tree

The testing harness should build on the logical tree/coordinator path first.

That means:

- no full WinUI activation requirement for ordinary component tests
- semantic queries and interactions operate on logical native nodes
- live WinUI projection tests remain useful, but they are a narrower layer

### 6.9 Documentation is part of the milestone, not cleanup

Compatibility policy, feature matrix, lifecycle semantics, and testing guidance are part of the implementation.

Do not leave them implicit.

---

## 7. Proposed Architecture

### 7.1 Generator-side `inject` model

Add a small explicit injected-service model under the generator.

Recommended responsibilities:

- `InjectFieldDefinition`
  one injected-service declaration in the AST
- parser support that recognizes `inject` as a component-prologue member
- validator support for:
  - duplicate injected names
  - invalid placement
  - obvious malformed declaration shapes
- emission support that:
  - generates cached service members
  - emits a small runtime hook to resolve services once per instance
  - preserves source spans for diagnostics and runtime context

Recommended file boundaries:

- `Csxaml.Generator/Ast/InjectFieldDefinition.cs`
- `Csxaml.Generator/Parsing/InjectFieldParser.cs`
- `Csxaml.Generator/Validation/InjectFieldValidator.cs`
- `Csxaml.Generator/Emission/InjectedServiceEmitter.cs`

Do not hide this behind giant parser or emitter methods.

### 7.2 Runtime activation seam

Introduce a dedicated component activation seam in the runtime.

Recommended types:

- `IComponentActivator`
  creates component instances given a component type and runtime context
- `DefaultComponentActivator`
  default implementation; may use `ActivatorUtilities` internally when services exist
- `ComponentContext`
  minimal per-instance runtime context; v1 MUST include `IServiceProvider Services`

Recommended responsibilities:

- `CsxamlHost`
  owns root mounting path and optionally receives services
- `ComponentTreeCoordinator`
  coordinates logical rendering and mount/unmount transitions
- `ChildComponentStore`
  uses `IComponentActivator` instead of raw `Activator.CreateInstance`

Important design constraint:

- keep the existing `CsxamlHost(Panel, ComponentInstance)` path for simple demos/tests
- add a service-aware path rather than replacing the simple path

### 7.3 Runtime hook for generated injection

The generator should target a small runtime hook in `ComponentInstance`.

Recommended shape:

- `ComponentInstance` gains an internal initialization hook that receives `ComponentContext`
- generated components override a literal service-resolution hook such as `ResolveInjectedServices(IServiceProvider services)` or equivalent
- the runtime ensures that hook runs once per component instance before the first render

Why this is preferable to constructor-only generation:

- it preserves a simple parameterless generated shape
- it avoids making `.csxaml` DI depend on constructor parsing or constructor signature conventions
- it keeps manual test components and existing root-instance paths easy to preserve

The activator may still use `ActivatorUtilities` for handwritten component constructors when a service provider exists, but the CSXAML language feature should not depend on constructor DI as its primary author-facing rule.

### 7.4 Lifecycle and cleanup model

Choose the smallest explicit lifecycle surface that solves the real problems.

Recommended v1 runtime contract:

- `ComponentInstance` gets a small mount notification hook such as `OnMounted()`
- component cleanup flows through ordinary `IDisposable` / `IAsyncDisposable`
- `ComponentTreeCoordinator` and `CsxamlHost` become disposable so they can release both native and component resources
- child components removed from the tree are disposed once they are no longer retained

Recommended explicit non-goals:

- no `OnUpdated`
- no dependency-array model
- no hook call-order semantics
- no hidden background task framework

### 7.5 Async-after-unmount behavior

Define this explicitly:

- once a component is unmounted, its invalidation callback becomes inert
- state changes on the dead instance must not schedule rerender
- cleanup/cancellation remains the component author's job through ordinary C# patterns
- docs should recommend cancellation-token-source ownership in components that launch long-running work

This should be documented and tested instead of left to emergent behavior.

### 7.6 Testing harness architecture

Add a deliberate testing layer over the logical tree.

Recommended project:

- `Csxaml.Testing`

Recommended primary APIs:

- `CsxamlTestHost.Render<TComponent>(...)`
- `CsxamlTestHost.Render(ComponentInstance root, ...)`
- render result object that exposes the latest logical root tree
- semantic query helpers:
  - `FindByAutomationId`
  - `FindByAutomationName`
  - `FindByText`
  - possibly `FindAllByTag` for secondary scenarios
- interaction helpers:
  - `Click`
  - `EnterText`
  - `SetChecked`

Recommended implementation approach:

- build on the logical native node tree and stored event handlers
- reuse the coordinator rather than reinventing rendering
- keep WinUI projection tests separate and optional

### 7.7 Compatibility and support docs

Milestone 13 should publish at least:

- `docs/compatibility-policy.md`
- `docs/supported-feature-matrix.md`
- `docs/lifecycle-and-async.md`
- `docs/component-testing.md`

The feature matrix should cover at least:

- core language constructs
- DI support
- lifecycle support
- built-in control/runtime support
- external-control support boundaries
- testing support
- tooling support status at a milestone level

### 7.8 Expected file and type boundaries

Milestone 13 should follow the repository's structural rules while adding new seams.

If an existing file starts to accumulate unrelated activation, lifecycle, DI, and testing logic, split it immediately instead of letting the milestone hide its own complexity.

Expected generator-side additions or extractions:

- `Csxaml.Generator/Ast/InjectFieldDefinition.cs`
- `Csxaml.Generator/Parsing/InjectFieldParser.cs` or a similarly narrow prologue-parsing file
- `Csxaml.Generator/Validation/InjectFieldValidator.cs`
- `Csxaml.Generator/Emission/InjectedServiceEmitter.cs`
- a small emitter extraction if generated initialization logic would otherwise bloat a larger component emitter file

Expected runtime-side additions or extractions:

- `Csxaml.Runtime/Components/ComponentContext.cs`
- `Csxaml.Runtime/Components/IComponentActivator.cs`
- `Csxaml.Runtime/Components/DefaultComponentActivator.cs`
- a small lifecycle/disposal state type if mount bookkeeping starts to grow

Expected testing-layer additions:

- `Csxaml.Testing/CsxamlTestHost.cs`
- `Csxaml.Testing/CsxamlRenderResult.cs`
- narrow query helper files under a `Queries` folder
- narrow interaction helper files under an `Interactions` folder

Structural guardrails:

- `CsxamlHost` should stay focused on host setup, root ownership, and disposal
- `ComponentTreeCoordinator` should stay focused on render coordination and lifecycle transitions, not become a test API surface
- `ChildComponentStore` should stay focused on child retention, activation, and release, not absorb service-provider policy or query logic
- the testing harness should consume the runtime seams, not punch through them with internal shortcuts

---

## 8. Phase Plan

Each phase should end with tests plus a brief review against the language spec, the roadmap exit criteria, and the developer experience bar.

### 8.1 Merge discipline

Do not land Milestone 13 as one large patch.

Prefer narrow vertical slices in this order:

1. acceptance scenarios, doc skeletons, and failing tests
2. runtime activation/context seam with no language change yet
3. `inject` parser, validator, emission, and runtime hookup
4. lifecycle/disposal/unmount safety
5. public testing harness extraction and migration of representative tests
6. docs, feature matrix, dogfood scenarios, and milestone closeout review

Each slice should keep the repo buildable, keep tests green outside the intentionally failing tests being introduced, and update adjacent docs when the implementation meaningfully changes.

### Phase 1 - Freeze contracts and acceptance cases

Goal:

- turn Milestone 13 into a concrete set of acceptance scenarios before new seams spread through the codebase

Tasks:

- define acceptance cases for:
  - required service resolution
  - missing required service failure
  - duplicate `inject` names
  - invalid `inject` placement
  - root host with and without services
  - child component DI activation
  - component disposal on unmount
  - async completion after unmount
  - hostless render/query/click/text/checked testing flows
- outline compatibility policy categories:
  - implemented and supported
  - specified but not yet implemented
  - intentionally unsupported
- outline supported feature matrix sections

Deliverables:

- acceptance test list
- doc skeletons for compatibility, lifecycle, testing, and feature matrix

### Phase 2 - Add runtime activation and context seams

Goal:

- create the runtime hooks that DI and lifecycle will rely on, without changing language surface yet

Tasks:

- introduce `ComponentContext`
- introduce `IComponentActivator` and default implementation
- thread activator/context through `CsxamlHost`, `ComponentTreeCoordinator`, and `ChildComponentStore`
- preserve the current root-instance constructor path
- add service-aware host overloads without breaking existing call sites
- keep all existing tests passing before `inject` lands

Deliverables:

- explicit component activation seam
- optional `IServiceProvider` host path
- no raw child activation left in `ChildComponentStore`

### Phase 3 - Implement `inject` end to end

Goal:

- land explicit component-level DI in the language and runtime

Tasks:

- add `InjectFieldDefinition` AST support
- update component-body parsing for prologue members
- add validation for duplicate names and malformed declarations
- add specific placement diagnostics for `inject`
- emit cached service members plus a runtime resolution hook
- attach source spans/member names so missing-service runtime failures point back to `.csxaml`
- ensure injected services never appear in component props or markup completion/validation paths

Deliverables:

- working `inject Type name;`
- source diagnostics for common authoring mistakes
- runtime missing-service failures with CSXAML context

### Phase 4 - Land lifecycle, disposal, and async safety

Goal:

- make mount/unmount and cleanup explicit and boring

Tasks:

- add the chosen mount notification API
- define unmount/disposal sequence for:
  - removed child components
  - root host disposal
  - retained versus replaced component instances
- dispose components that implement `IDisposable` or `IAsyncDisposable`
- make invalidation inert after unmount
- document and test async-after-unmount behavior

Deliverables:

- small lifecycle model
- explicit cleanup semantics
- async safety behavior that is easy to explain

### Phase 5 - Add the C#-first testing harness

Goal:

- turn the repo's internal logical-tree testing style into a real supported testing API

Tasks:

- create `Csxaml.Testing`
- add render-result object over the logical tree/coordinator
- add semantic query helpers
- add common interaction helpers
- add service override support for tests
- migrate a representative set of existing runtime tests to the new harness

Deliverables:

- public or package-ready test harness
- proof that common UI tests do not require WinUI activation

### Phase 6 - Publish stability docs and feature matrix

Goal:

- make the stability story explicit enough for outside users

Tasks:

- write compatibility policy
- write supported feature matrix
- write lifecycle/async guidance
- write component testing guide
- document DI boundaries and non-goals clearly

Deliverables:

- docs that match actual implementation
- explicit known-limitations section rather than implied caveats

### Phase 7 - Dogfood and close honestly

Goal:

- prove the milestone in real repo scenarios

Tasks:

- add at least one representative DI-backed component scenario
- prove missing-service failure messaging in a realistic component
- prove hostless component tests with services and interactions
- review root-host disposal and async-after-unmount cases for leaks or surprising rerender behavior
- compare docs, roadmap, and implementation line by line before marking the milestone complete

Deliverables:

- milestone review notes
- roadmap update only when the exit criteria are honestly met

---

## 9. Testing Strategy

Milestone 13 should be heavily test-driven because stability work is easy to overestimate from happy-path demos.

### 9.1 Generator tests

Add focused tests for:

- parsing valid `inject` declarations
- rejecting malformed `inject` declarations
- duplicate injected-name diagnostics
- invalid-placement diagnostics
- emission of cached service members and service-resolution hook
- keeping injected services out of generated props records

### 9.2 Runtime DI tests

Add focused tests for:

- root component activation with services
- child component activation with services
- missing required service failures with component/member context
- service resolution once per component instance
- keyed child reuse preserving injected-service identity on retained instances

### 9.3 Lifecycle and async tests

Add focused tests for:

- `OnMounted()` firing once per mounted instance
- disposal on child removal
- disposal on host/coordinator disposal
- invalidation after unmount becoming inert
- async completion after unmount not triggering rerender

### 9.4 Testing harness tests

Add focused tests for:

- render with and without services
- query by automation name/id
- query by visible text when represented in logical properties
- click, text input, and checked-state helpers
- test-time service override scenarios

### 9.5 End-to-end dogfood tests

Add repo-level proofs for:

- DI-backed components in ordinary runtime tests
- external-control scenarios still behaving under the new activator seams
- demo-style semantic queries using the testing harness rather than bespoke helpers

---

## 10. Risks and Traps

Milestone 13 has a few easy ways to go wrong.

### 10.1 Service locator creep

If helper code starts normalizing `GetRequiredService()` calls everywhere, the language feature exists but the design loses.

Keep `inject` as the preferred source model.

### 10.2 Constructor-shape coupling

If `.csxaml` DI starts depending on constructor signature rules, the generator/runtime contract becomes harder to reason about and existing manual components become easier to break.

Keep the language model centered on explicit `inject` declarations plus a small runtime initialization hook.

### 10.3 Lifecycle sprawl

If Milestone 13 starts inventing effect APIs, dependency arrays, or update hooks, it will sprawl badly.

Keep the lifecycle API small.

### 10.4 Disposal blind spots

If child components are replaced without disposal or root disposal does not flow down, the milestone will create subtle leaks while claiming to improve reliability.

Test disposal paths explicitly.

### 10.5 Testing API overfitting

If the testing harness becomes a pile of repo-specific helpers, it will not be a real v1 capability.

Prefer generic semantic queries and a tiny set of common interactions.

### 10.6 Documentation lag

If compatibility policy and feature matrix lag behind the code, the milestone is not done.

Docs are part of the implementation surface here.

---

## 11. Required Review Loop

At the end of each phase, ask:

1. Are props, state, and services still visibly distinct?
2. Does the public DI boundary still rest on `IServiceProvider` rather than a container-specific API?
3. Did we keep generated component code boring and deterministic?
4. Is lifecycle smaller than a custom effect framework?
5. Does async-after-unmount behavior now have one explicit, documented answer?
6. Would a new user know how to write a component test without reading internal runtime tests?
7. Are compatibility promises explicit enough to be quoted back by another engineer?
8. Did any file or type become a god object while adding activation, lifecycle, or testing seams?
9. Did docs and roadmap stay aligned with implementation?

If any answer is "no", fix the structure before continuing.

---

## 12. Concrete Implementation Checklist

- [ ] freeze Milestone 13 acceptance scenarios before code changes
- [ ] draft compatibility policy categories and supported feature matrix shape
- [ ] add explicit runtime activation seam over child and root component creation
- [ ] plumb optional `IServiceProvider` through `CsxamlHost`, `ComponentTreeCoordinator`, and child activation
- [ ] preserve the simple root-instance host path for non-DI scenarios
- [ ] add AST/parser support for `inject Type name;`
- [ ] validate duplicate and misplaced `inject` declarations
- [ ] emit cached injected-service members and a once-per-instance resolution hook
- [ ] keep injected services out of generated props and markup usage
- [ ] add missing-service runtime failures with component/member context
- [ ] define mount notification API
- [ ] define component disposal behavior on unmount and host shutdown
- [ ] make post-unmount invalidation inert
- [ ] document async-after-unmount behavior
- [ ] add `Csxaml.Testing` hostless render harness
- [ ] add semantic query helpers
- [ ] add click/text/checked interaction helpers
- [ ] support service overrides in tests
- [ ] expand golden generator coverage for injected-service emission
- [ ] expand runtime regression coverage for DI, lifecycle, cleanup, and async safety
- [ ] add interop regressions to prove external-control behavior survives the new seams
- [ ] publish compatibility policy
- [ ] publish supported feature matrix
- [ ] publish lifecycle/async guide
- [ ] publish component testing guide
- [ ] update `ROADMAP.md` only when the real exit criteria are met

---

## 13. Definition of Done

Milestone 13 is complete only when all of the following are true:

- the repo publishes explicit compatibility and support docs
- `inject Type name;` works end to end and remains clearly separate from props and markup
- runtime service resolution uses an `IServiceProvider` boundary and resolves once per component instance
- missing required services fail with component/member context rather than raw container errors alone
- mount, unmount, cleanup, and async-after-unmount behavior are explicit and tested
- hostless C#-first component tests exist as a real harness rather than ad hoc internal helpers
- representative DI, lifecycle, testing, and interop scenarios are covered by regression tests
- docs, roadmap, language spec, and implementation all tell the same story

If a developer still has to guess how service injection, cleanup, or component testing are supposed to work, Milestone 13 is not done.
