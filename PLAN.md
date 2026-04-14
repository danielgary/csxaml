# Milestone 10 Plan - Project System Maturity and Fast Inner Loop

## Status

- Drafted: 2026-04-13
- Roadmap target: Milestone 10 in [ROADMAP.md](./ROADMAP.md)
- Roadmap status: not started
- Document purpose: give a future agent an implementation-ready, review-ready plan for landing deterministic project integration, cross-project component consumption, and test-project friendliness without turning CSXAML into hidden build magic

Milestone 10 is the point where CSXAML stops being "the demo builds if you know the ritual" and starts proving it can live inside a normal solution.

The wrong version of this milestone:

- keeps copy-pasted custom targets in every project
- keeps the hardcoded `GeneratedCsxaml` fallback namespace alive
- treats test projects as special manual-generation exceptions
- discovers referenced components through heuristics and naming accidents
- rewrites `obj` aggressively on every build
- quietly smuggles generator internals into runtime-facing APIs

The right version is much more disciplined:

- one boring shared build path
- one boring default-namespace rule
- one explicit referenced-component metadata contract
- one explicit import-resolution story across local components, referenced components, and external controls
- unchanged builds do not churn outputs
- test projects behave like ordinary consumers
- future tooling and packaging remain possible because the build and metadata boundaries stay explicit

This plan assumes Milestone 10 succeeds only if a skeptical maintainer can explain:

- how a `.csxaml` file gets from source to `obj`
- how a downstream project learns about referenced components
- how namespace fallback works when no file-scoped namespace is present
- why a rebuild did or did not rerun generation
- why a renamed or deleted component does not leave stale generated files behind

without hand-waving.

---

## 1. Outcome

At the end of Milestone 10, CSXAML should support this project experience cleanly:

- a project opts into CSXAML generation once through shared build logic rather than per-project handwritten targets
- a `.csxaml` file with an explicit file-scoped `namespace` generates into that namespace
- a `.csxaml` file without an explicit namespace generates into a deterministic project-default namespace instead of the hardcoded placeholder `GeneratedCsxaml`
- generated project-internal support files such as manifests and external-control registration live in a deterministic internal namespace that does not leak into author-facing examples
- a component library project can expose generated components to downstream projects through normal assembly references
- a downstream `.csxaml` file can import referenced component namespaces through normal `using` directives, and may use alias-qualified tags when that keeps name resolution explicit
- test projects can consume generated component types through ordinary `ProjectReference` usage without duplicating generator targets for the referenced source files
- unchanged inputs do not cause full regeneration or unnecessary file rewrites
- deleted or renamed `.csxaml` files do not leave stale `.g.cs` files in `obj`
- project-reference and package-reference changes rerun generation predictably when they affect referenced controls or referenced CSXAML components

Milestone 10 is done only when the repo proves all of the following:

- the build integration is shared rather than duplicated
- generated namespace behavior is deterministic and documented
- at least one referenced component library compiles and is consumable from another project
- at least one test project consumes generated components through normal references rather than duplicate generation wiring
- clean, rebuild, and rename scenarios are covered by proof tests or build fixtures
- the roadmap and language spec still describe the system honestly

---

## 2. Scope

### In scope

- centralize CSXAML MSBuild integration into shared repo-level build assets
- define the v1 default generated-namespace convention
- define a deterministic internal generated namespace for project-level helper output
- replace destructive always-regenerate behavior with deterministic write-if-changed plus stale-output pruning
- add a referenced-component metadata contract that downstream generator runs can consume from assemblies
- support cross-project component discovery and validation
- support cross-project component imports through the same normal `using` and `using Alias = Namespace;` model already used for external controls
- keep resolution deterministic across:
  - local components
  - referenced components
  - built-in controls
  - referenced external controls
- prove ordinary test-project consumption through at least one real fixture
- add integration coverage for clean, rebuild, rename, and reference-change scenarios
- define the public package/API boundary decisions that this milestone must not accidentally freeze in the wrong shape
- update docs, plan truth, and roadmap truth as the design locks in

### Explicitly out of scope

- switching the whole system to a Roslyn source generator
- Visual Studio completion, editor diagnostics, formatting UI, or navigation work from Milestone 11
- source mapping or generated-code debugging improvements from Milestone 12
- lifecycle, async, disposal, or testing-harness semantics from Milestone 13
- packaging and release mechanics from Milestone 15
- a full custom SDK or NuGet-delivered build package
- automatic folder-to-namespace mapping
- implicit scanning of referenced project source trees
- dynamic runtime discovery of components without an explicit generated manifest contract
- broad rethinks of runtime reconciliation or language surface unrelated to project-system maturity

### Practical boundary

Milestone 10 should stay centered on:

- repo-level MSBuild assets such as `Directory.Build.props`, `Directory.Build.targets`, or a `build/` folder
- `Csxaml.Generator/Cli`
- `Csxaml.Generator/IO`
- `Csxaml.Generator/Semantics`
- `Csxaml.Generator/Validation`
- `Csxaml.Generator/Emission`
- the small public metadata contract assembly used by generator and generated code
- fixture projects and tests that prove multi-project behavior
- `Csxaml.Demo` only where namespace/default-namespace migration is required
- `LANGUAGE-SPEC.md`
- `ROADMAP.md`

Do not let Milestone 10 silently become:

- a tooling milestone
- a source-generator rewrite
- a packaging milestone
- a lifecycle/test-harness milestone
- a general "clean up the whole solution" milestone

---

## 3. Existing Architecture Baseline

### 3.1 Build integration is duplicated and bespoke

Today the repo has project-local generation targets rather than one shared CSXAML build path.

Concrete examples:

- [Csxaml.Demo.csproj](./Csxaml.Demo/Csxaml.Demo.csproj) defines a custom `GenerateCsxaml` target
- [Csxaml.Runtime.Tests.csproj](./Csxaml.Runtime.Tests/Csxaml.Runtime.Tests.csproj) defines a separate `GenerateDemoCsxaml` target

Both targets:

- collect references manually
- write a references file manually
- invoke `dotnet run --project ..\\Csxaml.Generator`
- include generated `.g.cs` files manually

This works for the current repo, but it is not yet a project system. It is two hand-maintained call sites.

### 3.2 Generation is currently full and destructive

The current project targets call:

- `RemoveDir` on the generated directory
- `MakeDir`
- generator invocation
- wildcard compile include

That means:

- every generation run destroys the previous output set
- unchanged outputs are rewritten
- rename/delete scenarios are handled by brute-force directory replacement rather than explicit stale-output management
- the build does not yet have a meaningful "fast inner loop"

This is acceptable for the prototype. It is not acceptable as the steady-state milestone-10 behavior.

### 3.3 The generator still lacks project-level namespace context

The current emitter falls back to:

- `GeneratedCsxaml` when a file does not declare an explicit namespace

Relevant file:

- [ComponentEmitter.cs](./Csxaml.Generator/Emission/ComponentEmitter.cs)

That hardcoded fallback was fine while everything lived in one project and the demo support types opted into the same placeholder namespace. It becomes actively harmful once:

- multiple component libraries exist
- test projects reference component libraries
- downstream projects need deterministic namespace discovery

### 3.4 Project-level helper output still assumes the old fallback namespace

The current generated external-control registration file is emitted into:

- `namespace GeneratedCsxaml;`

Relevant file:

- [GeneratedExternalControlRegistrationEmitter.cs](./Csxaml.Generator/Emission/GeneratedExternalControlRegistrationEmitter.cs)

That is a hidden project-system hazard because component files with explicit namespaces and project-level helper output with hardcoded fallback namespaces are on a collision course.

Milestone 10 needs to solve this explicitly rather than hoping all projects stay namespace-less.

### 3.5 Demo support code still leans on the placeholder namespace

The demo currently keeps support types in `GeneratedCsxaml`, for example:

- [TodoItemModel.cs](./Csxaml.Demo/Models/TodoItemModel.cs)
- [TodoColors.cs](./Csxaml.Demo/Support/TodoColors.cs)
- [TodoStyles.cs](./Csxaml.Demo/Support/TodoStyles.cs)

That is useful evidence:

- the fallback namespace is not just a generator detail anymore
- changing the fallback will have visible downstream consequences

Milestone 10 therefore needs an explicit migration step for demo and fixture code.

### 3.6 Component discovery is still local and simple-name based

The current component catalog:

- indexes only components from the current generator invocation
- keys them by simple component name
- has no concept of namespace-qualified component identity
- has no concept of referenced component assemblies

Relevant files:

- [ComponentCatalog.cs](./Csxaml.Generator/Validation/ComponentCatalog.cs)
- [ComponentCatalogBuilder.cs](./Csxaml.Generator/Validation/ComponentCatalogBuilder.cs)

That is intentionally narrow for earlier milestones, but it is not enough for cross-project resolution.

### 3.7 Import resolution is already more advanced for external controls than for components

The current tag resolver already understands:

- file-level `using Namespace;`
- file-level `using Alias = Namespace;`
- alias-qualified external control tags
- ambiguity diagnostics for imported external control names

Relevant files:

- [ImportScope.cs](./Csxaml.Generator/Semantics/ImportScope.cs)
- [MarkupTagResolver.cs](./Csxaml.Generator/Semantics/MarkupTagResolver.cs)

That is a good foundation, but the current resolver still only treats components as:

- local simple names

Milestone 10 should extend that existing import model rather than inventing a second one for components.

### 3.8 There is no referenced-component metadata contract yet

