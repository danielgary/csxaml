# Post-Render Language Spec Tightening Plan

## Status

- Drafted: 2026-04-15
- Executed: 2026-04-15
- Replaces: the earlier repo-root plan that centered on `return`-based final markup syntax
- Inputs:
  - `LANGUAGE-SPEC.md`
  - latest review feedback
  - `ROADMAP.md`

## Execution Outcome

This plan has now been executed through spec, generator, tooling, runtime-audit, and regression-test updates.

Closed in this pass:

- the spec now carries a maintained revision log for the final pre-1.0 tightening pass
- `using static` is implemented and tested as an ordinary C# import path that does not affect tag resolution
- slot placement is narrowed in both spec and validation so `<Slot />` is rejected inside `foreach`
- bounded-island scanning promises are narrowed to the lexical burden an implementation must actually satisfy, with raw-string regression coverage
- controlled-input wording now matches the retained WinUI adapter reality more closely, including equality suppression, selection/focus preservation goals, and explicit IME caveats
- duplicate-key, disposal, projection-failure, helper-declaration, and `DataContext` wording is tightened to reflect the current codebase and intentional v1 boundaries

No open implementation mismatches from this pass remain in this file.

## Purpose

The `render <Root />;` change is settled. The next pass should not reopen that decision or sprawl into a new language redesign.

This plan is for the remaining contract gaps that still matter after the `render`, lifecycle, WinUI interop, and duplicate-key clarifications:

- places where the source surface still looks more like C# than it really is
- places where the runtime contract is clear in intent but still fuzzy in edge behavior
- places where the spec needs to be more explicit about what v1 does not try to abstract
- places where the document is starting to drift from language contract into runtime/product-slice manual territory
- places where parser simplicity or generic runtime wording may understate real WinUI and C# implementation complexity

The goal is a tighter, more honest v1 document, not a bigger one.

## Fixed Decisions For This Pass

These should be treated as settled while executing this plan:

- `render <Root />;` remains the only valid final markup statement
- `component Element` stays for v1, but the spec should explain it more plainly
- one component declaration per `.csxaml` file stays for v1
- `State<T>` remains assignment-driven in this pass
- this pass should prefer surgical clarification over adding large new source features

## Important Non-Decision

The biggest open design question from the latest review is whether `State<T>.Value` should keep rerendering on reference-equal assignment.

This plan does **not** flip that contract yet.

Instead, this pass should:

- explain the current cost plainly
- make the current behavior feel intentional rather than accidental
- record any future equality-bailout or `Touch()`-style design as follow-up work rather than sneaking it into the spec without runtime design

## Primary Outcomes

After this pass, the spec should answer these questions cleanly:

- what does a `State<T>` declaration mean at the source level, if it is not literally a C# field initializer?
- what happens when code tries to write state during `render`?
- are file-local helper declarations actually part of v1 or still only an aspiration?
- how much modern C# lexical complexity can bounded-island scanning really promise without effectively reusing Roslyn-grade lexing behavior?
- what exactly does "suppress feedback loops" mean for controlled input?
- does the runtime contract say enough about cursor, selection, focus, and IME preservation to imply specialized adapters instead of naive property setters?
- what exactly is the contract for normalized control event shapes?
- where may `<Slot />` appear, and where must it not appear?
- when duplicate sibling keys are statically detectable, do they fail at compile time?
- when and where do `inject` declarations resolve, and on what host/dispatcher assumptions?
- does CSXAML support `using static`, and if so, what does it affect?
- what parts of app shell and navigation remain host-side rather than CSXAML-owned?
- which supported-slice details belong in the core spec versus prototype coverage or compatibility docs?
- which currently deferred areas are serious production blockers rather than casual future nice-to-haves?

## Agent Execution Rules

This document is meant to be executable by another agent, not just read as design commentary.

For every workstream below, the implementing agent must do all of the following:

