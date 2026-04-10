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

## PR / Change Discipline

For every non-trivial change, the agent should ask:

1. Did I put too much in one file?
2. Did I mix layers?
3. Is any type doing more than one job?
4. Can a new reader find the entry point quickly?
5. Can a bug be isolated to one small area?
6. Did I add tests for the behavior?

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