Downstream generation currently has no explicit way to ask a referenced assembly:

- which CSXAML components it exports
- what namespaces they live in
- what props they accept
- whether they support default child content
- what generated component type and props type should be emitted

That contract must become explicit in Milestone 10.

Without it, downstream resolution would be forced into one of several bad options:

- source scanning
- reflection over generated naming conventions
- hand-authored registration files

### 3.9 Test-project proof still relies on duplicate generation

`Csxaml.Runtime.Tests` currently regenerates the demo components from source with its own target rather than consuming them through a normal project reference.

That has been useful so far because it let runtime tests stay focused on runtime behavior, but it is still a milestone-10 gap relative to the roadmap promise:

- "test projects can consume generated components predictably without hand-authored duplicate generation wiring"

Milestone 10 needs at least one honest proof of the normal-reference path.

### 3.10 Clean/rebuild/rename and design-time behavior are not yet first-class proofs

The roadmap explicitly calls out:

- incremental generation
- deterministic `obj` behavior
- clean/rebuild and rename scenarios
- design-time/build-time stability

Today those are not yet backed by dedicated fixture-level proof.

Milestone 10 should not claim maturity until those scenarios are tested directly.

---

## 4. Decisions To Lock In Up Front

These decisions should guide the milestone unless a failing proof test forces a rethink.

### 4.1 Keep the current generator model for this milestone

Milestone 10 should improve the existing CLI-driven generation path rather than replacing it with a source generator or custom MSBuild task.

Why:

- the current generator already expresses the language and runtime contract clearly
- the current risk is project-system maturity, not generator capability
- jumping to a different generation technology would multiply unknowns and make milestone truth harder to judge

This milestone should leave the codebase with a cleaner seam for future generator-host changes, not use milestone pressure as an excuse to rewrite the host.

### 4.2 Introduce one shared build entry point

Recommended direction:

- repo-level `build/Csxaml.props`
- repo-level `build/Csxaml.targets`
- imported once for participating projects through root `Directory.Build.props` and `Directory.Build.targets`, or through explicit imports if that stays clearer

Projects should opt in with one obvious property, such as:

- `<EnableCsxaml>true</EnableCsxaml>`

Do not keep copy-paste generation targets in every project.

### 4.3 Define a boring default namespace rule now

Recommended default namespace behavior:

- if the file declares an explicit file-scoped namespace, use it
- otherwise use `$(RootNamespace)` if present
- otherwise use `$(AssemblyName)`

Do not add folder mirroring in v1.

This satisfies the language-spec requirement that the fallback be:

- deterministic
- discoverable

and it avoids the uncanny valley where authors have to guess whether folders silently rewrite namespaces.

### 4.4 Separate author-facing component namespaces from internal generated infrastructure

Milestone 10 should introduce a deterministic internal infrastructure namespace, separate from the author-facing component namespace.

Recommended direction:

- author-facing component namespace:
  - explicit file namespace, or
  - project default namespace fallback
- internal generated namespace:
  - `<ProjectDefaultNamespace>.__CsxamlGenerated`

Project-level helper output such as:

- component manifest providers
- external-control registration
- other future internal support files

should live there.

This keeps:

- user-facing examples clean
- internal generated support predictable
- cross-file explicit namespaces from colliding with project-level helper output

### 4.5 Referenced component discovery must use an explicit manifest contract

Do not infer referenced components from:

- class-name suffixes
- props-record naming conventions
- ad hoc reflection over all public types

Recommended direction:

- generate a project-level manifest provider into the built assembly
- expose it through a small public metadata contract
- let downstream generator runs load that manifest explicitly from referenced assemblies

That contract should include at least:

- component name
- component namespace
- assembly identity
- generated component CLR type name
- generated props CLR type name, if any
- parameter names and parameter type names in order
- whether default child content is supported

### 4.6 Cross-project component imports should use the same normal `using` and alias model as other imports

Milestone 10 should support both:

- `using MyCompany.Widgets;` plus bare `<TodoCard />` when unambiguous
- `using Widgets = MyCompany.Widgets;` plus `<Widgets:TodoCard />` when explicit qualification is desired

This keeps the mental model aligned with the direction already set for:

- external controls
- attached-property owner lookup
- file-local helper code and file-scoped namespaces

If milestone implementation adds alias-qualified component tags, update the language spec in the same change so the project does not drift into undocumented behavior.

### 4.7 Resolution rules must stay deterministic and boring

Recommended simple-tag resolution rule:

1. built-in native controls by exact simple name
2. local components from the current compilation by exact simple name
3. imported referenced components and imported external controls from visible namespaces
4. clear diagnostics for ambiguity or unsupported imported matches

Recommended alias-qualified resolution rule:

