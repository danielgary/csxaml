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
- File-level imports SHOULD prefer ordinary C# `using` forms over inventing an `xmlns`-style parallel system.

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
- App-level services SHOULD use explicit component-level `inject` declarations rather than component parameters, markup conventions, or ad hoc service-locator calls.
- Native control attributes MUST validate against metadata rather than fail only at runtime.
- `.csxaml` files SHOULD allow ordinary local helper code so authors can name calculations and behavior near the view instead of collapsing into giant inline expressions.
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
| App services | Explicit `inject` declarations | Keep services separate from props and local state |
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

In v1, a `.csxaml` file SHOULD contain exactly one component declaration plus optional file-level `using` directives, an optional file-scoped namespace, and optional file-local helper declarations.

This one-component-per-file model is a good default for readability, build determinism, and tooling.

### 5.3 Component Naming

Component names SHOULD use PascalCase.

For developer experience, the file name SHOULD normally match the component name, for example:

- `TodoCard.csxaml` -> `TodoCard`
- `TodoBoard.csxaml` -> `TodoBoard`

### 5.4 Namespace and Using Context

CSXAML relies on the host C# compilation context for resolving:

- types referenced in component parameters
- types referenced in injected-service declarations
- types referenced in `State<T>`
- types referenced inside C# expression islands
- component namespaces imported from other CSXAML libraries
- external control namespaces
- attached-property owner types

The current prototype implements file-level `using` directives, alias-qualified external and component tags, import-driven attached-property owner lookup for the current supported slice, one file-scoped `namespace` declaration per `.csxaml` file, and referenced-component discovery through an explicit generated manifest contract.

For v1, CSXAML SHOULD support an explicit, unsurprising file-level namespace/import story that aligns with ordinary C# rather than inventing a parallel mechanism.

The preferred source forms are:

```csharp
using MyApp.Shared.Components;
using CommunityToolkit.WinUI.Controls;
using Fluent = CommunityToolkit.WinUI.Controls;
using Widgets = MyCompany.Widgets;
using AutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace MyApp.Components;
```

These forms are intended to support:

- imported component tags such as `<TodoCard />` when the simple name is imported and unambiguous
- bare control tags such as `<InfoBar />` when the simple name is imported and unambiguous
- qualified tags such as `<Fluent:InfoBar />` or `<Widgets:UserAvatar />` when the prefix names a namespace alias
- attached-property owners such as `AutomationProperties.Name="Save"` when the owner type is visible by import or type alias

CSXAML SHOULD NOT invent a separate `xmlns` system or a custom `using as` syntax for this scenario.

When present, a file-scoped `namespace` SHOULD determine the namespace of the generated component and any file-local helper declarations.

When the source omits an explicit namespace, the implementation MUST fall back to a deterministic project-default component namespace rather than a hardcoded global placeholder. In the current repo implementation, that project-default namespace is provided by the shared build contract and defaults to the project's `RootNamespace`, then `AssemblyName` if `RootNamespace` is absent.

Project-level generated support files that are not author-facing component types, such as referenced-component manifest providers and generated external-control registration, SHOULD live in a deterministic internal namespace derived from the project-default component namespace rather than leaking into author-facing examples.

Ambiguous simple-name resolution MUST produce a diagnostic rather than heuristic guessing.

### 5.5 File-Local Helper Declarations

CSXAML SHOULD allow file-local helper declarations in the same file as the component.

This is the intended equivalent of helper functions and helper types living next to a React component in a `.tsx` file.

The minimum expected v1 shape is:

- component-local local variables and local functions before the final render return
- file-local helper types in the same `.csxaml` file

File-local helper declarations:

- share the file's `namespace` and `using` context
- SHOULD lower predictably to ordinary generated C#
- SHOULD stay small and focused on the component file they support
- SHOULD move to ordinary `.cs` files when they become broadly shared or substantially larger than the component itself

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
- `inject`
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
csxaml-file           ::= using-directive* namespace-declaration? file-member* EOF

file-member           ::= helper-declaration
                        | component-declaration

component-declaration ::= "component" "Element" identifier parameter-list? component-body

