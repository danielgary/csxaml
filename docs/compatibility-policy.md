# Compatibility Policy

## Scope

This document defines the compatibility promise for Milestone 13.

It covers:

- `.csxaml` source authoring
- generated-code expectations
- runtime and testing package alignment
- explicitly unsupported or still-evolving areas

## Source compatibility

The following source forms are part of the Milestone 13 compatibility promise:

- component parameters as the public prop surface
- `State<T>` declarations in the component prologue
- explicit service injection through `inject Type name;`
- ordinary markup attributes, events, and attached properties already covered by the language spec
- conditionals, loops, slots, and component composition already covered by shipped tests

Breaking source changes to these forms should be treated as compatibility breaks and require:

- a roadmap note
- updated docs
- migration guidance when practical

## Runtime and generator alignment

Supported combinations are intentionally narrow:

- `Csxaml.Generator` and `Csxaml.Runtime` from the same repo revision or release train
- `Csxaml.Testing` aligned with the same `Csxaml.Runtime`

Cross-version mixing is not a supported scenario for Milestone 13.

In practical terms:

- generated output may depend on runtime hooks added in the same milestone
- the testing package may depend on runtime activation and logical-tree shapes from the same milestone
- consumers should upgrade generator, runtime, and testing together

## DI compatibility boundary

The public DI boundary is `IServiceProvider`.

That means:

- CSXAML works with any DI setup that can provide or adapt to `IServiceProvider`
- CSXAML does not require a container-specific API at the host or testing boundary
- `ActivatorUtilities` is an implementation detail of the default activator, not the public contract

The Milestone 13 DI contract includes:

- `inject Type name;`
- once-per-instance required service resolution
- missing-service failures with component/member context

The Milestone 13 DI contract does not include:

- keyed or named service syntax
- optional service syntax
- subtree provider overrides
- per-component service scopes
- property injection
- props from DI

## Lifecycle compatibility boundary

The Milestone 13 lifecycle contract is intentionally small:

- components mount after a successful render/commit
- `OnMounted()` runs once per mounted instance
- removed components dispose once
- coordinator and host disposal tear down retained component subtrees
- post-unmount invalidation does not rerender the tree

Milestone 13 does not promise:

- `OnUpdated`
- effect hooks
- dependency arrays
- framework-managed async cancellation

## Testing compatibility boundary

`Csxaml.Testing` is the supported hostless logical-tree test API for Milestone 13.

The stable testing surface includes:

- render by root instance or root type
- optional service-provider or service-override setup
- semantic queries by automation id, automation name, and text
- interaction helpers for click, text input, and checked-state changes

The testing package does not promise:

- WinUI projection testing without WinUI availability
- pixel or visual diff tooling
- snapshot format stability across unrelated runtime tree changes

## Tooling status

Milestone 13 stabilizes language/runtime/testing contracts first.

Tooling surfaces such as Visual Studio integration and richer design-time experiences may continue to evolve more quickly and should be treated according to their own milestone notes.

## Change discipline

A change should be treated as a compatibility break if it:

- changes valid `.csxaml` source meaning
- changes required generator/runtime alignment rules
- changes the `inject` contract in a user-visible way
- changes lifecycle/disposal behavior in a user-visible way
- changes the public `Csxaml.Testing` API in a user-visible way

When a compatibility break is necessary, the repo should update:

- `LANGUAGE-SPEC.md`
- `ROADMAP.md`
- the relevant docs in `docs/`

## Known limitations

Milestone 13 deliberately keeps some areas narrow:

- no DI hooks syntax such as `UseService<T>()`
- no subtree DI scope manipulation
- no broad lifecycle DSL
- no dedicated lifecycle authoring syntax in `.csxaml` source yet
- no guarantee that helper-code-local variables are valid inside state initializer expressions
- no promise that cross-version generator/runtime pairs will work