1. resolve alias to exactly one namespace
2. search referenced components and external controls visible in that namespace
3. if one supported match exists, use it
4. if multiple supported matches exist, fail with a diagnostic
5. if only unsupported matches exist, fail with the explicit unsupported reason

Do not add heuristic fallback searches.

### 4.8 Test projects should behave like ordinary consumers

Milestone 10 should prove that at least one test project can:

- reference a component project or component library
- compile against its generated components
- instantiate or render those generated components

without:

- a duplicate `GenerateCsxaml` target aimed at the referenced source files
- hand-maintained generated-file includes
- hand-authored mirrored support code

### 4.9 Incremental means both target skipping and write stability

This milestone should not call something "incremental" unless both are true:

- MSBuild can skip generator invocation when inputs and reference outputs are unchanged
- when generation does run, unchanged generated files keep their content and timestamps

Deleting and rewriting every generated file on every input change is still churn, even if the target itself sometimes skips.

### 4.10 Generated app code must not depend on generator internals

Generated project code may depend on:

- `Csxaml.Runtime`
- the chosen public metadata contract assembly

Generated project code must not depend on:

- `Csxaml.Generator` internals
- test-only helpers
- repo-local build helper types that do not ship as normal references

This is part of the package/API boundary discipline Milestone 10 is supposed to lock down.

---

## 5. Tempting Wrong Approaches

### Wrong approach: switch to a Roslyn source generator now

That would multiply the problem space:

- new hosting model
- new incremental model
- new design-time story
- new debugging surface

before the current project contract is even explicit.

Milestone 10 should make the current generator host predictable first.

### Wrong approach: keep copy-paste targets and just "clean them up a little"

If `Csxaml.Demo`, fixture projects, and test projects all keep their own near-duplicate targets, the repo will drift immediately.

The right fix is one shared integration path.

### Wrong approach: preserve `GeneratedCsxaml` as the default forever

That namespace is:

- not project specific
- not discoverable from normal .NET conventions
- likely to collide across libraries
- hostile to cross-project imports

Milestone 10 should retire it as the fallback convention, not bless it.

### Wrong approach: infer referenced components from naming conventions

For example:

- find public types ending in `Component`
- guess the tag name by trimming the suffix
- guess the props type by appending `Props`

That is brittle, opaque, and hard to version intentionally.

The manifest contract must be explicit.

### Wrong approach: scan referenced source trees or `obj` folders

That would make build behavior dependent on:

- source layout
- intermediate output layout
- project-to-project file visibility quirks

The downstream contract should be assembly-based and deterministic.

### Wrong approach: treat test projects as a permanent special case

If milestone proof still requires a test project to regenerate someone else's `.csxaml` files manually, the roadmap promise has not actually landed.

### Wrong approach: keep destructive `RemoveDir` generation

That hides stale-file logic instead of solving it and guarantees needless churn.

### Wrong approach: make folder structure rewrite namespaces automatically

Folder mirroring sounds convenient until someone asks:

- what happens on rename
- what happens on partial namespace declarations
- how tooling displays the effective namespace

Milestone 10 should favor explicit file namespaces plus a simple project-default fallback.

### Wrong approach: freeze package boundaries accidentally

Do not let the easiest local implementation quietly decide forever:

- which assembly owns component manifest types
- whether generated projects reference generator-only assemblies
- whether internal build helpers become runtime surface area

Those boundaries need an intentional decision while the blast radius is still manageable.

---

## 6. Recommended V1 Project Model

### 6.1 Project opt-in should be obvious

Recommended project shape:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <EnableCsxaml>true</EnableCsxaml>
    <RootNamespace>MyCompany.Widgets</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <CsxamlSource Include="**\*.csxaml" Exclude="bin\**;obj\**" />
  </ItemGroup>
</Project>
```

The project should not need to know:

- where the generator output folder lives
- how references are collected
- how manifests are emitted
- how stale files are pruned

Those are shared-build concerns.

### 6.2 Namespace behavior should be unsurprising

Examples:

```csharp
// No explicit namespace in UserAvatar.csxaml
component Element UserAvatar(string UserId) {
    return <TextBlock Text={UserId} />;
}
```

If `RootNamespace` is `MyCompany.Widgets`, the generated component lives in:

- `MyCompany.Widgets`

If the file declares:

```csharp
namespace MyCompany.Widgets.Admin;

component Element AuditBadge(string Label) {
    return <TextBlock Text={Label} />;
}
```

the generated component lives in:

- `MyCompany.Widgets.Admin`

No hidden folder convention should intervene.

### 6.3 Internal generated support should be hidden but deterministic

For the same project, internal support files should live in:

- `MyCompany.Widgets.__CsxamlGenerated`

For example:

- `MyCompany.Widgets.__CsxamlGenerated.GeneratedComponentManifest`
- `MyCompany.Widgets.__CsxamlGenerated.GeneratedExternalControlRegistration`

Generated component code may reference those types with fully-qualified names.

User-authored code should not need to import or know them.

### 6.4 Referenced component consumption should feel like ordinary C#

Consumer example:

```csharp
using MyCompany.Widgets;
using Admin = MyCompany.Widgets.Admin;

