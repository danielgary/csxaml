# AGENTS.md

## Purpose

This project is a small compiler/runtime experiment for CSXAML. The primary goal is not just to make it work. The primary goal is to make it easy for a human to read, debug, extend, and trust.

Every implementation decision must optimize for:

1. correctness
2. readability
3. small understandable units
4. explicit behavior
5. low surprise
6. maintainable file and type boundaries

Do not collapse major logic into giant files. Do not hide complexity behind clever abstractions. Do not trade clarity for terseness.

---

## Repository Layout

This repo is split into a few clear layers so a reader can find the compiler, runtime, tooling, and demo code without guessing.

### Root-level docs and build files

- `LANGUAGE-SPEC.md`  
  The language contract for CSXAML.

- `ROADMAP.md`  
  Milestones, scope, and current project status.

- `README.md`  
  Human-readable repo overview.

- `plan.md`  
  Active execution plan for larger spec/runtime/tooling changes.

- `Csxaml.sln`  
  The main solution file.

- `Directory.Build.props` / `Directory.Build.targets`  
  Shared build configuration for the repo.

- `NuGet.Config`  
  Restore sources and package configuration.

### Root-level folders

- `build/`  
  Shared MSBuild targets and generation wiring.

- `docs/`  
  Supporting docs such as debugging notes and external-control interop details.

- `scripts/`  
  Helper scripts for development workflows.

- `VSCodeExtension/`  
  VS Code extension assets, grammar, snippets, and extension host code.

- `artifacts/`  
  Built artifacts and packaged outputs.

- `.vscode/`  
  Local editor and workspace settings.

## Project Inventory

This is the quick reference for every project in the solution and what it is responsible for.

### Core compiler and metadata projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `Csxaml.ControlMetadata` | library | Shared metadata model for controls, properties, events, child-content rules, and value-kind hints. |
| `Csxaml.ControlMetadata.Generator` | executable | Generates control metadata from WinUI and supported external controls into the shared metadata model. |
| `Csxaml.Generator` | executable | The CSXAML compiler/source generator pipeline: tokenization, parsing, validation, source mapping, and C# emission. |

### Runtime and support projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `Csxaml.Runtime` | library | Retained-mode runtime: nodes, component instances, state invalidation, reconciliation, adapter wiring, WinUI projection, and runtime diagnostics. |
| `Csxaml.Testing` | library | Test support helpers used by runtime and integration tests. |

### Tooling and editor projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `Csxaml.Tooling.Core` | library | Shared language-service logic: markup scanning, C# projection, completion, definitions, formatting, semantic tokens, and workspace loading. |
| `Csxaml.LanguageServer` | executable | LSP host built on top of `Csxaml.Tooling.Core`. |
| `Csxaml.VisualStudio` | extension project | Visual Studio-specific host for document registration, language-server startup, and VSIX packaging. |

### Demo and fixture projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `Csxaml.Demo` | WinUI app | Main demo application showing the current CSXAML authoring model end to end. |
| `Csxaml.ExternalControls` | WinUI library | Small sample external controls used to prove external-control metadata generation and runtime interop. |
| `Csxaml.ProjectSystem.Components` | WinUI library | Fixture component library used to verify normal project-reference consumption of generated CSXAML components. |
| `Csxaml.ProjectSystem.Consumer` | WinUI app | Fixture consumer app that references `Csxaml.ProjectSystem.Components` to prove project-system and build integration. |

### Test projects

| Project | Kind | Purpose |
| --- | --- | --- |
| `Csxaml.ControlMetadata.Generator.Tests` | test project | Regression coverage for metadata extraction and generation. |
| `Csxaml.Generator.Tests` | test project | Coverage for tokenizer, parser, validation, emission, diagnostics, and source mapping. |
| `Csxaml.Runtime.Tests` | test project | Coverage for state invalidation, lifecycle, reconciliation, native projection, adapters, retained identity, and runtime diagnostics. |
| `Csxaml.Tooling.Core.Tests` | test project | Coverage for completion, definitions, formatting, semantic tokens, C# projection, and language-server protocol behavior. |
| `Csxaml.VisualStudio.Tests` | test project | Coverage for VSIX packaging and Visual Studio host integration behavior. |
| `Csxaml.ProjectSystem.Tests` | test project | End-to-end proof that generated CSXAML components work through ordinary project references and repo build wiring. |

### Important non-project deliverables

- `VSCodeExtension/`  
  The VS Code extension lives outside the `.sln`, but it is still one of the main editor-facing deliverables in the repo.

## Where To Start

If you are new to the repo, read in this order:

1. `LANGUAGE-SPEC.md` for the intended language surface
2. `Csxaml.Generator/` for how `.csxaml` turns into generated C#
3. `Csxaml.Runtime/` for how generated trees become retained WinUI UI
4. `Csxaml.Demo/` for the current end-to-end example
5. `Csxaml.Tooling.Core/`, then `Csxaml.LanguageServer/` or `Csxaml.VisualStudio/`, for editor behavior

---

## Non-Negotiables

### 1. Keep files small

Prefer many small files over a few giant files.

Target guidelines:

- ideal file size: under 200 lines
- warning threshold: 300 lines
- hard stop: 400 lines unless there is a very strong reason

If a file starts getting large, stop and split it.

### 2. One responsibility per file

Each file should have one clear job.

Good examples:

- `Tokenizer.cs`
- `Parser.cs`
- `Token.cs`
- `AstNodes.cs`
- `CodeEmitter.cs`
- `EmitterContext.cs`
- `Renderer.cs`

Bad examples:

- one file containing tokenizer, parser, AST, emitter, diagnostics, and CLI
- one runtime file containing node types, reconciliation, host, renderer, and state

### 3. One major type per file

Do not stack many unrelated classes into a single source file.

Preferred:

- one public type per file
- small related internal helper types may live nearby only if they are tiny and tightly coupled

### 4. No "god objects"

Do not create central objects that know too much or do too much.

Avoid classes like:

- `CompilerManager` that parses, validates, emits, and writes files
- `RuntimeEngine` that owns state, rendering, reconciliation, event wiring, and WinUI projection
- `ParserHelpers` giant utility classes full of unrelated static methods

If a type is accumulating unrelated reasons to change, split it.

### 5. No clever parser shortcuts if they hurt readability

This project is a compiler-like system. The parser must be easy to follow.

Prefer:

- explicit tokenization
- explicit parse methods
- explicit AST node types
- explicit diagnostics

Avoid:

- giant regex-driven parsing
- deeply nested ad hoc string slicing logic
- parser code that mixes lexing, parsing, validation, and codegen

### 6. No mixing layers

Keep these layers separate:

- syntax model
- parser
- semantic validation
- code emission
- runtime model
- WinUI host/rendering
- demo app code

Do not let one layer reach through and do another layer's job.

---

## Required Project Structure Discipline

Use small, obvious folders.

Example structure:

/Csxaml.Generator
/Cli
/Syntax
/Parsing
/Ast
/Validation
/Emission
/IO
/Diagnostics

/Csxaml.Runtime
/Nodes
/Components
/State
/Reconciliation
/Rendering
/Hosting

/Csxaml.Demo
/Components
/Models
/Views

If a folder starts feeling vague, rename it to reflect actual responsibility.

---

## File Organization Rules

### Generator side

Do not put all compiler logic in one file.

Minimum separation expected:

- `Program.cs`  
  CLI entry only

- `GeneratorRunner.cs`  
  coordinates high-level generation flow only

- `Tokenizer.cs`  
  converts text to tokens only

- `Token.cs`  
  token definitions only

- `Parser.cs`  
  syntax parsing only

- `ParserContext.cs`  
  parser cursor/state only

- `Ast` folder  
  AST node files, split by concept if needed

- `Validator.cs`  
  semantic checks only

- `CodeEmitter.cs`  
  emits C# only

- `OutputWriter.cs`  
  writes generated files only

- `Diagnostic.cs` / `DiagnosticBag.cs`  
  diagnostics only

Do not merge these because it feels faster.

### Runtime side

Do not put all runtime types in one file.

Minimum separation expected:

- node types in separate files
- component instance base in its own file
- state container in its own file
- coordinator/reconciler in its own file
- WinUI renderer in its own file
- host bridge in its own file

If reconciliation starts growing, split it further:

- instance matching
- key resolution
- subtree update flow

---

## Function Size Rules

Keep methods short and obvious.

Guidelines:

- ideal: under 30 lines
- warning: over 50 lines
- hard stop: over 75 lines unless the method is truly linear and very clear

If a method exceeds the warning threshold, split it into named helpers.

Names should explain intent, not mechanics.

Prefer:

- `ParseComponentDeclaration`
- `ParseReturnBlock`
- `EmitPropsRecord`
- `MatchExistingChildInstance`

Avoid:

- `HandleStuff`
- `ProcessNode`
- `DoParse`
- `BuildEverything`

---

## Naming Rules

Names must be boring, literal, and predictable.

Prefer:

- `ComponentNode`
- `NativeNode`
- `ParseIfBlock`
- `EmitComponentClass`
- `ReconcileChildrenByKey`

Avoid:

- cute names
- vague names
- overloaded names used in multiple layers
- abbreviations unless universally obvious