parameter-list        ::= "(" parameter ("," parameter)* ")"
parameter             ::= csharp-type identifier

component-body        ::= "{"
                           component-prologue-member*
                           component-helper-code*
                           "return" markup-node ";"
                         "}"

component-prologue-member ::= inject-field
                            | state-field

inject-field          ::= "inject" csharp-type identifier ";"

state-field           ::= "State" "<" csharp-type ">" identifier
                          "=" "new" "State" "<" csharp-type ">"
                          "(" csharp-island ")"
                          ";"

component-helper-code ::= csharp-helper-code

markup-node           ::= element-node

element-node          ::= "<" tag-name attribute* "/>"
                        | "<" tag-name attribute* ">" child-node* "</" tag-name ">"

child-node            ::= markup-node
                        | slot-outlet
                        | if-block
                        | foreach-block

slot-outlet           ::= "<" "Slot" attribute* "/>"
                        | "<" "Slot" attribute* ">" child-node* "</" "Slot" ">"

if-block              ::= "if" "(" csharp-island ")" "{" child-node* "}"

foreach-block         ::= "foreach" "(" "var" identifier "in" csharp-island ")" "{" child-node* "}"

attribute             ::= attribute-name "=" attribute-value
attribute-value       ::= string-literal | expression-island
expression-island     ::= "{" csharp-island "}"
```

Where:

- `using-directive` and `namespace-declaration` use ordinary C# source forms
- `helper-declaration` is file-local helper declaration text that does not contain CSXAML markup and lowers predictably to generated C#
- `csharp-type` is C# type syntax
- `csharp-helper-code` is component-local ordinary C# declarations/statements before the final markup return
- `csharp-island` is opaque C# source text captured by balanced-delimiter scanning
- `tag-name` and `attribute-name` support the current v1 forms `identifier`, `identifier ":" identifier`, and `identifier "." identifier`
- bare `Slot` is a reserved child-content outlet inside a component definition; prefixed tags such as `Widgets:Slot` remain ordinary tags

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
- injected service type names
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

For v1, the intended meaning of these forms is:

- `Prefix:Control` resolves through a file-level namespace alias introduced by ordinary C#-style `using Prefix = Some.Namespace;`
- `Owner.Property` resolves through an attached-property owner type that is visible by import or by a C#-style type alias

This is important because future XAML interop should feel additive rather than syntactically disruptive while still staying aligned with normal C# import habits.

### 8.4 Syntax Admission Rule

Future syntax SHOULD meet all of the following before being accepted:

- it begins with an unambiguous leading token
- it does not require arbitrary reinterpretation of C# islands
- it does not turn attribute values into context-free guesswork
- it makes editor completion and diagnostics easier, not harder

### 8.5 Helper Code Scanning

The parser SHOULD allow file-local helper declarations and component-local helper code without fully parsing arbitrary C# declaration syntax.

A practical v1 rule is:

- file-local helper declarations are scanned as opaque C# declarations between top-level forms
- component prologue members such as `inject` declarations and `State<T>` declarations are parsed explicitly before helper-code scanning begins
- after the last component prologue member, component-local helper code is scanned as ordinary C# until the first outermost `return` immediately followed by a markup opener `<`
- nested `return` tokens inside local functions, lambdas, or nested blocks do not terminate the component prologue
- helper code MUST not itself contain CSXAML markup fragments

This keeps the parser small while still letting component authors write named local values and helpers in the same file.

### 8.6 Name Resolution Guardrails

- simple tag names imported through `using Namespace;` may bind only when a single supported match exists
- alias-qualified tags such as `Alias:Control` MUST not fall back to broad best-effort searches
- unknown or unsupported external controls MUST fail at compile-time validation if metadata is unavailable
- import and alias rules SHOULD stay file-local and explicit enough that tooling can resolve them without whole-solution guesswork

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
- MUST NOT be used to declare injected services

CSXAML MUST not degrade component props into weak dynamic bags.

Injected services are not props. They MUST remain a separate declaration form so parent components do not pass them through markup and tooling does not surface them as component attributes.

### 9.4 Injected Services

CSXAML MAY declare required component-scoped services using explicit `inject` declarations in the component prologue.

For example:

```csharp
component Element TodoBoard {
    inject ITodoService todoService;
    inject ILogger<TodoBoard> logger;

    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(new());

    async Task LoadAsync() {
        logger.LogInformation("Loading todos");
        Items.Value = await todoService.GetTodosAsync();
    }

    return <StackPanel>
        <Button Content="Refresh" OnClick={async () => await LoadAsync()} />
    </StackPanel>;
}
```

Injected services:

- use a C# type plus identifier name
- are not component props
- MUST NOT appear in the component parameter list
- MUST NOT be supplied through markup attributes on component usage
- SHOULD use camelCase identifiers in source

### 9.4.1 Placement and Availability

`inject` declarations:

- MAY appear only in the component body prologue before ordinary helper code and before the final render return
- MAY be interleaved with `State<T>` declarations
- MUST NOT appear in markup child positions
- SHOULD be available to component helper code and C# expression islands like ordinary component members

### 9.4.2 Resolution Semantics

Each `inject` declaration resolves once per component instance from the ambient component service provider.

The runtime SHOULD build on the ordinary .NET `IServiceProvider` model rather than inventing a second container abstraction.

The host container's normal singleton, scoped, and transient behavior SHOULD remain authoritative. CSXAML SHOULD NOT add language-level lifetime syntax for v1.

Generated code SHOULD lower injected services to boring cached members such as constructor-initialized readonly fields or equivalent instance-level bindings, rather than re-resolving services during each render.

Component helper code SHOULD prefer explicit `inject` declarations over ad hoc service-locator calls such as `GetRequiredService()` sprinkled through render logic.

### 9.4.3 Failure Behavior and Mutability

Missing required services MUST fail with a component-specific diagnostic or exception message that identifies the component and injected member name.

For example, a failure should read in source terms such as:

> Failed to resolve required service `ITodoService` for `TodoBoard.todoService`.

Implementations SHOULD wrap or enrich raw container exceptions with this CSXAML component context rather than surfacing only a low-level container error.

Injected services are read-only bindings. They MUST NOT be assignable from markup and MUST NOT flow through generated component props.

### 9.4.4 Deliberate Exclusions for v1

To keep the model small and explicit, v1 SHOULD NOT center dependency injection on:

- component parameters that blur props with services
- markup-based injection forms or service mini-languages
- attribute-based injection such as `[Inject]` that requires arbitrary member scanning to discover dependencies
- keyed, named, or optional service syntax
- property injection or service values forwarded through generated props

### 9.5 Local Helper Code

After injected-service declarations and state declarations and before the final render return, a component body MAY contain ordinary C# local declarations and local functions.

This exists so authors can:

- compute local values once and reuse them in markup
- give repeated logic a readable name
- keep component-specific helper behavior close to the render path

For example:

```csharp
component Element TodoBoard {
    State<List<TodoItemModel>> Items = ...;

    var selected = Items.Value.Single(item => item.Id == SelectedItemId.Value);
    string HeaderText() => $"{selected.Title} ({Items.Value.Count})";

    return <TodoEditor Title={selected.Title} Header={HeaderText()} />;
}
```

This helper code SHOULD remain ordinary C# and SHOULD NOT require a second statement language.

### 9.6 Required Return

Each component body currently contains exactly one outermost `return` of a markup node.

The render return is the `return` that is immediately followed by a markup opener `<` at the outermost component-body level.

Nested `return` tokens inside local functions, lambdas, or nested C# blocks do not count as the component render return.

This restriction keeps the core grammar simple and easy to reason about while still allowing local helper code before the view is returned.

### 9.7 Duplicate Names

Duplicate parameter names, duplicate injected-service names, and duplicate state field names are invalid.

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

### 10.5 Async Work and Lifecycle Direction

CSXAML SHOULD keep async work and side effects in ordinary C# methods, local helper code, and a minimal runtime lifecycle API rather than inventing a second effect language.

v1 SHOULD define at least:

- how mount and unmount notifications are expressed
- how cleanup or disposal is performed
- what happens when async work completes after unmount
- how explicit state updates from async work remain predictable and testable

The goal is explicit, boring behavior rather than ambient framework magic.

### 10.6 Relationship to Injected Services

`State<T>` and `inject` serve different roles and SHOULD remain visibly distinct:

- `State<T>` represents component-owned mutable UI state
- `inject` represents ambient app services supplied by the host
- state is mutable from component logic
- injected services are read-only bindings

This distinction helps preserve one of the language's clearest boundaries: props are public inputs, state is local mutable UI state, and injected services are ambient collaborators.

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

Resolution SHOULD be exact and deterministic across local components, referenced component libraries, and imported external controls.

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

Component elements support child content only when the target component explicitly declares one default slot outlet.

In v1:

- the outlet syntax is bare `Slot`
- at most one default slot outlet is allowed per component definition
- `Slot` attributes are invalid
- `Slot` child nodes are invalid
- `Slot Name="..."` MUST fail with a clear "named slots are not supported yet" diagnostic
- a root-level `<Slot />` is invalid because components still return one root node rather than fragments

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

This does not prevent ordinary C# locals or local functions before the final render return in a component body.

Those child-position forms can be added later if they genuinely improve the model, but they should not arrive at the cost of turning markup parsing into general C# parsing.

---

## 16. Native Element Semantics

### 16.1 Metadata-Driven Surface

Native elements are validated against generated control metadata.

This metadata model SHOULD apply to both built-in controls and supported external controls.

Metadata answers questions such as:

- is this tag a known native control?
- what properties are exposed?
- what events are exposed?
- what child-content shape is allowed?
- what value-kind hint should apply?

### 16.1.1 External Control Metadata

Supported external controls SHOULD enter the system through referenced-assembly-aware metadata generation.

The implementation SHOULD prefer deterministic discovery, such as explicit project/package reference handling plus controlled inclusion rules, over unconstrained scanning of every reachable CLR type.

The current supported slice and its intentional limitations SHOULD stay documented explicitly in [docs/external-control-interop.md](docs/external-control-interop.md) so roadmap promises remain concrete.

For each supported external control, metadata SHOULD capture at least:

- CLR type identity
- namespace and assembly identity
- supported properties
- supported events
- child-content shape
- value-kind hints

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
- `Style`
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

### 16.6 Style and Theme Values

CSXAML v1 keeps styling inside the ordinary attribute expression model.

Authors SHOULD be able to write:

- `Background={Theme.CardBrush}`
- `Foreground={Theme.OnPrimaryBrush}`
- `Style={AppStyles.PrimaryButton}`

The language SHOULD NOT add a second styling syntax for v1 such as:

- selector mini-languages
- CSS-like classes
- framework-owned style resources with special CSXAML-only lookup rules

Reusable visual values SHOULD just be ordinary C# values or ordinary WinUI-compatible objects.

When a reusable helper needs resource lookup or fallback creation, that behavior SHOULD live in library/runtime code rather than new markup syntax.

To preserve hostless rendering and component-test ergonomics, style helper objects MAY defer WinUI resource or style realization until projected rendering time.

This keeps the source model simple while leaving room for richer composition, styling, and slot scenarios later.

### 16.7 Runtime Application Model

Runtime native property application SHOULD use:

- generated metadata
- explicit adapter logic
- explicit conversion helpers

Runtime native property application SHOULD NOT use reflection as the primary hot-path mechanism.

For supported external controls, runtime creation and patching SHOULD flow through generated adapters or an explicit registration model derived from the same metadata.

A late-bound reflection fallback MAY exist for experiments or narrow escape hatches, but it SHOULD NOT define the core v1 runtime path.

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
- injected services are not part of the markup prop surface and MUST NOT be accepted as attributes
- child content is invalid unless the callee declares a default slot outlet
- when child content is allowed, it is transported separately from the public props record
- named slots are intentionally deferred; v1 supports one default slot only

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
- invalid or misplaced `inject` declarations
- duplicate attributes
- child-content violations

### 19.2 Tooling Surface

The language design SHOULD naturally support:

- tag completion
- native prop and event completion
- component prop completion
- source squiggles
- go-to-definition
- formatting and indentation
- generated/source mapping

This is one reason metadata-driven validation is so important. It helps the editor speak the same language as the compiler and runtime.

### 19.3 Generated Code Philosophy

Generated C# SHOULD be:

- deterministic
- readable
- debuggable when necessary
- boring

Developers should not need to live in generated code, but when they do open it, it should make sense.

### 19.4 Runtime Context

Runtime failures such as missing required injected services SHOULD identify the component name, source file when known, and injected member name rather than surfacing raw container or activation errors alone.

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
- a second framework-owned styling DSL for v1
- implicit lifecycle magic when ordinary C# plus small runtime hooks would do

This is intentional. The project is trying to produce a more modern C#-first authoring experience, not a byte-for-byte clone of legacy XAML.

---

## 21. Developer Experience Guidelines

The language should feel pleasant in everyday use.

### 21.1 Mental Model

The recommended mental model is:

- write structure in tags
- declare ambient app services explicitly with `inject`
- compute local values in ordinary C# helper code before the final render return
- write values in C#
- write behavior in lambdas or method calls
- write reusable inputs as typed component props
- keep component files small and obvious

### 21.2 Authoring Conventions

The following conventions are strongly recommended:

- PascalCase for component names and native tags
- PascalCase for native prop and event names
- camelCase for local variables, injected service names, and state names when they are true variables
- one component per file
- small components with explicit props
- prefer named local values and helper functions over repeating heavy expressions inline
- prefer WinUI styles/resources and plain C# theme objects over framework-specific styling DSLs
- helper logic in ordinary C# types rather than giant inline expressions when expressions become hard to read

### 21.3 Why This Feels Modern

CSXAML feels modern when it lets a developer:

- stay in one file
- keep helper logic close to the view when it is local, and move it out when it becomes shared
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
- metadata-driven common layout property support for the current curated controls
- metadata-driven `Style` support for built-in controls and the current supported external-control slice
- owner-qualified attached-property syntax for the built-in slice
- generated generic native prop and event bags
- generated generic attached-property bags
- runtime adapters for `Border`, `Button`, `Grid`, `ScrollViewer`, `StackPanel`, `TextBlock`, `TextBox`, and `CheckBox`
- reusable style helpers that stay as ordinary expression values in hostless logical-tree rendering and resolve during WinUI projection
- `Key`-driven identity for repeated rendering
- attached-property carry-through from component usages to rendered native roots
- automation metadata hooks for semantic component testing
- deterministic project-default namespace fallback plus one internal generated namespace convention for project-level support files
- explicit generated component manifests for referenced-assembly component discovery
- cross-project component imports through the same `using` and alias model used for external controls
- shared repo-level build targets with deterministic `obj` output, write-if-changed generation, stale-file pruning, and clean integration
- proof that ordinary C# test projects can consume generated CSXAML components through normal project references

Known gaps relative to the intended v1 experience:

- whole-file comment support needs further hardening beyond the helper-code and boundary scanners
- external control namespace syntax, referenced-assembly discovery, and generated runtime registration are implemented for the current supported slice; [docs/external-control-interop.md](docs/external-control-interop.md) describes that slice and its current limitations, while broader external control shape coverage is still in progress
- the v1 styling story is intentionally thin and currently stops at reusable values plus WinUI `Style` support; richer styling constructs remain future work
- broader attached-property owner resolution through file-level imports and external metadata is not yet implemented
- named slots, slot fallback content, and fragment-root slot pass-through are intentionally deferred
- explicit lifecycle/disposal semantics are not yet finished
- explicit component-level `inject` declarations and service-aware component activation are now part of the intended v1 spec surface, but they are not yet implemented in the current prototype
- parser/validator diagnostics, direct source-authored build failures, deterministic source-map sidecars under `obj`, and staged runtime exception context are now implemented for the current slice; fuller debugger integration and broader IDE coverage are still ahead
- formatting support is not yet defined end to end
- repo-local project-system build behavior is now explicit and deterministic, but NuGet-delivered build packaging and richer IDE design-time tooling are still ahead

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
