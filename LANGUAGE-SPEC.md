# CSXAML Language Specification

## Status

This document defines the draft v1 language shape for CSXAML.

It is intended to do three things at once:

- describe the language that exists today in the prototype
- define the compatibility and parsing rules that future work should preserve
- give the project a clear developer-experience target for a modern C# + XAML + WinUI authoring model

Normative language in this document uses `MUST`, `SHOULD`, and `MAY`.

Implementation notes call out places where the current prototype only implements a subset of the intended v1 surface.

---

## 1. What CSXAML Is

CSXAML is a source-generated UI language for WinUI applications.

It is not:

- a replacement for C#
- a replacement for WinUI
- an XML dialect that tries to preserve every XAML rule
- a string-based template language

It is:

- a component language that lowers to ordinary generated C#
- a XAML-shaped tree syntax with C# expressions embedded directly where values and behavior are needed
- a strongly typed way to build WinUI views without splitting structure and logic across markup and code-behind

The design goal is that a C# and XAML developer should look at a `.csxaml` file and immediately understand the mental model:

- structure reads like XAML
- behavior reads like C#
- types are real C# types
- events are real delegates
- diagnostics point back to the source file

---

## 2. Design Goals

CSXAML v1 should optimize for the following.

### 2.1 C# Compatibility

- C# remains the authority for types, expressions, lambdas, object creation, method calls, generics, and delegate shapes.
- CSXAML MUST not invent a second expression language for ordinary UI logic.
- If a developer already knows how to write a C# expression, they should usually be able to paste that expression into CSXAML with minimal or no adaptation.

### 2.2 XAML Familiarity

- Tree structure MUST feel natural to a XAML author.
- Native WinUI controls, properties, and events SHOULD retain recognizable names.
- Layout, nesting, and attribute-based configuration SHOULD feel like a modernized XAML authoring model rather than a React clone in disguise.

### 2.3 Parseability

- The language MUST remain easy to parse with deterministic, context-aware parsing.
- New features SHOULD introduce clear delimiters and avoid ambiguous bare syntax.
- The parser SHOULD be able to treat embedded C# as bounded islands rather than fully reimplementing Roslyn.

### 2.4 Developer Experience

- Component parameters MUST be strongly typed.
- Native control attributes MUST validate against metadata rather than fail only at runtime.
- Generated code MUST be boring, stable, and deterministic.
- Errors SHOULD point to `.csxaml` spans rather than forcing the developer into generated code.
- The language SHOULD scale toward editor completion, diagnostics, navigation, formatting, and source mapping.

### 2.5 WinUI Alignment

- The native control model MUST map cleanly onto WinUI controls and properties.
- Runtime property application SHOULD use explicit metadata and adapter logic, not runtime reflection in hot paths.
- The language SHOULD feel like a first-class way to build WinUI apps, not a toy syntax layered on top of it.

---

## 3. Relationship to C# and XAML

CSXAML is deliberately hybrid.

| Concern | CSXAML choice | Compatibility intent |
| --- | --- | --- |
| Types | C# type names | Reuse the existing .NET type system |
| Expressions | C# expressions | Avoid a custom binding DSL |
| UI tree structure | XAML-like tags and attributes | Preserve familiar visual authoring |
| Native control vocabulary | WinUI control/property/event metadata | Stay aligned with the platform |
| Component inputs | Named props with strong typing | Combine XAML readability with C# safety |
| Event handlers | C# delegates and lambdas | Avoid string handler names |
| Build pipeline | Source generation to ordinary C# | Keep normal .NET compilation and debugging |

Important deliberate differences from classic XAML:

- attribute expressions use `{ ... }` for C# expressions, not XAML markup extensions
- string literals are quoted explicitly
- event values are delegates or lambdas, not handler-name strings
- state is explicit C# state, not a separate binding object model
- code-behind is not the primary authoring model

Important deliberate differences from handwritten C# UI trees:

- layout structure is concise and visual
- control metadata enables early validation
- component props read like markup attributes rather than constructor argument soup

---

## 4. Compilation Model

A CSXAML build SHOULD follow this conceptual pipeline:

1. Read a `.csxaml` file as source text.
2. Tokenize the file using a lightweight CSXAML tokenizer.
3. Parse the file into a CSXAML syntax tree.
4. Validate markup against:
   - component definitions
   - native control metadata
   - reserved framework rules
5. Emit deterministic generated C#.
6. Compile the generated C# with the normal C# compiler.
7. At runtime, map the logical tree to WinUI controls through explicit adapters.

This design keeps responsibilities clean:

- CSXAML owns structure and metadata-driven validation
- C# owns expression semantics and type checking
- WinUI owns final control behavior

---

## 5. Source File Model

### 5.1 File Extension

CSXAML source files use the `.csxaml` extension.

### 5.2 Top-Level Shape

In the current prototype, each `.csxaml` file contains exactly one component declaration.

This one-component-per-file model is a good default for readability, build determinism, and tooling.

### 5.3 Component Naming

Component names SHOULD use PascalCase.

For developer experience, the file name SHOULD normally match the component name, for example:

- `TodoCard.csxaml` -> `TodoCard`
- `TodoBoard.csxaml` -> `TodoBoard`

### 5.4 Namespace and Using Context

CSXAML relies on the host C# compilation context for resolving:

- types referenced in component parameters
- types referenced in `State<T>`
- types referenced inside C# expression islands

The current prototype keeps namespace/import handling simple and relies on the generated C# environment.

v1 SHOULD support an explicit, unsurprising file-level namespace/import story that aligns with ordinary C# rather than inventing a parallel mechanism.

---

## 6. Lexical Structure

### 6.1 Encoding and Line Endings

- UTF-8 SHOULD be the canonical source encoding.
- Implementations SHOULD accept normal Windows and Unix line endings.

### 6.2 Whitespace

Whitespace is generally insignificant except:

- inside string literals
- inside C# expression islands
- wherever ordinary C# syntax requires it

### 6.3 Identifiers

Identifiers SHOULD follow C# identifier rules.

This applies to:

- component names
- parameter names
- state field names
- native tag names
- component tag names
- attribute names

### 6.4 Keywords

The current core language reserves at least these keywords:

- `component`
- `Element`
- `State`
- `return`
- `if`
- `foreach`
- `var`
- `in`

Additional keywords SHOULD be introduced sparingly and only when they clearly improve readability or parseability.

### 6.5 String Literals

Attribute string literals use double quotes:

```xml
<TextBlock Text="Hello" />
```

Outside C# expression islands, CSXAML SHOULD keep string literal syntax intentionally small and predictable.

If a richer C# string form is needed, it SHOULD be written inside an expression island:

```xml
<TextBlock Text={$"Count: {count}"} />
```

### 6.6 Comments

For long-term developer experience, CSXAML SHOULD support `//` and `/* ... */` comments wherever whitespace is allowed.

The current prototype only fully relies on C#-style comment behavior inside C# islands and does not yet treat full-file comments as a finished part of the surface.

That limitation is implementation debt, not a desired design trait.

---

## 7. Grammar Overview

The following grammar is intentionally simplified and uses descriptive non-terminals for embedded C# fragments.

```ebnf
csxaml-file           ::= component-declaration EOF

component-declaration ::= "component" "Element" identifier parameter-list? component-body

parameter-list        ::= "(" parameter ("," parameter)* ")"
parameter             ::= csharp-type identifier

component-body        ::= "{"
                           state-field*
                           "return" markup-node ";"
                         "}"

state-field           ::= "State" "<" csharp-type ">" identifier
                          "=" "new" "State" "<" csharp-type ">"
                          "(" csharp-island ")"
                          ";"

markup-node           ::= element-node

element-node          ::= "<" tag-name attribute* "/>"
                        | "<" tag-name attribute* ">" child-node* "</" tag-name ">"

child-node            ::= markup-node
                        | if-block
                        | foreach-block

if-block              ::= "if" "(" csharp-island ")" "{" child-node* "}"

foreach-block         ::= "foreach" "(" "var" identifier "in" csharp-island ")" "{" child-node* "}"

attribute             ::= attribute-name "=" attribute-value
attribute-value       ::= string-literal | expression-island
expression-island     ::= "{" csharp-island "}"
```

Where:

- `csharp-type` is C# type syntax
- `csharp-island` is opaque C# source text captured by balanced-delimiter scanning
- `tag-name` and `attribute-name` are currently plain identifiers, but qualified forms are intentionally reserved for future XAML compatibility

---

## 8. Parsing Strategy

Parseability is a first-class design concern, not an afterthought.

### 8.1 Context-Sensitive but Bounded

CSXAML uses a context-sensitive parser with a small token vocabulary:

- identifiers
- string literals
- symbols
- end of file

The parser does not attempt to fully parse all embedded C# syntax.

Instead, it treats C# as bounded islands in places where the grammar already knows C# is expected:

- component parameter type names
- `State<T>` type names
- `State<T>(...)` initializer bodies
- attribute expression values inside `{ ... }`
- `if (...)` conditions
- `foreach (var item in ...)` collection expressions

### 8.2 Balanced Island Scanning

When scanning a C# island, the parser MUST:

- respect nested `()`, `[]`, and `{}` delimiters
- ignore delimiters inside quoted literals
- stop only when the matching outer delimiter is reached

This approach gives the language several benefits:

- most ordinary C# expressions can pass through unchanged
- the CSXAML parser stays small and understandable
- Roslyn remains the ultimate authority on C# expression correctness

### 8.3 Reserved Future Surface

To preserve compatibility with XAML-like expansion, the grammar SHOULD reserve these forms even if the prototype does not yet implement them fully:

- `Prefix:Control` tag names for external control namespaces
- `Owner.Property` attribute names for attached properties

This is important because future XAML interop should feel additive rather than syntactically disruptive.

### 8.4 Syntax Admission Rule

Future syntax SHOULD meet all of the following before being accepted:

- it begins with an unambiguous leading token
- it does not require arbitrary reinterpretation of C# islands
- it does not turn attribute values into context-free guesswork
- it makes editor completion and diagnostics easier, not harder

---

## 9. Component Declarations

### 9.1 Core Form

A component declaration has this shape:

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    return <StackPanel>
        <TextBlock Text={Title} />
        <Button Content="Toggle" OnClick={OnToggle} />
    </StackPanel>;
}
```

### 9.2 `component Element`

`component` introduces a CSXAML component declaration.

`Element` identifies the component kind currently supported by the language: a renderable UI component that returns a markup tree.

Future component kinds MAY be added, but they SHOULD not destabilize the core component grammar.

### 9.3 Parameters

Component parameters:

- are declared using C# type syntax and identifier names
- define the public prop surface of the component
- MUST remain strongly typed
- SHOULD be generated as a stable props record or equivalent structure in emitted C#

CSXAML MUST not degrade component props into weak dynamic bags.

### 9.4 Required Return

Each component body currently contains exactly one `return` of a markup node.

This restriction keeps the core grammar simple and easy to reason about.

### 9.5 Duplicate Names

Duplicate parameter names and duplicate state field names are invalid.

Diagnostics SHOULD point to the duplicate declaration span in the `.csxaml` file.

---

## 10. State Declarations

### 10.1 Syntax

State declarations use explicit C#-typed state:

```csharp
State<int> Count = new State<int>(0);
State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(CreateSeedItems());
```

### 10.2 Semantics

A state field:

- belongs to the component instance
- has an explicit generic type argument
- has an initializer expressed in C# syntax
- invalidates the component when its value changes

### 10.3 Type Matching

The type argument in the field declaration and the constructor call MUST refer to the same state type.

This rule keeps the syntax boring and predictable:

```csharp
State<int> Count = new State<int>(0);      // valid
State<int> Count = new State<long>(0);     // invalid
```

### 10.4 Data Flow Preference

For predictable UI behavior, CSXAML SHOULD prefer explicit state updates over magical binding rules.

That means this style is preferred:

```csharp
OnToggle={() => Items.Value = Items.Value.Select(...).ToList()}
```

over hidden mutation models that make rerender causes hard to trace.

---

## 11. Markup Model

### 11.1 A Single Tree

A component returns exactly one root markup node.

That node can contain children through:

- nested elements
- `if` blocks
- `foreach` blocks

### 11.2 No Free Text Nodes in Core v1

The current core language does not define free text nodes between tags as a separate syntax form.

All visible text is expressed explicitly through controls such as:

- `TextBlock Text="Hello"`
- `Button Content="Save"`

This keeps parsing straightforward and keeps WinUI mapping explicit.

### 11.3 Native vs Component Elements

Markup tags come in two categories:

- native elements backed by WinUI metadata, such as `Button`
- component elements backed by CSXAML component definitions, such as `TodoCard`

Resolution SHOULD be exact and deterministic.

---

## 12. Element Syntax

### 12.1 Self-Closing Elements

Self-closing syntax is allowed:

```xml
<Button Content="Save" OnClick={Save} />
```

### 12.2 Container Elements

Elements with children use open and close tags:

```xml
<Border Background={Theme.CardBrush}>
    <TextBlock Text="Hello" />
