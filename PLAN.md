# Milestone 2 Plan - React-like Component Model for C#/XAML

This milestone should prove that CSXAML is more than file-to-code generation. It needs to prove the beginnings of a real component model: props, composition, conditional rendering, repeated rendering, and identity preservation.

Milestone 1 proved the build pipeline, runtime element tree, WinUI rendering, and state-triggered rerendering. Milestone 2 should build directly on that foundation rather than widening into lots of new controls.

---

## Milestone 2 objective

Implement the smallest credible React-like component model for C#/XAML platforms with support for:

- typed props
- nested component composition
- conditional rendering
- repeated child rendering from collections
- explicit keys for repeated children
- preservation of child component instance identity across rerenders

This milestone is complete when a small app can be written fully in CSXAML using parent and child components, and child-local state is preserved when key and type are preserved.

---

## Target demo

Build a small **Todo Board** demo.

### `TodoBoard`

Owns a collection of todo items in state and renders a list of `TodoCard` children.

### `TodoCard`

Receives props, renders title, conditionally shows a done badge, and exposes a toggle button.

### Required demo behavior

- parent renders repeated child components
- child gets typed props
- child can render conditionally
- repeated children use explicit keys
- toggling an item rerenders correctly
- if child local state is added, it survives rerenders when identity is preserved

Do not pick a broader demo. This one forces the right semantics.

---

## Scope rules

The agent must keep Milestone 2 narrow.

### In scope

- typed component parameters
- child component usage in markup
- `if (...) { ... }` conditional blocks
- constrained `foreach (var x in y) { ... }` blocks
- explicit `Key={...}` support
- component identity preservation at component boundaries

### Out of scope

- general WinUI parity
- styling system
- slots / child content
- fragments
- `else`
- hooks/effects
- context/provider model
- async rendering
- refs
- Roslyn source generators
- design-time tooling
- generalized diff engine for all native controls

---

## Architectural direction

Milestone 1 likely treated everything as a native element tree and rebuilt subtrees eagerly. That is acceptable for the counter POC. It is not enough for a React-like model.

Milestone 2 needs a logical runtime tree with a distinction between:

- **native nodes** that map to WinUI controls
- **component nodes** that represent CSXAML child component instances

The runtime must start preserving component instance identity, even if native UI inside those boundaries is still rebuilt.

That is the right compromise here. Do not attempt full retained native-control diffing yet.

---

## Key design decisions

### 1. Use typed props, not dictionaries

Each component with parameters should generate a typed props record or equivalent strongly typed structure.

Example target shape:

```csharp
public sealed record TodoCardProps(
    string Title,
    bool IsDone,
    Action OnToggle
);
```

Avoid string-keyed property bags.

### 2. Distinguish native tags from component tags

The parser/generator must treat native elements and component references differently.

Examples:

- `StackPanel`, `TextBlock`, `Button` -> native nodes
- `TodoCard` -> component node

### 3. Preserve component identity by type + key

For repeated child components:

- if key matches and type matches, preserve instance
- otherwise create a new instance

For non-repeated fixed children:

- position-based matching is acceptable for this milestone

### 4. Rebuild native subtrees within a component if needed

Do not try to optimize native projection too early. Preserve identity at component boundaries first.

---

# Milestone 2 implementation phases

## Phase 1 - Upgrade runtime model

### Goal

Move from a flat native element tree to a logical tree that can represent child components.

### Required work

Introduce or replace the existing `Element` model with a `Node` model.

Suggested conceptual types:

- `Node`
- `NativeNode`
- `ComponentNode`
- `StackPanelNode`
- `TextBlockNode`
- `ButtonNode`

`ComponentNode` should represent a child component reference plus props and optional key.

Example conceptual shape:

```csharp
public sealed record ComponentNode(
    Type ComponentType,
    object Props,
    string? Key
) : Node;
```

Also introduce a runtime coordinator that can:

- mount the root component
- manage a logical component instance tree
- rerender a component subtree when invalidated
- preserve child component instances by type/key or type/position

### Deliverables

- logical node model
- component node type
- root render coordinator
- updated component base abstraction that can participate in component-instance management

### Acceptance criteria

- root component can render a child component node
- runtime can instantiate and render a child component
- rerender still works through state invalidation

---

## Phase 2 - Add typed props support

### Goal

Allow components to declare typed parameters and receive them at render time.

### Required syntax support

Component declaration with typed parameter list.

Example target:

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    return <StackPanel>
        <TextBlock Text={Title} />
        <Button Content="Toggle" OnClick={OnToggle} />
    </StackPanel>;
}
```

### Generator changes

The generator must:

- parse component parameter list
- emit a typed props record or equivalent
- emit component class that stores/uses typed props
- lower parent-provided props into the generated props structure

### Deliverables

- parser support for component parameters
- AST support for parameters
- emitter support for generated props type
- runtime support for passing props into child instances

### Acceptance criteria

- a child component can receive and use typed props
- generated code compiles cleanly
- no weakly typed prop bag is used

---

## Phase 3 - Add nested component composition

### Goal

Allow components to render other components.

### Required syntax support

Parent markup can include child component usage:

```csharp
<TodoCard Title={item.Title} IsDone={item.IsDone} OnToggle={() => Toggle(item.Id)} />
```

### Generator changes

The generator must:

- recognize component tags vs native tags
- lower child component usage to `ComponentNode`
- validate required props are present

### Runtime changes

The runtime must:

- instantiate child component instances from `ComponentNode`
- assign props
- render child output
- preserve child identity where appropriate

### Deliverables

- parser support for child component tags
- component node emission
- runtime child-component instantiation and reuse

### Acceptance criteria

- parent component renders child component
- child receives props and renders correctly
- rerender updates flow through parent and child correctly

---

## Phase 4 - Add conditional rendering

### Goal

Support simple conditional UI.

### Required syntax support

```csharp
if (IsDone) {
    <TextBlock Text="Done" />
}
```

### Generator strategy

Do not introduce a complicated runtime conditional abstraction unless necessary. Lower conditionals into generated node inclusion/exclusion.

The simplest acceptable lowering is:

- include child node when condition true
- omit child node when false

### Deliverables

- parser support for `if (...) { ... }`
- AST representation for conditional block
- emitter support for conditional child inclusion

### Acceptance criteria

- done badge appears only when `IsDone` is true
- toggling state updates conditional output correctly

---

## Phase 5 - Add repeated rendering with keys

### Goal

Support list rendering and identity preservation.

### Required syntax support

```csharp
foreach (var item in Items.Value) {
    <TodoCard
        Key={item.Id.ToString()}
        Title={item.Title}
        IsDone={item.IsDone}
        OnToggle={() => Toggle(item.Id)} />
}
```

### Generator changes

The generator must:

- parse constrained `foreach`
- lower repeated blocks into node lists
- capture `Key={...}` when present

### Runtime changes

The runtime must:

- reconcile repeated child components by key + type
- preserve matching child instances
- create/remove instances when collection changes

Native controls inside a child may still be rebuilt. That is acceptable.

### Deliverables

- parser support for `foreach`
- AST support for repeated blocks
- emitter support for list lowering
- runtime keyed child-instance preservation

### Acceptance criteria

- multiple `TodoCard` children render from a collection
- toggling one item does not scramble identity of other items
- keyed components preserve local state when still present

---

## Phase 6 - Final demo wiring

### Goal

Express the whole Todo Board demo in CSXAML.

### Required demo pieces

#### `TodoItemModel.cs`

Plain C# model with at least:

- `Id`
- `Title`
- `IsDone`

#### `TodoCard.csxaml`

Receives props and conditionally shows the done badge.

#### `TodoBoard.csxaml`

Owns list state and renders repeated `TodoCard` children.

### Example target shape

`TodoCard.csxaml`

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    return <StackPanel>
        <TextBlock Text={Title} />
        if (IsDone) {
            <TextBlock Text="Done" />
        }
        <Button Content="Toggle" OnClick={OnToggle} />
    </StackPanel>;
}
```