1. update the normative spec text and any affected examples
2. audit the mapped code areas and either implement the required behavior or record a precise gap
3. update or add regression tests
4. run the relevant test projects
5. record any remaining mismatch as an explicit issue in this file and in `ROADMAP.md` if it materially affects scope, risk, or v1 credibility

No workstream counts as complete when only the prose changed.

If the agent discovers that the desired spec wording and the current implementation diverge, it must do exactly one of these before closing the workstream:

- fix the implementation
- narrow the spec wording
- record the gap explicitly as unresolved

Silent drift is not an allowed outcome.

## Codebase Tracking Map

Use this map to keep the work grounded in the actual repo rather than in generic language-design talk.

### 1. Spec and author-facing docs

- `LANGUAGE-SPEC.md`
- `docs/external-control-interop.md`
- `docs/debugging-and-diagnostics.md`
- demo-facing `.csxaml` examples under `Csxaml.Demo`

### 2. Parser and bounded-island scanning

- `Csxaml.Generator/Parsing/Parser.cs`
- `Csxaml.Generator/Parsing/RenderStatementLocator.cs`
- `Csxaml.Generator/Parsing/ComponentHelperCodeParser.cs`
- `Csxaml.Generator/Parsing/CSharpTextScanner.cs`
- `Csxaml.Generator/Parsing/FileMemberBoundaryScanner.cs`
- `Csxaml.Generator/Parsing/ChildNodeParser.cs`
- `Csxaml.Generator/Parsing/UsingDirectiveParser.cs`
- `Csxaml.Generator/Parsing/NamespaceDirectiveParser.cs`

### 3. Generator validation, AST, and source mapping

- `Csxaml.Generator/Validation/SlotDefinitionValidator.cs`
- `Csxaml.Generator/Ast/StateFieldDefinition.cs`
- `Csxaml.Generator/Ast/SlotOutletNode.cs`
- `Csxaml.Generator/Ast/ComponentHelperCodeBlock.cs`
- `Csxaml.Generator/Ast/FileHelperCodeBlock.cs`
- `Csxaml.Generator/SourceMapping/GeneratedDiagnosticMapper.cs`
- `Csxaml.Generator/SourceMapping/GeneratedSourceMapWriter.cs`

### 4. Runtime state, lifecycle, and reconciliation