</Border>
```

### 12.3 Closing Tags

Closing tags MUST match the opening tag name exactly.

Tag matching is case-sensitive.

### 12.4 Child Content Rules

Whether a native element may have children is determined by control metadata.

Examples:

- `TextBlock` does not support child elements
- `Border` supports a single child
- `StackPanel` supports multiple children

Component elements do not currently support child content in the core language.

Future slot support may extend this, but it should do so explicitly rather than implicitly overloading current behavior.

---

## 13. Attributes and Values

### 13.1 Two Value Forms

Attribute values intentionally have only two surface forms:

- string literal: `"Hello"`
- C# expression: `{Title}`, `{18}`, `{IsDone ? DoneBrush : NotDoneBrush}`

There are no bare unquoted values like:

```xml
<TextBlock Text=Hello />
```

This restriction is deliberate. It improves parseability, formatting consistency, and error reporting.

### 13.2 String Literals

String literals are best for obvious string values:

```xml
<TextBlock Text="Done" />
<Button Content="Toggle" />
```

### 13.3 Expression Values

Expression values are required whenever the value is not a simple string literal or when the metadata requires a typed value:

```xml
<TextBlock FontSize={18} />
<StackPanel Orientation={global::Microsoft.UI.Xaml.Controls.Orientation.Horizontal} />
<Button OnClick={() => Count.Value++} />
```

### 13.4 Duplicate Attributes

Duplicate attributes on the same element are invalid.

Examples:

```xml
<Button Content="A" Content="B" />
<TodoCard Title="A" Title="B" />
```

### 13.5 Reserved Attributes

`Key` is a reserved framework attribute.

It is allowed on native and component elements, but it is not part of the native WinUI property bag.

`Key` exists to drive reconciliation identity.

---

## 14. C# Expression Islands

### 14.1 Principle

Expressions inside CSXAML SHOULD be ordinary C#.

CSXAML SHOULD not make developers learn a second mini-language for:

- conditions
- lambdas
- property access
- method calls
- object creation
- collection transforms

### 14.2 Valid Expression Contexts

C# expression islands currently appear in:

- attribute values: `Text={Title}`
- event values: `OnClick={() => Count.Value++}`
- conditions: `if (IsDone) { ... }`
- loop collections: `foreach (var item in Items.Value) { ... }`
- state initializers: `new State<List<T>>(CreateItems())`

### 14.3 Semantic Authority

The CSXAML parser captures C# islands as text.

The C# compiler remains the authority on:

- type correctness
- overload resolution
- lambda binding
- generic inference
- member resolution

This is a major compatibility advantage. CSXAML does not need to reproduce Roslyn semantics to feel natural.

### 14.4 Modern C# Compatibility Goal

The v1 parser SHOULD robustly tolerate modern C# expression forms inside islands, including:

- lambdas
- ternaries
- generics
- collection expressions
- object creation
- member access chains
- string interpolation

The parser quality bar SHOULD be high enough that embedded C# usually feels copy-paste safe.

---

## 15. Control Flow in Markup

### 15.1 `if`

Conditional rendering uses C# conditions:

```csharp
if (IsDone) {
    <TextBlock Text="Done" />
}
```

The body yields zero or more child nodes.

### 15.2 `foreach`

Repeated rendering uses a constrained C#-shaped loop:

```csharp
foreach (var item in Items.Value) {
    <TodoCard Key={item.Id} Title={item.Title} IsDone={item.IsDone} OnToggle={...} />
}
```

The current core form intentionally restricts `foreach` to:

- `var` iteration variable
- a single `in` collection expression
- a child-content block

This restriction preserves readability and simplifies parsing.

### 15.3 Why No Arbitrary C# Statements

Inside markup child positions, the language currently allows:

- markup nodes
- `if` blocks
- `foreach` blocks

It does not allow arbitrary statements such as:

- `switch`
- local variable declarations
- `while`
- `for`

Those can be added later if they genuinely improve the model, but they should not arrive at the cost of turning markup parsing into general C# parsing.

---

## 16. Native Element Semantics

### 16.1 Metadata-Driven Surface

Native elements are validated against generated control metadata.

Metadata answers questions such as:

- is this tag a known native control?
- what properties are exposed?
- what events are exposed?
- what child-content shape is allowed?
- what value-kind hint should apply?

### 16.2 Native Properties and Native Events Are Distinct

A native event is not just another property.

CSXAML MUST treat them separately in:

- metadata
- validation
- emission
- runtime binding

### 16.3 Event Naming

CSXAML uses normalized event names such as:

- `OnClick`
- future `OnTextChanged`
- future `OnCheckedChanged`

This keeps the authoring model consistent and easy to discover.

### 16.4 Value-Kind Hints

Metadata carries conversion hints such as:

- `String`
- `Bool`
- `Int`
- `Double`
- `Enum`
- `Object`
- `Brush`
- `Thickness`
- `Unknown`

These hints exist to support:

- validation
- code generation
- runtime coercion where appropriate
- future tooling and completion

### 16.5 Literal Rules for Native Props

The language SHOULD prefer this rule:

- string literals are only directly valid for properties where a string literal is obviously safe
- typed values use C# expressions

Examples:

```xml
<TextBlock Text="Done" />
<TextBlock FontSize={18} />
<StackPanel Spacing={12} />
<StackPanel Orientation={global::Microsoft.UI.Xaml.Controls.Orientation.Horizontal} />
```

This rule is good for both parseability and developer experience because it makes type intent visible at the source level.

### 16.6 Runtime Application Model

Runtime native property application SHOULD use:

- generated metadata
- explicit adapter logic
- explicit conversion helpers

Runtime native property application SHOULD NOT use reflection as the primary hot-path mechanism.

---

## 17. Component Element Semantics

### 17.1 Named Props

Component usage reads like markup but maps to strong typed props:

```xml
<TodoCard
    Key={item.Id}
    Title={item.Title}
    IsDone={item.IsDone}
    OnToggle={() => Toggle(item.Id)} />