component Element Dashboard(string UserId) {
    return <StackPanel>
        <UserAvatar UserId={UserId} />
        <Admin:AuditBadge Label="Verified" />
    </StackPanel>;
}
```

That is the right mental model:

- bring namespaces into scope with `using`
- use bare names when unambiguous
- use aliases when explicit qualification makes the code clearer

### 6.5 Test projects should stop pretending they are special

Recommended proof scenario:

- `Csxaml.ProjectSystem.TestComponents` class library contains `.csxaml` components
- `Csxaml.ProjectSystem.Tests` references that project normally
- tests instantiate or render generated component types directly

No duplicate generator target in the test project.

That is the kind of proof Milestone 10 needs.

---

## 7. Required Architecture Changes

### 7.1 Centralize build logic into shared repo assets

Recommended files:

- `build/Csxaml.props`
- `build/Csxaml.targets`
- optional root `Directory.Build.props`
- optional root `Directory.Build.targets`

The shared build contract should define:

- project opt-in property such as `EnableCsxaml`
- default generated directory, preferably under `$(IntermediateOutputPath)Csxaml`
- generated source manifest path
- generated reference manifest path
- generated stamp or output manifest path
- project default namespace
- internal generated namespace
- generator invocation target
- cleanup target integration with `CoreClean`

Keep the targets:

- short
- literal
- inspectable

Do not bury the actual control flow behind imported mystery targets with opaque names.

### 7.2 Extend generator options to carry project context explicitly

The generator currently only receives:

- output directory
- references-file path
- input files

Milestone 10 should add explicit project context, at minimum:

- default component namespace
- internal generated namespace
- optionally project or assembly name if that simplifies diagnostics or manifest emission

Recommended additions:

- extend `GeneratorOptions`
- extend `GeneratorOptionsParser`
- thread the new context through `GeneratorRunner`, `CompilationContext`, and emitters

Do not keep project-default namespace behavior as a hardcoded emitter fallback.

### 7.3 Make output writing deterministic and non-destructive

`OutputWriter` should become responsible for:

- writing only when content actually changed
- preserving unchanged file timestamps
- deleting stale generated files within the designated generated directory
- refusing to delete outside the designated generated directory

Recommended direction:

- generator computes the exact current output file set
- output writer writes changed files
- output writer removes stale `.g.cs` files under the generated directory that are no longer part of the current set
- output writer also writes a stable generated-file manifest if that helps `CoreClean` and fixture assertions

This is the right place to solve stale-output behavior because the generator already knows the intended file set.

### 7.4 Introduce a small public component-manifest contract

Recommended direction:

- place a data-only manifest contract in `Csxaml.ControlMetadata` unless that assembly becomes semantically muddy enough to justify spinning out a tiny shared contract assembly first

The contract should be boring:

- `ComponentMetadata`
- `ComponentParameterMetadata`
- `CompiledComponentManifest`
- optional assembly attribute or provider interface for discovery

The contract should not contain:

- parser logic
- runtime rendering logic
- build-target logic

Its only job is to let generated component libraries describe their exported component surface to downstream generator runs.

### 7.5 Emit a manifest provider into each generating component project

Milestone 10 should generate a project-level manifest provider file that contains the local component surface.

Each entry should include at least:

- tag name
- namespace name
- assembly identity
- fully qualified generated component type name
- fully qualified generated props type name, or null when the component has no props
- ordered parameter metadata
- default-slot support flag

The provider should live in the internal generated namespace and be discoverable without naming heuristics.

### 7.6 Add a referenced-component catalog builder

The generator needs a new semantic step that:

- inspects referenced assemblies
- locates the explicit component manifest provider
- reads referenced component metadata into a downstream catalog

Keep this loader narrow:

- load only assemblies already supplied through the build reference list
- read only the known manifest contract
- cache loaded manifests per assembly path during one generator run
- surface explicit unsupported or malformed-manifest diagnostics when needed

Do not turn this into broad reflection over every public type in every assembly.

### 7.7 Rewrite component catalogs around explicit component entries

The current catalog stores `ParsedComponent` by simple name.

Milestone 10 should introduce an explicit component entry model that can represent both:

- local components from the current project
- referenced components from manifest metadata

Each entry should answer:

- simple name
- namespace name
- assembly identity
- full identity
- component CLR type name
- props CLR type name
- ordered parameter metadata
- default-slot support
- whether the component is local or referenced

Local-only data such as source spans can remain attached separately for diagnostics.

### 7.8 Extend tag resolution to handle referenced components cleanly

`MarkupTagResolver` should evolve so that it can:

- resolve local components
- resolve imported referenced components
- resolve alias-qualified referenced components
- keep built-in native control behavior stable
- keep imported external control behavior stable
- produce good ambiguity diagnostics when names collide
- diagnose duplicate referenced component identities clearly when namespace plus simple name collide across assemblies

Important guardrail:

Do not split tag resolution into separate hidden code paths that duplicate import rules for components and native controls. The import model should stay one coherent system.

### 7.9 Emit fully qualified component type usage

`ChildNodeEmitter` currently assumes:

- local simple component tag name
- local props record type name
- local component class type name

Milestone 10 should switch component emission to use explicit resolved metadata, not local naming assumptions.

That means the resolver result for components should provide enough information to emit:

- `typeof(global::MyCompany.Widgets.UserAvatarComponent)`
- `new global::MyCompany.Widgets.UserAvatarProps(...)`

or their zero-props equivalent.

This change will also make local-component emission more explicit and robust.

### 7.10 Move project-level helper output off the author-facing namespace assumption

Current external-control registration output should be updated so that:

- it lives in the internal generated namespace
- generated component code references it explicitly by fully qualified name

This avoids hidden namespace coupling and keeps future project-level helper output in one deterministic place.

### 7.11 Add real multi-project fixture coverage

Milestone 10 needs honest proof fixtures, not just unit tests over generator internals.

Recommended fixture shape:

- `Csxaml.ProjectSystem.Components`
  - class library
  - contains a few `.csxaml` components
  - includes both explicit-namespace and fallback-namespace files
- `Csxaml.ProjectSystem.Consumer`
  - class library or small app
  - consumes the components through `using` imports and alias-qualified tags
- `Csxaml.ProjectSystem.Tests`
  - MSTest project
  - references the component library normally
  - instantiates generated components or runs hostless runtime proofs

In addition, integration tests or fixture scripts should run:

- build
- rebuild
- clean
- rename/delete scenarios

### 7.12 Reduce duplicate generation in existing tests where it matters

Milestone 10 does not need to refactor every current test immediately, but it should leave the repo with at least one honest normal-reference proof and a clear path away from bespoke duplication.

Reasonable milestone-10 target:

- keep specialized demo-regeneration tests only where they are still buying focused runtime coverage
- add at least one normal-reference test project as the canonical project-system proof
- stop adding new duplicate-regeneration patterns once the shared build path exists

### 7.13 Lock package and API boundaries intentionally

Before the milestone is called complete, the plan should explicitly record:

- which assembly owns the public component manifest contract
- which assemblies generated app code is allowed to reference
- where shared build assets live in the repo
- which pieces are still repo-local implementation details rather than public release surface

This does not require Milestone 15 packaging work, but it does require intentional boundaries.

---

## 8. Execution Plan

### Phase 1 - Lock the milestone with real proof fixtures and failing tests

#### Goal

Define the success surface before refactoring the build.

#### Tasks

- add a small component-library fixture project dedicated to project-system proof
- add a consumer fixture project that references that library
- add a test-project proof that references the library normally
- add failing tests or scripted assertions for:
  - fallback namespace uses project default namespace
  - explicit file namespace overrides fallback
  - referenced component resolution through `using Namespace;`
  - alias-qualified referenced component resolution through `using Alias = Namespace;`
  - stale generated file cleanup after rename/delete
  - unchanged rebuild does not rewrite outputs
  - referenced-assembly changes rerun downstream generation
  - test project consumes generated components without duplicate generation target

#### Guardrail

Do not change the build system first and then "add proof later." The fixture surface is what keeps this milestone honest.

### Phase 2 - Centralize the shared build contract and project context

#### Goal

Replace duplicated project-local generation targets with one shared integration path and explicit generator inputs.

#### Tasks

- add shared build props/targets files
- define the opt-in property for projects that contain `.csxaml`
- define standard generated-directory and manifest-file paths
- extend generator CLI options with:
  - default component namespace
  - internal generated namespace
- update generating projects to use the shared target rather than handwritten inline targets
- keep the current invocation model readable; avoid stacking shell indirection on top of MSBuild indirection

#### Guardrail

After this phase, there should be one obvious place to read the CSXAML build contract.

### Phase 3 - Land deterministic output writing, stale-output pruning, and clean integration

#### Goal

Make `obj` behavior boring and fast enough for real iteration.

#### Tasks

- teach `OutputWriter` to write only changed files
- add stale-file pruning limited to the designated generated directory
- emit a stable generated-file manifest or equivalent output list if needed for clean and fixture assertions
- stop deleting the whole generated directory on every generation run
- register generated outputs with `FileWrites` or equivalent clean integration
- add clean/rebuild/rename fixture assertions

#### Guardrail

Do not call the milestone "incremental" if generation still rewrites every output file whenever anything changes.

### Phase 4 - Emit and load referenced component manifests

#### Goal

Introduce the explicit contract that lets downstream projects understand referenced CSXAML components.

#### Tasks

- add the public component-manifest contract types
- generate a project-level manifest provider into each producing project
- add a referenced-component manifest loader over referenced assemblies
- keep load behavior deterministic and local to the provided reference list
- add validation and failure diagnostics for malformed or missing manifest expectations where appropriate
- add unit tests for manifest generation and loading

#### Guardrail

If implementation starts guessing component shape from naming conventions instead of reading the manifest contract, stop and correct it immediately.

### Phase 5 - Rewrite component catalogs, resolution, and emission for cross-project use

#### Goal

Make the generator understand referenced components as first-class symbols.

#### Tasks

- replace simple-name-only component catalog entries with explicit component metadata entries
- extend `MarkupTagResolver` to resolve:
  - local components
  - imported referenced components
  - alias-qualified referenced components
  - existing imported external controls
- preserve deterministic ambiguity diagnostics
- update validators to use component metadata rather than local AST assumptions where needed
- update `ChildNodeEmitter` to emit fully qualified component and props type names
- update project-level helper emission such as external-control registration to use the internal generated namespace

#### Guardrail

Keep resolution logic in one understandable place. Do not fork "component import logic" and "control import logic" into parallel half-systems.

### Phase 6 - Prove ordinary test-project consumption

#### Goal

Close the roadmap gap where test projects still need bespoke generation wiring.

#### Tasks

- add or update one test project so it references a component library normally
- use the referenced generated component types in at least one real test
- remove duplicate generation wiring from that proof path
- document which existing duplicate-generation tests remain temporary and why

#### Guardrail

Do not declare success just because build fixtures compile. A real test project must consume generated components through a normal reference path.

### Phase 7 - Harden reference invalidation and design-time-adjacent behavior

#### Goal

Make the shared target predictable when references and build modes change.

#### Tasks

- ensure reference-list changes trigger generation
- ensure project-reference rebuilds propagate via referenced assembly timestamps or equivalent inputs
- ensure package-version or referenced-control-library changes invalidate appropriately
- run at least one design-time-style or `DesignTimeBuild=true` smoke build to confirm the target does not behave catastrophically
- keep design-time support narrow and honest; the milestone only needs stability, not full IDE richness

#### Guardrail

Do not over-promise full IDE behavior. The standard for Milestone 10 is "stable enough for normal development," not "tooling is done."

### Phase 8 - Migrate demo/support namespaces and update milestone truth

#### Goal

Align the example code and docs with the new namespace reality.

#### Tasks

- update demo support types away from the placeholder `GeneratedCsxaml` assumption
- update any generated-component consumers such as app bootstrap code to use the new namespace model
- update `LANGUAGE-SPEC.md` if alias-qualified referenced component tags or other import semantics were extended
- update `ROADMAP.md` milestone status and notes only after the proof matrix is actually satisfied
- add notes about any intentionally deferred project-system limits

#### Guardrail

Do not leave the demo and docs teaching the old placeholder namespace after the real default convention changes.

---

## 9. Verification Matrix

### Shared build integration

- one shared repo-level CSXAML target path exists
- demo and any fixture projects use the shared path rather than copy-pasted targets
- projects without `EnableCsxaml` are unaffected

### Namespace behavior

- file with explicit `namespace` emits there
- file without explicit namespace emits into project default namespace
- internal helper output emits into the internal generated namespace
- demo/support code compiles cleanly under the new convention

### Referenced component discovery

- producing project emits a manifest provider
- downstream generator loads the referenced manifest
- manifest data includes props and slot support
- manifest data includes enough identity to diagnose same-name exports from different assemblies
- malformed or unsupported manifest conditions fail clearly

### Resolution behavior

- local simple component tags still work
- imported referenced component tags work through `using Namespace;`
- alias-qualified referenced component tags work through `using Alias = Namespace;`
- imported external controls still work
- ambiguity across referenced components yields a diagnostic
- duplicate same-namespace same-name exports across different assemblies yield a diagnostic
- unsupported imported matches yield explicit diagnostics

### Emission behavior

- emitted component references use fully qualified type names
- zero-props components and props-bearing components both work
- child content/default slot support survives cross-project use

### Incremental and cleanup behavior

- unchanged rebuild skips generation or at minimum does not rewrite outputs
- changed `.csxaml` file updates only affected generated outputs
- renamed `.csxaml` file removes the old generated file
- deleted `.csxaml` file removes the old generated file
- `dotnet clean` removes generated outputs predictably

### Reference invalidation behavior

- referenced component-library rebuild invalidates downstream generation
- referenced external-control library change invalidates downstream generation
- changed project default namespace invalidates generation

### Test-project proof

- a test project references a component library normally
- the test project uses generated component types without duplicate generation wiring
- proof remains understandable to a new maintainer

### Milestone truth check

- roadmap checklist items for Milestone 10 are satisfied by actual proof, not just code existence
- language spec still matches the implemented import/namespace model

---

## 10. Concrete Checklist

- [ ] add shared repo-level CSXAML build props/targets
- [ ] define a single project opt-in property such as `EnableCsxaml`
- [ ] move generating projects onto the shared build path
- [ ] define `CsxamlGeneratedDirectory` under `$(IntermediateOutputPath)`
- [ ] define stable source/reference/stamp manifest file paths
- [ ] extend generator CLI options with default component namespace
- [ ] extend generator CLI options with internal generated namespace
- [ ] replace the hardcoded `GeneratedCsxaml` fallback in the emitter
- [ ] define the internal generated namespace convention
- [ ] emit project-level helper output into the internal generated namespace
- [ ] make generated component code reference helper output by fully qualified name
- [ ] add write-if-changed output behavior
- [ ] add stale generated-file pruning
- [ ] integrate generated outputs with clean behavior
- [ ] add a public component-manifest contract
- [ ] generate a project-level component manifest provider
- [ ] load referenced component manifests from referenced assemblies
- [ ] introduce explicit component catalog entries for local and referenced components
- [ ] resolve imported referenced components through `using Namespace;`
- [ ] resolve alias-qualified referenced components through `using Alias = Namespace;`
- [ ] preserve existing external-control import behavior
- [ ] preserve existing local component behavior
- [ ] emit fully qualified referenced component and props type names
- [ ] validate child-content support from component metadata, not local AST assumptions alone
- [ ] add a component-library fixture project
- [ ] add a consumer fixture project
- [ ] add a normal-reference test-project proof
- [ ] add build/clean/rebuild/rename integration assertions
- [ ] add reference-invalidation integration assertions
- [ ] migrate demo/support code off the placeholder namespace assumption
- [ ] update spec wording if alias-qualified component tags become part of the implemented model
- [ ] update roadmap truth only after proof scenarios pass

---

## 11. Prove-Yourself-Wrong Loop

Use this loop after every phase and before calling the milestone complete.

1. Did the build contract actually get simpler?

- If the answer is "not really" because multiple projects still carry custom targets, keep going.

2. Did we accidentally preserve `GeneratedCsxaml` in a way that still matters?

- If demo code, support code, or generated helper code still depends on it as the real default story, the milestone is not done.

3. Are referenced components discovered through an explicit contract?

- If the code is still guessing from type names, stop and fix it.

4. Does imported component resolution feel like the same world as external-control imports?

- If components and external controls now use visibly different namespace mental models, the plan is drifting away from the roadmap direction.

5. Did we accidentally create a build system that looks incremental but still churns files?

- Check both target skipping and file timestamp stability.

6. Can a test project consume components without re-running generation over the referenced source files?

- If not, the test-project goal is still open.

7. Did we accidentally force generated code to reference generator-only internals?

- If yes, fix the boundary before continuing.

8. Did project-level helper output become clearer or murkier?

- If manifests, registration, and support files are scattered across namespaces with no clean rule, stop and simplify.

9. Did the resolver stay understandable?

- If the tag-resolution code now feels like multiple interleaved search engines, refactor before moving on.

10. Would a new engineer know how to import a referenced component?

- The intended answer should be one sentence:
  - "Use a normal `using`, and alias it if you need to qualify the tag."

If the answer requires caveats, there is probably still too much magic.

---

## 12. Stop Conditions

Stop and refactor before continuing if any of these happen:

- shared build logic starts splitting into per-project variants again
- generator output still rewrites unchanged files after the incremental phase
- referenced component discovery relies on naming heuristics instead of manifest metadata
- generated app code gains a compile dependency on `Csxaml.Generator`
- namespace fallback becomes more complicated than explicit-file-namespace or project-default-namespace
- component and external-control import logic diverge into separate mental models
- test-project proof still depends on duplicate generation wiring
- design-time stabilization work starts turning into Milestone 11 tooling implementation

Do not push through these warnings. Simplify first.

---

## 13. Exit Criteria Restated

Milestone 10 is complete only when:

- there is one shared, understandable CSXAML build path
- project default namespace behavior is explicit, deterministic, and no longer centered on `GeneratedCsxaml`
- internal generated support files have a deterministic home
- referenced component libraries export an explicit manifest contract
- downstream projects can import referenced components through normal `using` semantics
- test projects can consume generated components through normal references without duplicate generation wiring
- unchanged builds avoid needless regeneration churn
- rename/delete/clean/rebuild scenarios are covered by proof
- roadmap and language-spec documentation match the implementation that actually landed

If any one of those is still unproven, Milestone 10 is not done.