`TodoBoard.csxaml`

```csharp
component Element TodoBoard {
    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateSeedItems());

    return <StackPanel>
        foreach (var item in Items.Value) {
            <TodoCard
                Key={item.Id.ToString()}
                Title={item.Title}
                IsDone={item.IsDone}
                OnToggle={() => Toggle(item.Id)} />
        }
    </StackPanel>;
}
```

Exact syntax can vary slightly if needed, but the semantics must remain the same.

### Acceptance criteria

- no hand-authored child component C# is required for the demo
- demo runs from generated code
- repeated child components behave correctly

---

# Required code structure rules for the agent

Since maintainability is already a concern, the milestone plan needs explicit structure rules.

## Generator project

Keep these responsibilities separate:

- `Cli/Program.cs`
- `Cli/GeneratorRunner.cs`
- `Syntax/Token.cs`
- `Syntax/Tokenizer.cs`
- `Parsing/Parser.cs`
- `Parsing/ParserContext.cs`
- `Ast/...` split into small files
- `Validation/...`
- `Emission/...`
- `Diagnostics/...`

Do not allow tokenizer, parser, AST, diagnostics, and emitter to collapse into one or two large files.

## Runtime project

Keep these responsibilities separate:

- `Nodes/...`
- `Components/...`
- `State/...`
- `Reconciliation/...`
- `Rendering/...`
- `Hosting/...`

Do not let the runtime coordinator, reconciliation rules, WinUI rendering, and state handling merge into a single giant file.

## File-size rule

- preferred: under 200 lines
- warning: over 300
- hard stop: over 400

If a file crosses the warning threshold, split it before continuing.

---

# Testing requirements

Milestone 2 will get harder to reason about very quickly without tests.

## Generator tests

Add tests for:

- parsing component parameter lists
- parsing child component usage
- parsing `if` blocks
- parsing `foreach` blocks
- prop validation failures
- emitted-code snapshots for representative components

## Runtime tests

Add tests for:

- child component instance preservation by key
- replacement when key changes
- position-based reuse for fixed children
- conditional node appearance/disappearance
- repeated child ordering
- local child state preserved across parent rerender

## Regression rule

Any bug fix must start with a failing test.

---

# Milestone 2 stop conditions

The agent should stop and report if any of these happen:

- parser, AST, diagnostics, and emission logic are being mixed together
- runtime and WinUI rendering are being mixed together
- identity preservation rules are becoming unclear
- props are drifting toward string-keyed dynamic bags
- files are growing into large multi-responsibility files
- the implementation is starting to chase WinUI breadth instead of component semantics

If one of those occurs, refactor before continuing.

---

# Definition of done

Milestone 2 is done when all of these are true:

- parent CSXAML components can render child CSXAML components
- child components can receive typed props
- conditional rendering works
- repeated rendering works
- repeated child components support explicit keys
- child component instances are preserved when key and type are preserved
- child-local state survives parent rerenders when identity is preserved
- the Todo Board demo runs fully from generated CSXAML code
- code remains split into small, understandable files

---

# Recommended implementation order

Use this order exactly:

1. Upgrade runtime model to support `ComponentNode`
2. Add typed props support
3. Add child component composition
4. Add conditional rendering
5. Add repeated rendering with keys
6. Build Todo Board demo
7. Tighten diagnostics and tests
8. Refactor any files that became too large

That sequence keeps the work centered on React-like semantics rather than drifting into unrelated framework work.

If you want, I can turn this into a copy-paste `implementation-plan.md` with explicit file-by-file deliverables and checkpoints for the agent.