Do not use "manager", "helper", "utils", or "misc" unless there is no better name.

---

## Commenting Rules

Code should be understandable mostly from structure and names. Comments should explain intent or tricky invariants, not narrate obvious code.

Good comments:

- explain why a rule exists
- explain a subtle invariant
- explain a tradeoff or temporary limitation

Bad comments:

- restating the next line
- large banner comments compensating for unclear structure
- outdated comments

If code needs a long explanation, first consider whether it should be split.

---

## Complexity Rules

Prefer explicit duplication over premature abstraction.

If two pieces of code are similar but their future may diverge, do not force them into a generic abstraction yet.

Only extract abstractions when:

1. duplication is real
2. behavior is stable
3. the abstraction makes code easier to understand

Do not build a "framework" during the milestone unless the milestone explicitly requires it.

---

## Parsing and AST Rules

Keep syntax, AST, and emission decoupled.

### Parser rules

- parser should produce AST, not emitted code
- parser should not know about WinUI
- parser should not write files
- parser should not perform codegen decisions beyond syntax structure

### AST rules

- AST types should be dumb data structures
- AST types should not contain parser logic
- AST types should not contain emission logic

### Emission rules

- emitter consumes AST
- emitter should not re-parse source text
- emitter should be deterministic
- emitter output should be boring and stable

---

## Runtime Rules

The runtime must remain understandable.

### State

- state belongs to component instances
- state invalidation path must be easy to trace
- no hidden global mutation

### Reconciliation

- start with the smallest algorithm that satisfies the milestone
- document matching rules clearly
- preserve identity only where explicitly required
- do not add speculative optimization

### Rendering

- keep logical tree and WinUI projection conceptually separate
- if native subtree rebuild is used, make that explicit
- do not hide expensive behavior

---

## Testing Rules

Never add or change behavior without tests.

Minimum expected tests for meaningful logic:

### Generator

- tokenizer tests
- parser tests
- diagnostic tests
- emitter snapshot/golden tests

### Runtime

- state invalidation tests
- reconciliation identity tests
- keyed child preservation tests
- conditional render behavior tests
- repeated child ordering tests

### Regression rule

When fixing a bug:

1. write a failing test first
2. fix the bug
3. keep the test

---

## Roadmap Discipline

Keep [ROADMAP.md](/c:/Users/DanielGary/source/repos/csxaml/ROADMAP.md) current as work lands.

Rules:

- When a meaningful change lands, update the relevant milestone status or checklist in `ROADMAP.md` in the same change.
- If work completes a milestone, mark it complete only when the milestone exit criteria are actually met.
- If work starts a milestone but does not finish it, mark it `in progress` or add a dated note describing what changed.
- If implementation uncovers a blocker, scope change, or architectural risk, record it in `ROADMAP.md` rather than leaving it implicit.
- Do not silently mark roadmap items complete just because code exists; reflect real project status, including known gaps.

---

## PR / Change Discipline

For every non-trivial change, the agent should ask:

1. Did I put too much in one file?
2. Did I mix layers?
3. Is any type doing more than one job?
4. Can a new reader find the entry point quickly?
5. Can a bug be isolated to one small area?
6. Did I add tests for the behavior?
7. Did I update `ROADMAP.md` if this change materially moved milestone status, scope, or known risks?

If the answer to any of these is "no", restructure before moving on.

---

## Stop Conditions

Stop and refactor before continuing if any of these happen:

- a file exceeds 300 lines
- a method exceeds 50 lines and is hard to scan
- a class has multiple unrelated responsibilities
- parser, diagnostics, and emitter logic are mixed together
- runtime tree logic and WinUI rendering are mixed together
- a change requires scrolling through a huge file to understand
- a bug fix requires editing unrelated code in multiple places

Do not push through these warnings. Refactor first.

---

## Preferred Style of Progress

Build in narrow vertical slices, but keep the internal structure clean from the beginning.

Preferred order:

1. add a small feature
2. keep files split by responsibility
3. add tests
4. refactor immediately if structure gets muddy
5. then move to the next feature

Do not "get it working in one file first" unless the code will immediately be split in the same change.

---

## What to optimize for

Optimize for the future maintainer who opens this code six months from now and needs to understand it quickly.

That means:

- short files
- predictable names
- shallow call stacks
- explicit data flow
- clean project boundaries
- tests near the behavior they protect

If there is a choice between "slightly less code" and "much easier to understand", choose easier to understand.

---

## Final Rule

A working implementation that is hard to read is not done.

The implementation is only acceptable when:

- the code works
- the file structure makes sense
- responsibilities are obvious
- tests exist
- a new engineer can navigate the system without guessing