```

### 17.2 Validation Rules

For component elements:

- unknown props are invalid
- duplicate props are invalid
- missing required props are invalid
- children are currently invalid

### 17.3 Type Safety

Type safety SHOULD be enforced through generated C# props structures and the ordinary C# compiler.

The ideal developer experience is:

- author sees named markup props
- generator emits a strong typed props record or equivalent
- ordinary C# type checking does the rest

This gives the language XAML readability without sacrificing C# guarantees.

---

## 18. Keys and Identity

`Key` is a framework-level identity hint used during reconciliation.

### 18.1 Rules

- `Key` MAY be used on native or component elements
- `Key` MUST not be forwarded as a normal native property
- `Key` SHOULD be stable across rerenders
- repeated renders SHOULD supply keys whenever identity matters

### 18.2 Why It Exists

Keys let the runtime preserve intended identity across:

- repeated rendering
- reorder scenarios
- conditional subtree changes

That is critical for predictable interactive behavior and future retained reconciliation work.

---

## 19. Diagnostics and Tooling Expectations

CSXAML should feel like a good toolchain citizen.

### 19.1 Source Diagnostics

Diagnostics SHOULD point to `.csxaml` spans for:

- parse failures
- unknown native tags
- unknown native props
- unknown native events
- invalid component props
- duplicate attributes
- child-content violations

### 19.2 Tooling Surface

The language design SHOULD naturally support:

- tag completion
- native prop and event completion
- component prop completion
- source squiggles
- go-to-definition
- generated/source mapping

This is one reason metadata-driven validation is so important. It helps the editor speak the same language as the compiler and runtime.

### 19.3 Generated Code Philosophy

Generated C# SHOULD be:

- deterministic
- readable
- debuggable when necessary
- boring

Developers should not need to live in generated code, but when they do open it, it should make sense.

---

## 20. Compatibility Commitments

### 20.1 C# Compatibility Commitments

Future versions of CSXAML SHOULD preserve these principles:

- embedded expressions stay ordinary C#
- types stay ordinary C# type names
- delegates stay ordinary C# delegates
- source generation lowers to normal C# rather than a custom runtime VM

### 20.2 XAML Compatibility Commitments

Future versions of CSXAML SHOULD preserve these principles:

- UI trees stay tag-based and attribute-based
- native WinUI surface names remain recognizable
- control/property/event metadata follows WinUI rather than inventing unrelated names
- future external control and attached-property syntax should feel familiar to XAML authors

### 20.3 Deliberate Non-Compatibility

CSXAML deliberately does not aim to preserve every XAML mechanism.

In particular, it does not center the v1 design around:

- markup extensions as the primary expression model
- string handler names
- binding-path mini-languages as the main data model
- heavy reliance on code-behind ceremony

This is intentional. The project is trying to produce a more modern C#-first authoring experience, not a byte-for-byte clone of legacy XAML.

---

## 21. Developer Experience Guidelines

The language should feel pleasant in everyday use.

### 21.1 Mental Model

The recommended mental model is:

- write structure in tags
- write values in C#
- write behavior in lambdas or method calls
- write reusable inputs as typed component props
- keep component files small and obvious

### 21.2 Authoring Conventions

The following conventions are strongly recommended:

- PascalCase for component names and native tags
- PascalCase for native prop and event names
- camelCase for local/state parameter names when they are true variables
- one component per file
- small components with explicit props
- helper logic in ordinary C# types rather than giant inline expressions when expressions become hard to read

### 21.3 Why This Feels Modern

CSXAML feels modern when it lets a developer:

- stay in one file
- keep real C# logic
- avoid code-behind ceremony
- avoid stringly typed bindings
- preserve WinUI familiarity
- get metadata-driven validation before runtime

That combination is the core product idea.

---

## 22. Current Prototype Coverage

The current prototype already implements a meaningful subset of this spec.

Implemented today:

- one component per `.csxaml` file
- typed component parameters
- explicit `State<T>`
- `return` of a markup root
- native and component tags
- string literal and `{...}` expression attribute values
- `if` child blocks
- constrained `foreach` child blocks
- metadata-driven native property and event validation
- generated generic native prop and event bags
- runtime adapters for `Border`, `Button`, `StackPanel`, and `TextBlock`
- `Key`-driven identity for repeated rendering

Known gaps relative to the intended v1 experience:

- explicit file-level namespace/import syntax is not yet a finished surface
- whole-file comment support needs hardening
- external control namespace syntax is not yet implemented
- attached property syntax is not yet implemented
- component child-content slots are not yet implemented
- richer source mapping and editor tooling are still ahead
- project-system and design-time behavior need more maturation

---

## 23. Example

This is representative of the language direction the project is aiming for:

```csharp
component Element TodoBoard {
    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(
        new List<TodoItemModel>
        {
            new TodoItemModel("todo-1", "Draft plan", false),
            new TodoItemModel("todo-2", "Wire runtime", true),
            new TodoItemModel("todo-3", "Write tests", false)
        });

    return <StackPanel Spacing={12}>
        <TextBlock Text="Todo Board" FontSize={24} />
        foreach (var item in Items.Value) {
            <TodoCard
                Key={item.Id}
                Title={item.Title}
                IsDone={item.IsDone}
                OnToggle={() => Items.Value = Items.Value.Select(todo =>
                    todo.Id == item.Id ? todo with { IsDone = !todo.IsDone } : todo).ToList()} />
        }
    </StackPanel>;
}
```

This example captures the intended balance:

- component structure is visual
- state is explicit
- types are real
- logic is plain C#
- markup remains concise

---

## 24. Final Constraint

If a future version of CSXAML becomes harder to parse, harder to explain to a C# + XAML developer, or more surprising to diagnose, that should be treated as a regression in language quality even if the feature technically works.

The language is only successful if it remains:

- compatible with how C# developers think
- familiar to how XAML developers structure UI
- straightforward to parse and validate
- pleasant to use in a real WinUI codebase