- `Csxaml.Runtime/State/State.cs`
- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Runtime/Reconciliation/ComponentMatchKey.cs`
- `Csxaml.Runtime/Rendering/WinUiNodeRenderer.cs`
- `Csxaml.Runtime/Rendering/RenderedChildMatcher.cs`

### 5. Interactive-control and projection adapters

- `Csxaml.Runtime/Adapters/TextBoxControlAdapter.cs`
- `Csxaml.Runtime/Adapters/CheckBoxControlAdapter.cs`
- `Csxaml.Runtime/Adapters/ControlledTextInputState.cs`
- `Csxaml.Runtime/Adapters/ControlledBoolInputState.cs`
- `Csxaml.Runtime/Adapters/TextSelectionRange.cs`
- `Csxaml.Runtime/Adapters/NativeEventBindingStore.cs`
- `Csxaml.Runtime/Adapters/RenderedNativeElement.cs`
- `Csxaml.Runtime/Adapters/UiElementCollectionPatcher.cs`
- `Csxaml.Runtime/Adapters/ExternalEventBinder.cs`
- `Csxaml.Runtime/Adapters/ExternalControlAdapter.cs`
- `Csxaml.Runtime/Adapters/ExternalControlDescriptor.cs`
- `Csxaml.Runtime/Adapters/ExternalControlRegistry.cs`

### 6. Tooling, projection, and formatting

- `Csxaml.Tooling.Core/Net10/Formatting/CsxamlFormattingService.cs`
- `Csxaml.Tooling.Core/Net10/SemanticTokens/CsxamlSemanticTokenService.cs`
- `Csxaml.Tooling.Core/Net10/CSharp/CsxamlCSharpProjectionBuilder.cs`
- `Csxaml.Tooling.Core/Net10/CSharp/CsxamlRenderProjectionEmitter.cs`
- `VSCodeExtension/syntaxes/csxaml.tmLanguage.json`
- `VSCodeExtension/snippets/csxaml.code-snippets`

### 7. Test suites to keep aligned

- parsing:
  - `Csxaml.Generator.Tests/Parsing/ParserTests.cs`
  - `Csxaml.Generator.Tests/Parsing/HelperCodeParserTests.cs`
  - `Csxaml.Generator.Tests/Parsing/SlotParserTests.cs`
  - `Csxaml.Generator.Tests/Parsing/InjectParserTests.cs`
  - `Csxaml.Generator.Tests/Parsing/AttachedPropertyParserTests.cs`
- validation and emission:
  - `Csxaml.Generator.Tests/Validation/SlotValidationTests.cs`
  - `Csxaml.Generator.Tests/Emission/SlotEmissionTests.cs`
  - `Csxaml.Generator.Tests/Emission/HelperCodeEmissionTests.cs`
- diagnostics and source mapping:
  - `Csxaml.Generator.Tests/Diagnostics/BuildDiagnosticMappingTests.cs`
  - `Csxaml.Generator.Tests/Diagnostics/SourceMappingTests.cs`
- runtime state, lifecycle, and reconciliation:
  - `Csxaml.Runtime.Tests/State/StateTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorStateTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorRenderPhaseTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorReconciliationTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorSlotTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/InjectedServiceResolutionTests.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/ComponentLifecycleTests.cs`
- runtime adapters and projection:
  - `Csxaml.Runtime.Tests/Adapters/ControlledTextInputStateTests.cs`
  - `Csxaml.Runtime.Tests/Adapters/ControlledBoolInputStateTests.cs`
  - `Csxaml.Runtime.Tests/Adapters/TextSelectionRangeTests.cs`
  - `Csxaml.Runtime.Tests/Adapters/ExternalControlAdapterTests.cs`
  - `Csxaml.Runtime.Tests/Adapters/ExternalControlStyleTests.cs`
  - `Csxaml.Runtime.Tests/Adapters/AttachedPropertyApplicatorTests.cs`
  - `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererTests.cs`
  - `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererRetentionTests.cs`
  - `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererTextBoxProjectionTests.cs`

## Per-Workstream Closure Checklist

Use this checklist for every workstream before marking it complete:

- [ ] spec text updated
- [ ] examples/docs updated
- [ ] mapped code areas audited
- [ ] implementation updated or explicit gap recorded
- [ ] regression tests added or updated
- [ ] relevant test commands run
- [ ] unresolved issues logged below
- [ ] `ROADMAP.md` updated if scope/risk changed

## Issue Tracking Rules

Every issue found while implementing this plan must end up in one of three states:

- fixed in code and covered by tests
- narrowed in the spec so the mismatch no longer exists
- recorded as an unresolved gap with exact file paths and a short reason

Do not leave vague "needs follow-up" prose in commit messages or agent notes without also recording it here and, when material, in `ROADMAP.md`.

## Implementation Issue Log

Fill this in during execution. Do not leave discovered mismatches untracked.

| ID | Workstream | File(s) | Problem | Resolution | Status |
| --- | --- | --- | --- | --- | --- |
| FIXED-1 | 2 / 7 | `Csxaml.Generator/Parsing/UsingDirectiveParser.cs`; `Csxaml.Generator/Emission/ComponentEmitter.cs`; `Csxaml.Tooling.Core/Common/Markup/CsxamlUsingDirectiveScanner.cs`; `Csxaml.Tooling.Core/Net10/Completion/*`; tests | `using static` was still a spec question and not fully wired through parser/emitter/tooling behavior | implemented `using static` end to end for ordinary C# lookup, excluded it from tag resolution, and added generator/tooling regression coverage | closed |
| FIXED-2 | 4 | `Csxaml.Generator/Validation/SlotDefinitionValidator.cs`; `Csxaml.Generator.Tests/Validation/SlotValidationTests.cs`; `LANGUAGE-SPEC.md` | default-slot placement rules were underspecified and `<Slot />` inside `foreach` remained accepted by validation | narrowed the spec and validator so slot outlets are rejected inside `foreach`, with regression coverage | closed |
| FIXED-3 | 2.25 / 3 | `Csxaml.Generator/Parsing/CSharpTextScanner.cs`; `Csxaml.Generator.Tests/Parsing/CSharpIslandParserTests.cs`; `LANGUAGE-SPEC.md` | bounded-island wording overpromised lightweight scanning without proving modern raw/interpolated string handling | kept the existing lexical scanner, added raw-string regression tests, and tightened the spec so conforming implementations must actually satisfy modern lexical cases | closed |
| FIXED-4 | 1 / 3 / 5 / 8 / 9 / 10 | `LANGUAGE-SPEC.md`; `plan.md`; `ROADMAP.md` | final pre-1.0 contract gaps remained around helper declarations, revision tracking, controlled-input expectations, duplicate-key phase, failure wording, and deferred WinUI boundaries | updated the spec, added a maintained revision log, narrowed promises to current behavior, and recorded remaining interop boundaries explicitly | closed |

## Workstream 1 - State Source Semantics

This is the most important wording fix left in the document.

### Required spec changes

- state declarations must be described as CSXAML component-prologue declarations, not as literal C# field initializers
- initialization must be defined in source order during component instance creation
- later text should refer back to the lifecycle ordering section rather than restating half the contract in multiple places
- examples should stop accidentally implying that ordinary C# field-initializer rules apply unchanged

### Normative direction

The spec should say plainly that state initializers:

- run once per component instance creation
- run after inject resolution
- run in source order
- may reference earlier `inject` declarations
- may reference earlier `State<T>` declarations
- may reference component parameters / incoming props in the same source-level instance-creation context

The generator is responsible for lowering those declarations into whatever C# shape preserves that contract.

### Follow-through

- update the state syntax/semantics section
- update the lifecycle ordering section so it remains the canonical source for initialization order
- update examples that currently look like impossible raw C# field initializers
- audit generator/runtime ordering assumptions in:
  - `Csxaml.Generator/Ast/StateFieldDefinition.cs`
  - `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
  - `Csxaml.Runtime.Tests/Reconciliation/InjectedServiceResolutionTests.cs`
  - `Csxaml.Runtime.Tests/State/StateTests.cs`

## Workstream 2 - Render-Time State Writes

The current runtime error is not enough on its own.

### Required spec changes

- render-time state writes remain invalid
- statically detectable render-time `State<T>.Value` assignments should be diagnosed at compile time
- cases that depend on runtime control flow may still fail only at runtime

### Scope boundary

The compile-time rule should cover only cases that are genuinely local and detectable, such as assignments lexically inside the render payload or expression islands that execute during render.

It should not pretend to prove behavior for deferred lambdas, async callbacks, or arbitrary helper-method bodies when that would require speculative whole-program reasoning.

### Codebase tracking focus

- `Csxaml.Runtime/State/State.cs`
- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorRenderPhaseTests.cs`
- `Csxaml.Runtime.Tests/State/StateTests.cs`

## Workstream 2.25 - Bounded-Island Lexing Realism

The current bounded-island story is conceptually right, but the implementation risk is easy to understate.

### Required spec and implementation-planning changes

- treat modern C# lexical forms inside islands as a real conformance burden, not a casual scanner detail
- add explicit conformance coverage for interpolated strings, raw strings, interpolated raw strings, comments, and nested interpolation holes
- make sure the spec does not promise "copy-paste safe modern C#" if the implementation is still a lightweight delimiter counter that cannot actually honor those lexical forms

### Recommended direction

This pass should not design a new parser architecture, but it should record the real engineering constraint plainly:

- either bounded-island scanning must become robust enough to match Roslyn-level lexical realities for the supported forms
- or the spec/compatibility wording must narrow the promise instead of implying that any lightweight scanner can do the job

### Codebase tracking focus

- `Csxaml.Generator/Parsing/CSharpTextScanner.cs`
- `Csxaml.Generator/Parsing/ComponentHelperCodeParser.cs`
- `Csxaml.Generator/Parsing/Parser.cs`
- `Csxaml.Generator.Tests/Parsing/ParserTests.cs`
- `Csxaml.Generator.Tests/Parsing/HelperCodeParserTests.cs`

## Workstream 2.5 - Render Statement Detection Wording

The `render` direction is correct, but the parsing section can still be made easier to trust.

### Required spec changes

- add one or two concrete valid/invalid examples directly in the parsing section, not only later in diagnostics
- keep the parser rule formal enough to be testable without pretending it is elegant
- keep the fuller `render` rationale in one canonical place and shorten the duplicate explanation elsewhere

### Recommended direction

The parsing section should explicitly show at least one valid helper-code example and one invalid malformed final-statement example so the reader does not have to reconstruct the rule only from prose.

### Codebase tracking focus

- `Csxaml.Generator/Parsing/RenderStatementLocator.cs`
- `Csxaml.Generator/Parsing/Parser.cs`
- `Csxaml.Tooling.Core/Net10/Formatting/CsxamlFormattingService.cs`
- `Csxaml.Tooling.Core/Net10/SemanticTokens/CsxamlSemanticTokenService.cs`

## Workstream 3 - Controlled Input Semantics

The spec now has the right general posture. This pass should make it precise enough to survive real text input.

### Required spec changes

- define feedback-loop suppression in terms of the control's documented normalized value contract or metadata/adapter comparison rule
- avoid vague "same value" wording with no comparison rule behind it
- acknowledge active text composition / IME scenarios explicitly
- keep event-payload normalization limited to controls whose normalized shape is actually documented
- say whether normalized event shapes are represented in metadata, compatibility documentation, or both
- say whether compile-time validation binds against that normalized shape when it exists
- say whether a control may expose both raw and normalized forms in v1; this plan prefers "not unless explicitly documented"
- keep naming of normalized forms explicit and predictable rather than adapter-only folklore
- state more clearly that interactive-control support cannot be modeled as naive generic property reapplication
- call out cursor, selection, focus, and IME preservation as adapter responsibilities for controlled built-in inputs

### Recommended direction

The spec should say that controlled-value reapplication may be deferred or suppressed during active IME composition or an equivalent in-progress platform input state when immediate projection would disrupt native input behavior.

### Follow-through

- tighten `TextBox` / `CheckBox` examples and wording
- expand conformance expectations for controlled-input stability
- keep runtime wording honest that specialized control adapters are part of the viability story for interactive controls

### Codebase tracking focus

- `Csxaml.Runtime/Adapters/TextBoxControlAdapter.cs`
- `Csxaml.Runtime/Adapters/CheckBoxControlAdapter.cs`
- `Csxaml.Runtime/Adapters/ControlledTextInputState.cs`
- `Csxaml.Runtime/Adapters/ControlledBoolInputState.cs`
- `Csxaml.Runtime/Adapters/TextSelectionRange.cs`
- `Csxaml.Runtime/Adapters/NativeEventBindingStore.cs`
- `Csxaml.Runtime.Tests/Adapters/ControlledTextInputStateTests.cs`
- `Csxaml.Runtime.Tests/Adapters/ControlledBoolInputStateTests.cs`
- `Csxaml.Runtime.Tests/Adapters/TextSelectionRangeTests.cs`
- `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererTextBoxProjectionTests.cs`

## Workstream 4 - Slot Placement and Reuse Rules

The default-slot transport story is mostly there. The remaining problem is where a slot outlet may appear.

### Required spec changes

- a component definition may contain at most one syntactic default-slot outlet
- that outlet must appear only where ordinary child markup nodes are valid
- `<Slot />` may appear inside `if` blocks
- `<Slot />` must not appear inside `foreach` blocks
- fallback content inside `<Slot> ... </Slot>` remains deferred and invalid

### Why this matters

Without this, the current text leaves too much room for repeated-slot rendering, identity confusion, and incompatible future semantics.

### Codebase tracking focus

- `Csxaml.Generator/Validation/SlotDefinitionValidator.cs`
- `Csxaml.Generator/Ast/SlotOutletNode.cs`
- `Csxaml.Generator.Tests/Validation/SlotValidationTests.cs`
- `Csxaml.Generator.Tests/Parsing/SlotParserTests.cs`
- `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorSlotTests.cs`

## Workstream 5 - Duplicate-Key Failure Phase

Duplicate sibling keys are now covered, but the failure phase should be tighter.

### Required spec changes

- when duplicate sibling keys are statically detectable, implementations should report them as compile-time diagnostics
- when duplicate sibling keys depend on dynamic runtime values, implementations should fail deterministically at runtime
- the spec should stop sounding like either phase is equally arbitrary for every case

### Follow-through

- tighten the key/identity section wording
- add conformance coverage for both statically detectable and runtime-detected duplicate-key paths where applicable

### Codebase tracking focus

- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Runtime/Reconciliation/ComponentMatchKey.cs`
- `Csxaml.Runtime/Rendering/RenderedChildMatcher.cs`
- `Csxaml.Runtime.Tests/Reconciliation/ComponentTreeCoordinatorReconciliationTests.cs`
- `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererRetentionTests.cs`

## Workstream 6 - Inject Resolution, Dispatcher, and Host Affinity

The runtime order is better now, but it still needs stronger host-level assumptions.

### Required spec changes

- the instance-creation/render sequence defined in the lifecycle section should be stated as executing on the owning host dispatcher or UI scheduler
- v1 `inject` resolution is synchronous
- async-only service resolution is not part of the component contract and must be handled by the host before component instantiation if needed
- the host dispatcher captured for a component instance should remain the authority for later UI work

### Recommended direction

For cross-host or cross-dispatcher reparenting, the spec should choose one honest answer and state it directly:

- either the dispatcher is captured at instantiation and does not migrate
- or such reparenting is undefined in v1

This plan prefers the second wording because it promises less and matches current experimental posture better.

### Codebase tracking focus

- `Csxaml.Runtime/Reconciliation/ComponentTreeCoordinator.cs`
- `Csxaml.Runtime.Tests/Reconciliation/InjectedServiceResolutionTests.cs`
- `Csxaml.Runtime.Tests/Reconciliation/ComponentLifecycleTests.cs`

## Workstream 7 - File Surface Cleanup

These are smaller changes, but they remove recurring paper cuts.

### 7.1 `using static`

The spec should stop omitting this silently.

Recommended direction:

- allow `using static` only as an ordinary C# name-import mechanism for helper code and expression islands
- state explicitly that it does not create tag prefixes
- state explicitly that it does not change attached-property owner resolution rules unless the implementation deliberately grows that support later

### 7.2 `component Element`

Add one short explicit rule:

- `Element` is the only currently defined component kind
- future kinds may be added later without changing existing `component Element` declarations

### 7.3 App-shell posture

Add a short line near the front of the spec that CSXAML is not, in v1, an app-shell or navigation framework.

That should point readers toward host-side `Page`, `Frame`, `NavigationView`, and related app-structure concerns instead of leaving them implicit.

### 7.4 File-local helper type example

Add one compact example of a file-local helper type or enum so the spec is not only describing the feature abstractly.

### 7.5 File-local helper declaration status

The spec should stop sounding unsure about whether file-local helper declarations are actually part of v1.

Recommended direction:

- if they are intended v1 surface, say so normatively
- if the implementation still trails that contract in places, record that as a prototype gap rather than leaving the language rule soft

### Codebase tracking focus

- `Csxaml.Generator/Parsing/FileMemberBoundaryScanner.cs`
- `Csxaml.Generator/Ast/FileHelperCodeBlock.cs`
- `Csxaml.Generator/Ast/ComponentHelperCodeBlock.cs`
- `Csxaml.Generator.Tests/Emission/HelperCodeEmissionTests.cs`
- `Csxaml.Generator.Tests/Parsing/HelperCodeParserTests.cs`

## Workstream 8 - Failure and Disposal Precision

The current render/projection failure wording is much better, but it still needs cleaner framing.

### Required spec changes

- make it clearer that partially applied native projection after failure is a known limitation of the v1 retained-projection model, not just an incidental detail
- clarify the relation between the last successful logical/rendered model and the partially updated native tree after a failed projection pass
- tighten disposal semantics enough to answer at least:
  - whether sibling disposal order is deterministic
  - whether child-disposal failure aborts parent disposal or whether cleanup continues
  - whether disposal failures should be wrapped/aggregated in a controlled way

### Scope boundary

This pass does not need to invent a large disposal framework. It does need to keep the runtime contract from sounding complete when key cleanup-failure questions are still unspecified.

### Codebase tracking focus

- `Csxaml.Runtime/Rendering/WinUiNodeRenderer.cs`
- `Csxaml.Runtime/Adapters/RenderedNativeElement.cs`
- `Csxaml.Runtime/Adapters/NativeEventBindingStore.cs`
- `Csxaml.Runtime.Tests/Reconciliation/ComponentLifecycleTests.cs`
- `Csxaml.Runtime.Tests/Rendering/WinUiNodeRendererTests.cs`

## Workstream 9 - Conformance and Spec-Boundary Tightening

The conformance section should reflect the sharper contract.

### Required additions

- state-initializer ordering that references earlier `inject` declarations
- compile-time versus runtime handling of render-time state writes
- bounded-island scanning coverage for modern C# lexical forms
- slot-under-`foreach` rejection
- compile-time versus runtime duplicate-key handling
- controlled-input feedback-loop suppression behavior
- IME / in-progress text-input preservation for the supported `TextBox` slice
- event normalization coverage for controls with documented normalized delegate shapes

### Spec-boundary cleanup

This pass should also trim avoidable language/runtime/product blending.

Recommended direction:

- keep core language sections focused on the language contract
- keep support-slice inventories and current built-in-control lists in prototype coverage or compatibility docs when they are not themselves language rules
- audit lingering `SHOULD` versus `MUST` wording in central v1 areas, especially where the document currently sounds more aspirational than intended

### Known-gap honesty

If the implementation still trails the spec in any of these areas, the spec and roadmap should describe the gap explicitly instead of letting the reader infer support from examples alone.

### Codebase tracking focus

- `LANGUAGE-SPEC.md`
- `ROADMAP.md`
- `docs/external-control-interop.md`
- `docs/debugging-and-diagnostics.md`
- `Csxaml.Generator.Tests/Diagnostics/BuildDiagnosticMappingTests.cs`
- `Csxaml.Generator.Tests/Diagnostics/SourceMappingTests.cs`

## Workstream 10 - Deferred But High-Risk Product Gaps

Some deferred surfaces now need to be tracked more explicitly as production blockers, even if this pass does not design them.

### Required planning changes

- record virtualization/template interop as a high-risk gap rather than a casual future enhancement
- record `DataContext` projection/interoperability for external control subtrees as a serious ecosystem interop problem
- record named slots as a meaningful composition limitation, not just a cosmetic omission

### Recommended direction

This pass should not invent:

- a `<Virtualize />` primitive
- a full `ItemsRepeater`/`DataTemplate` story
- a subtree `DataContext` bridge syntax
- named-slot syntax

But the plan and roadmap should make clear that these are likely required before broad production claims about heavy list UIs, third-party control ecosystems, or richer layout primitives become credible.

### Codebase tracking focus

- `LANGUAGE-SPEC.md`
- `ROADMAP.md`
- `docs/external-control-interop.md`
- runtime external-control adapter and registry files under `Csxaml.Runtime/Adapters`

## Changes This Plan Does Not Try To Make

This pass should not quietly widen scope into any of the following:

- removing `component Element`
- relaxing one-component-per-file
- inventing hook-style lifecycle syntax
- generalizing controlled input to every control in the platform
- adding named slots or slot fallback content
- changing `State<T>` equality semantics without a deliberate runtime/API design
- introducing imperative element handles / `ref`

Those are valid future topics. They are not the job of this pass, but some of them now need to be recorded more clearly as high-risk follow-up rather than soft someday work.

## Execution Order

1. tighten `State<T>` source semantics and lifecycle cross-references
2. tighten render-time state-write diagnostics
3. tighten bounded-island scanning realism and conformance wording
4. tighten render-statement detection wording and examples
5. tighten controlled-input equality, IME, normalization, and adapter wording
6. tighten slot placement rules
7. tighten duplicate-key failure phase wording
8. tighten inject/dispatcher/host wording
9. clean up `using static`, helper-declaration status, app-shell posture, and `component Element` wording
10. tighten failure/disposal framing
11. update conformance expectations, known-gap notes, and high-risk deferred-product notes

## Verification Commands

Run the relevant subset after each workstream and the full sweep at the end.

### Targeted regression commands

```powershell
dotnet test Csxaml.Generator.Tests\Csxaml.Generator.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.Runtime.Tests\Csxaml.Runtime.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.Tooling.Core.Tests\Csxaml.Tooling.Core.Tests.csproj -m:1 /p:UseSharedCompilation=false
dotnet test Csxaml.ProjectSystem.Tests\Csxaml.ProjectSystem.Tests.csproj -m:1 /p:UseSharedCompilation=false
```

### Drift-detection checks

Use focused `rg` sweeps to confirm the repo is telling one story:

```powershell
rg "return <|return \\(" LANGUAGE-SPEC.md docs Csxaml.Demo VSCodeExtension
rg "render <" LANGUAGE-SPEC.md docs Csxaml.Demo VSCodeExtension
rg "OnTextChanged|OnCheckedChanged|Slot|DataContext|ItemsRepeater|ControlTemplate" LANGUAGE-SPEC.md docs
```

## Final Agent Sign-Off

Before closing the plan execution, the implementing agent must state explicitly:

- which workstreams were completed
- which files changed for each completed workstream
- which tests were run
- which issue-log entries remain open, if any
- whether `ROADMAP.md` was updated to reflect any unresolved or re-scoped work

## Definition of Done

This plan is complete only when all of the following are true:

- the spec no longer reads like state declarations are raw C# field initializers
- the spec distinguishes statically detectable render-time state writes from runtime-only failures
- the bounded-island promise is either backed by realistic lexical-conformance language or narrowed so it does not overpromise
- the parsing section includes concrete examples that make the final `render` rule easier to trust
- the controlled-input section says enough about comparison, cursor/selection behavior, and composition behavior to be implementable
- the event-normalization contract says enough about shape, validation, and documentation to stop feeling implicit
- the slot section clearly answers where `<Slot />` may appear
- the helper-declaration sections no longer wobble between "part of v1" and "just a goal"
- the duplicate-key section distinguishes compile-time diagnostics from dynamic runtime failures where appropriate
- the inject/lifecycle sections clearly answer dispatcher and synchronous-resolution assumptions
- the file-level import story says something explicit about `using static`
- the front of the spec stops implying that CSXAML owns app-shell/navigation concerns
- the projection/disposal sections either answer the important cleanup/failure questions or mark them as explicit v1 limitations
- the core language sections are a bit cleaner about what belongs there versus in product-scope/support-slice notes
- the plan/spec/roadmap now treat virtualization, `DataContext` interop, and named-slot limitations as explicit production risks instead of buried deferments
- the conformance section covers the new guarantees instead of only the older grammar/runtime ones

If a careful reader can still ask "but is that really a C# field initializer?", "can `<Slot />` appear in a `foreach`?", "is helper code actually v1?", "when does a duplicate key fail?", "will a lightweight scanner really survive modern C# strings?", or "what happens to cursor state and IME composition?" after this pass, the work is not done.
