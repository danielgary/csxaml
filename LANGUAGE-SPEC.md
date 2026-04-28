# CSXAML Language Specification

## Status

This document defines the draft v1 language shape for CSXAML.

It is intended to do three things at once:

- describe the language that exists today in the prototype
- define the compatibility and parsing rules that future work should preserve
- give the project a clear developer-experience target for a modern C# + XAML + WinUI authoring model

Normative language in this document uses `MUST`, `SHOULD`, and `MAY`.

Sections 1 through 21 define the draft v1 language contract.

Sections 22 through 24 are explicitly non-normative and record current prototype coverage, an illustrative example, and project-level closing guidance only.

When this document mentions current implementation behavior, that text is descriptive unless the same rule is also stated normatively in the language sections above.

Behavior that is not described in this document is out of scope for draft v1 and MUST NOT be inferred solely from generated code or temporary prototype quirks.

Implementation notes call out places where the current prototype only implements a subset of the intended v1 surface.

### Revision Log

This maintained revision log starts with the final pre-1.0 tightening pass. Earlier fixed choices remain recorded in the decision log below.

| Revision | Date | Summary |
| --- | --- | --- |
| `draft-v1-prepin-alignment` | 2026-04-15 | Closed the last known source/implementation drift around attached-property owner visibility and dotted tag admission: generator, tooling, demos, fixtures, and repo docs now follow the spec's ordinary-import/type-alias owner-resolution model, and tooling/parser tag scanning now consistently admits dotted tag forms. |
| `draft-v1-prepin-final` | 2026-04-15 | Final pre-1.0 contract tightening pass: added a maintained revision log; clarified file-local helper support, `using static`, balanced-island lexical requirements, render detection examples, slot placement, controlled-input adapter expectations, duplicate-key failure phase, and runtime failure/disposal wording. |
| `draft-v1-state-equality` | 2026-04-15 | Changed `State<T>` invalidation from always-dirty-on-assignment to reference-equality (reference types) and default-equality (value types) suppression; added explicit `Touch()` method as the rerender escape hatch for controlled-mutation patterns; updated §10.2.1, added §10.2.1.1, and updated §10.4 accordingly. |

### Decision Log

The following decisions are fixed for this revision of the spec:

- the final markup statement syntax is `render <Root />;`
- `render` does not require or permit wrapping the markup root in parentheses
- `return <Root />;` is invalid
- `return ( <Root /> );` is invalid
- `component Element` remains the v1 declaration form because the language still distinguishes a renderable UI component kind explicitly
- v1 keeps exactly one component declaration per `.csxaml` file; additional file-local helper components remain deferred
- file-level ordinary C# `using` support includes `using static`, but static imports affect only ordinary C# name lookup, not tag resolution or attached-property owner lookup
- `State<T>` invalidation skips rerender on reference equality (reference types) or default-equality (value types); authors use `Touch()` to force a rerender despite equality
- `State<T>` invalidation remains assignment-driven in v1; in-place mutation does not trigger rerender without an explicit `Touch()` call
- default slot outlets remain at-most-once and are invalid inside `foreach` in v1
- the runtime lifecycle contract is normative for mount, rerender, unmount, and disposal behavior, but v1 does not yet add dedicated source-level cancellation syntax

CSXAML uses `render` instead of `return` because the final markup statement is a CSXAML construct, not a general C# expression return. Using a dedicated keyword makes the language boundary explicit and avoids implying ordinary C# return semantics where they do not exist. It also gives the parser a dedicated final markup statement to detect instead of a special-case `return`.

---

## 1. What CSXAML Is

CSXAML is a source-generated UI language for WinUI applications.

It is not:

- a replacement for C#
- a replacement for WinUI
- an XML dialect that tries to preserve every XAML rule
- a string-based template language
- a complete replacement for every app-shell, templating, virtualization, or `DataContext`-driven WinUI pattern

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

A conforming CSXAML implementation MUST behave as if it follows this pipeline:

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

Implementations MAY fuse or optimize stages internally as long as the observable language contract remains the same.

This design keeps responsibilities clean:

- CSXAML owns structure and metadata-driven validation
- C# owns expression semantics and type checking
- WinUI owns final control behavior

---

## 5. Source File Model

### 5.1 File Extension

CSXAML source files use the `.csxaml` extension.

### 5.2 Top-Level Shape

In v1, a `.csxaml` file MUST contain exactly one component declaration plus optional file-level `using` directives, an optional file-scoped namespace, and optional file-local helper declarations.

This one-component-per-file model keeps symbol discovery, generated-file identity, source mapping, and workspace indexing boring and deterministic.

Additional file-local helper components remain deferred rather than implied by omission.

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

### 5.4.1 Allowed File-Level `using` Forms

A `.csxaml` file MAY contain ordinary non-global C# `using` directives in these source forms:

- namespace import: `using MyApp.Components;`
- namespace alias: `using Fluent = CommunityToolkit.WinUI.Controls;`
- type alias: `using AutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;`
- static type import: `using static System.Math;`

Project-level `global using` directives supplied by surrounding C# compilation still participate indirectly through ordinary C# type resolution, but `.csxaml` source MUST NOT invent a separate import syntax.

Static imports participate only in ordinary C# name lookup for helper code, state initializers, and expression islands. They do not create tag prefixes, they do not add simple-tag namespaces, and they do not change attached-property owner resolution.

### 5.4.2 Tag Resolution Algorithm

For a simple tag `<Name />`, the implementation MUST build a candidate set from:

- visible CSXAML component types named `Name` in the file namespace or namespaces imported by ordinary `using Namespace;`
- built-in native WinUI controls from the default control-metadata set
- visible external controls named `Name` in namespaces imported by ordinary `using Namespace;`

If the candidate set contains exactly one supported match, that match is selected.

If the candidate set is empty, the tag is unknown.

If the candidate set contains more than one supported match, the tag is ambiguous and MUST produce a diagnostic. Implementations MUST NOT silently prefer a component over a native control or a native control over a component.

For an alias-qualified tag `<Alias:Name />`, `Alias` MUST resolve through a namespace alias introduced by `using Alias = Some.Namespace;`.

The implementation MUST search only that aliased namespace across visible component manifests and native/external control metadata.

Type aliases do not create tag prefixes.

If exactly one supported `Name` exists in the aliased namespace, it is selected. Otherwise the implementation MUST emit an unknown-tag or ambiguous-tag diagnostic as appropriate.

Alias-qualified tags MUST NOT fall back to broad global search.

### 5.4.3 Attached-Property Owner Resolution

For an attached-property attribute `Owner.Property`, `Owner` MUST resolve the same way an ordinary visible type name resolves in the file's C# namespace context:

- an in-scope type name made visible through the file namespace or ordinary `using Namespace;` imports
- or a C# type alias introduced by `using Alias = Some.Type;`

Namespace aliases are not valid attached-property owners by themselves.

If exactly one attached property named `Property` exists on the resolved owner type in loaded metadata, the attribute binds to that attached property.

If no such attached property exists, the attribute is invalid.

If multiple supported attached properties with the same source spelling would match, the attribute is ambiguous and MUST produce a diagnostic.

### 5.5 File-Local Helper Declarations

CSXAML v1 supports file-local helper declarations in the same file as the component.

This is the intended equivalent of helper functions and helper types living next to a React component in a `.tsx` file.

These helper declarations are part of the v1 language surface, not merely a future goal.

The supported v1 shape is:

- component-local local variables and local functions before the final render statement
- file-local helper types in the same `.csxaml` file

For example:

```csharp
file sealed class TodoFormatter
{
    public static string Format(string value) => value.Trim();
}
```

File-local helper declarations:

- share the file's `namespace` and `using` context
- MUST lower predictably to ordinary generated C#
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
- `render`
- `if`
- `foreach`
- `var`
- `in`

Additional keywords SHOULD be introduced sparingly and only when they clearly improve readability or parseability.

Ordinary C# `return` remains valid inside opaque helper code and C# expression islands. It is not the CSXAML final-markup statement.

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

CSXAML comments use ordinary C#-style `//` and `/* ... */` forms.

Comments SHOULD be allowed wherever whitespace is otherwise allowed.

The current prototype still lags that language rule in some whole-file scenarios and currently relies most heavily on comment handling inside C# islands and helper-code scanning.

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
                           render-statement
                         "}"

render-statement      ::= "render" markup-node ";"

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

Comments and whitespace are omitted from this overview grammar and MAY appear wherever whitespace is otherwise allowed unless a rule explicitly prohibits them.

Where:

- `using-directive` and `namespace-declaration` use ordinary C# source forms
- `helper-declaration` is file-local helper declaration text that does not contain CSXAML markup and lowers predictably to generated C#
- `csharp-type` is C# type syntax
- `csharp-helper-code` is component-local ordinary C# declarations/statements before the final render statement
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
- respect C# lexical forms that suspend delimiter meaning
- stop only when the matching outer delimiter is reached

Balanced island scanning MUST treat the following C# lexical forms as opaque:

- regular string literals
- verbatim string literals
- interpolated string literals
- raw string literals
- character literals
- line comments
- block comments
- nested interpolation holes inside interpolated strings

The scanner is lexical rather than semantic. It MUST NOT reinterpret `<` inside a C# island as markup, and it MUST NOT attempt to decide whether a token sequence represents generics, comparison operators, or some other valid C# form.

Preprocessor directives MAY appear inside opaque C# islands and helper code. They participate only as opaque C# text and MUST NOT introduce new CSXAML grammar forms.

Conforming implementations MAY satisfy this requirement by delegating lexical work to Roslyn or to an equivalent scanner. A lightweight custom scanner is acceptable only if it still handles the lexical forms listed above correctly. An implementation that fails on ordinary modern C# string, raw-string, interpolation, or comment forms does not satisfy the v1 compatibility goal.

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
- while scanning component-local helper code, the parser tracks C# lexical state, delimiter depth, and nested block depth
- the component render statement is the first `render` token encountered at helper depth zero whose next non-trivia source text begins with a markup opener `<`
- `render` is valid only as the final outermost markup statement of a component body
- nested `render` tokens inside strings, comments, local functions, lambdas, or nested blocks do not terminate helper-code scanning
- ordinary C# `return` inside helper code remains ordinary opaque C# text and does not participate in CSXAML render-statement detection
- `return <Grid />;` and `return ( <Grid /> );` are invalid and SHOULD produce targeted diagnostics that tell the author to use `render <Grid />;`
- helper code MUST not itself contain CSXAML markup fragments

For example, this is valid:

```csharp
component Element TodoBoard {
    var title = "Todo";
    render <TextBlock Text={title} />;
}
```

This is invalid:

```csharp
component Element TodoBoard {
    return <TextBlock Text="Todo" />;
}
```

This is also invalid:

```csharp
component Element TodoBoard {
    render Title;
}
```

This keeps the parser small while still letting component authors write named local values and helpers in the same file.

Outside parser positions that explicitly expect markup, `<` MUST be treated as part of opaque C# text rather than as a tag opener.

### 8.6 Name Resolution Guardrails

- simple tag names imported through `using Namespace;` may bind only when a single supported match exists
- alias-qualified tags such as `Alias:Control` MUST not fall back to broad best-effort searches
- unknown or unsupported external controls MUST fail at compile-time validation if metadata is unavailable
- import and alias rules SHOULD stay file-local and explicit enough that tooling can resolve them without whole-solution guesswork

### 8.7 Parser Recovery

When a parse failure occurs, implementations SHOULD recover at the nearest stable structural boundary that can be recognized without guessing, such as:

- `/>`
- `>`
- `</Tag>`
- `}`
- end of file

Recovery SHOULD prefer surfacing multiple local diagnostics over abandoning the rest of the file after the first malformed construct.

---

## 9. Component Declarations

### 9.1 Core Form

A component declaration has this shape:

```csharp
component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
    render <StackPanel>
        <TextBlock Text={Title} />
        <Button Content="Toggle" OnClick={OnToggle} />
    </StackPanel>;
}
```

### 9.2 `component Element`

`component` introduces a CSXAML component declaration.

`Element` identifies the component kind currently supported by the language: a renderable UI component that produces a markup tree.

`Element` is the only component kind defined by v1.

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

    render <StackPanel>
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

- MAY appear only in the component body prologue before ordinary helper code and before the final render statement
- MAY be interleaved with `State<T>` declarations
- MUST NOT appear in markup child positions
- SHOULD be available to component helper code and C# expression islands like ordinary component members

### 9.4.2 Resolution Semantics

Each `inject` declaration resolves once per component instance from the ambient component service provider.

The runtime SHOULD build on the ordinary .NET `IServiceProvider` model rather than inventing a second container abstraction.

The host container's normal singleton, scoped, and transient behavior SHOULD remain authoritative. CSXAML SHOULD NOT add language-level lifetime syntax for v1.

Generated code SHOULD lower injected services to boring cached members such as constructor-initialized readonly fields or equivalent instance-level bindings, rather than re-resolving services during each render.

Component helper code SHOULD prefer explicit `inject` declarations over ad hoc service-locator calls such as `GetRequiredService()` sprinkled through render logic.

Resolved injected bindings remain stable for the lifetime of that component instance. They do not automatically refresh on ordinary rerender.

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

### 9.4.5 Ambient Scope and Disposal

All `inject` declarations in a component instance resolve from the same ambient component service scope.

Child components inherit that same ambient scope by default unless the host establishes a different scope outside CSXAML source syntax.

CSXAML v1 does not define component-authored nested scopes or syntax for establishing child service scopes.

Injected services resolve before state initialization and before the component's first render.

State initializers MAY reference injected services and earlier state fields to the extent ordinary generated C# allows. They MUST NOT depend on later declarations becoming available early.

Each injected member resolves independently from the same ambient scope. The declaration order of multiple `inject` members does not create a second dependency model beyond ordinary container resolution.

CSXAML v1 does not define asynchronous service resolution. Required injected services MUST be synchronously obtainable from the ambient scope during component instance creation.

For dispatcher-affine native UI hosts such as WinUI, the host/runtime MUST ensure that component creation, render, native projection, and disposal run on the host's required UI scheduler or are marshaled there before touching native state. CSXAML v1 does not define cross-dispatcher component transfer or reparenting semantics.

Unmounting a component does not individually dispose injected services. Disposal of singleton, scoped, or transient services follows host container ownership rules. If the host creates a component subtree scope, the host owns disposal of that scope when the subtree unmounts.

### 9.4.6 Component Instance Creation and Rerender Order

A rendered component corresponds to a retained runtime instance with stable identity until reconciliation removes it or replaces it because the parent tree no longer matches the same component kind and key.

For an initial mount, the runtime MUST behave as if it performs these steps in order:

1. create the component instance
2. store the incoming props on that instance
3. resolve injected services from the ambient scope
4. initialize state fields in source order
5. execute the component render
6. reconcile and project the produced child tree

Helper code and expression islands evaluate against the current component instance, current props, current state values, and current injected services using ordinary C# closure semantics.

State initializers run exactly once per component instance creation. They MUST NOT rerun on ordinary rerenders of a preserved instance.

For a preserved instance on rerender, the runtime MUST behave as if it performs these steps in order:

1. store the incoming props and child content on that existing instance
2. keep the previously resolved injected bindings in place
3. execute the component render against the updated props, preserved state, preserved injected bindings, and current helper-code evaluation context
4. reconcile and project the produced child tree

### 9.4.7 Render Scheduling

Component render for a single instance MUST be non-reentrant.

Explicit state assignment marks the component dirty synchronously, but the host runtime MAY batch multiple dirty marks into one later rerender pass.

The observable behavior MUST be as if at least one rerender occurs after the last successful state assignment before the pending render queue drains.

### 9.4.8 Mounted, Unmounted, and Disposed States

After a component instance successfully renders and its produced child tree is reconciled, that instance enters the mounted state.

While a component instance is retained across rerenders, the same instance continues to render with updated props and child content, preserved state, and the same resolved injected bindings.

When reconciliation removes a component instance from the tree, the instance becomes unmounted. After unmount:

- the instance MUST NOT schedule new renders into the host tree
- later async continuations MAY still run as ordinary C# work, but post-unmount state writes MUST NOT resurrect the component
- child component disposal and runtime cleanup MUST proceed according to the retained-tree removal path

When a component subtree or host is disposed, the runtime MUST dispose retained child components exactly once according to ordinary `IDisposable` and `IAsyncDisposable` ownership rules.

### 9.4.8.1 Disposal Ordering and Participation

Component cleanup participates through ordinary component-instance implementation of `IDisposable` or `IAsyncDisposable`. CSXAML v1 does not add a separate disposal declaration keyword in source syntax.

When a component subtree is disposed, child component instances MUST be disposed before their parent component instance is disposed.

Disposal is therefore depth-first with respect to the retained component tree. Sibling disposal order is not otherwise a source-level language contract.

File-local helper declarations are not independently disposed by the CSXAML runtime merely because they appear in the same `.csxaml` file. They participate in cleanup only through ordinary C# ownership from component code.

If disposal enters through an async host/runtime path, `IAsyncDisposable` cleanup SHOULD run through that async path. If disposal enters through a synchronous path, the runtime MAY block while waiting for async cleanup to complete.

CSXAML v1 does not define a separate automatic background-disposal scheduler. Any UI-thread affinity for disposal remains a host/runtime concern.

### 9.4.9 Render Failure Semantics

If component render or logical-tree expansion throws during a render pass, that render pass aborts and the exception MUST surface through the runtime failure path.

When a child render fails, later siblings in that containing render pass are not processed for that pass.

New child component instances created during the aborted pass MUST be disposed during abort cleanup. Previously retained component instances from the last successful pass remain the retained instances for future attempts unless later reconciliation removes them explicitly.

No new logical render tree is committed for that failed pass.

The last successful logical tree remains the retained logical baseline for future reconciliation attempts.

### 9.5 Local Helper Code

After injected-service declarations and state declarations and before the final render statement, a component body MAY contain ordinary C# local declarations and local functions.

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

    render <TodoEditor Title={selected.Title} Header={HeaderText()} />;
}
```

This helper code SHOULD remain ordinary C# and SHOULD NOT require a second statement language.

### 9.6 Required Render Statement

Each component body MUST end with exactly one outermost render statement of the form:

```csharp
render <Root />;
```

The render statement is the outermost `render` whose next non-trivia source text begins with markup.

Nested `render` tokens inside local functions, lambdas, strings, comments, or nested C# blocks do not count as the component render statement.

The payload of `render ...;` is still exactly one markup root. It is not a general C# expression form.

`render` is a CSXAML-specific statement used to emit the component's final markup tree. CSXAML uses `render` instead of `return` because this final statement is not an ordinary C# expression return, and using a dedicated keyword makes that boundary explicit while simplifying parser detection.

### 9.6.1 Render-Statement Diagnostics

Implementations SHOULD define targeted diagnostics for at least these cases:

- `return <Grid />;`
- `return ( <Grid /> );`
- `render` followed by non-markup content
- missing trailing `;` after the final render statement
- missing final `render` statement in a component body
- multiple sibling markup roots where a single root is required

Diagnostics for invalid final-markup syntax SHOULD explicitly tell the author to use `render <Root />;`.

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

These declarations are CSXAML component-prologue declarations, not ordinary C# field initializers pasted verbatim into a generated class body. A conforming implementation MAY lower them to backing members, helper methods, or equivalent generated structure as long as the observable lifecycle and invalidation behavior defined in sections 9 and 10 is preserved.

### 10.2 Semantics

A state field:

- belongs to the component instance
- has an explicit generic type argument
- has an initializer expressed in C# syntax
- invalidates the component when its value changes

### 10.2.1 Value Observation and Assignment

`State<T>` observation in v1 is assignment-driven.

Only reads and writes of `State<T>.Value` are part of the language contract.

A successful assignment to `Value` MUST mark the owning component dirty unless the newly assigned value is equal to the previous value under the equality rule defined below. When the equality rule treats the new and previous values as equal, the assignment MUST NOT mark the component dirty.

The equality rule is:

- for reference types, `ReferenceEquals(previous, next)`
- for value types, `EqualityComparer<T>.Default.Equals(previous, next)`

Implementations MUST NOT use `Equals` or `IEquatable<T>` for reference-type comparison. The contract for reference types is identity, not semantic equality. This avoids surprising no-rerender behavior for `record` types and other reference types that override `Equals` with value-based semantics, and it keeps the equality check cheap and predictable.

Authors who need to signal a rerender despite equality MUST use the explicit `Touch()` method described in §10.2.1.1.

In-place mutation of an object or collection currently stored inside a `State<T>` does not automatically invalidate the component, and reassigning the same reference back to `Value` no longer signals invalidation by itself. Authors who mutate in place MUST call `Touch()` to trigger rerender.

### 10.2.1.1 The `Touch()` Method

`State<T>` exposes a `Touch()` method that marks the owning component dirty without changing the stored value.

`Touch()` exists for the explicit case where an author has mutated the contents of a reference held by `Value` in place and wants to signal a rerender without replacement, or where an author wants to force a rerender despite the equality rule defined in §10.2.1.

`Touch()` MUST mark the component dirty unconditionally. It follows the same scheduling and batching rules as ordinary `Value` assignments (§10.2.2).

Authors SHOULD prefer replacement over mutation-plus-`Touch()` where practical, because replacement keeps data flow assignment-driven and easier to trace per §10.4. `Touch()` is the documented escape hatch when replacement is impractical, not a first-choice pattern.

Calling `Touch()` on a state field belonging to an unmounted component MUST follow the same rule as a post-unmount `Value` write per §10.2.4: the call MUST NOT resurrect or rerender the disposed component instance.

### 10.2.2 Scheduling and Batching

The `Value` setter stores the new value and marks the component dirty synchronously.

The actual rerender MAY be queued or batched by the host runtime.

Multiple `Value` assignments before the next render pass MAY coalesce into one rerender.

Rerender proceeds top-down from the invalidated component and reconciles descendants against the prior child tree.

### 10.2.3 Thread Affinity and Render Safety

State updates that trigger UI work MUST execute on the UI scheduler or dispatcher associated with the owning component host instance, or be marshaled there by that host runtime before rendering occurs.

State writes during render are invalid and SHOULD fail with a clear runtime error rather than recursively reentering the same component render.

Implementations SHOULD also surface a compile-time or generated-code diagnostic when a render-phase state write is statically obvious, but the runtime non-reentrancy rule remains authoritative.

### 10.2.4 Unmount Safety

After a component unmounts, later state writes MUST NOT resurrect or rerender that disposed component instance.

Implementations SHOULD ignore such writes and surface a controlled debug diagnostic rather than allowing stale async work to reattach UI.

### 10.3 Type Matching

The type argument in the field declaration and the constructor call MUST refer to the same state type.

This rule keeps the syntax boring and predictable:

```csharp
State<int> Count = new State<int>(0);      // valid
State<int> Count = new State<long>(0);     // invalid
```

### 10.4 Data Flow Preference

For predictable UI behavior, CSXAML SHOULD prefer explicit state updates over magical binding rules.

Explicit state does not require immutable data structures.

Authors MAY replace a value with a new object, or mutate a local object or collection and then assign it back through `Value`.

The language contract is assignment-driven, not collection-strategy-driven.

Authors who mutate in place and then reassign the same reference MUST use `Touch()` (§10.2.1.1) to trigger rerender. Reassigning the same reference no longer signals invalidation by itself under the equality rule in §10.2.1.

That means this style is preferred:

```csharp
OnToggle={() => Items.Value = Items.Value.Select(...).ToList()}
```

over hidden mutation models that make rerender causes hard to trace.

### 10.5 Async Work and Lifecycle Direction

CSXAML SHOULD keep async work and side effects in ordinary C# methods, local helper code, and a minimal runtime lifecycle API rather than inventing a second effect language.

The runtime lifecycle ordering described in section 9.4 is normative for mount, rerender, unmount, and disposal behavior.

The author-facing source syntax for richer lifecycle hooks remains intentionally small in v1. CSXAML does not define hook-style source primitives as the primary model.

Any future lifecycle API MUST define at least:

- how mount and unmount notifications are expressed
- how cleanup or disposal is performed
- what happens when async work completes after unmount
- how explicit state updates from async work remain predictable and testable

The goal is explicit, boring behavior rather than ambient framework magic.

Async continuations behave like ordinary C# continuations attached to component methods or delegates.

Async work MAY update state only through explicit `Value` assignment, and the unmount rule in section 10.2.4 still applies after the component leaves the tree.

CSXAML v1 does not yet add dedicated source-level cancellation syntax. Authors SHOULD use ordinary `CancellationTokenSource` ownership patterns in helper code when cancellation matters. A future per-instance cancellation surface MAY be added, but it MUST preserve explicit ordinary-C# control flow.

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

A component produces exactly one root markup node.

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
- a `Slot` outlet MAY appear directly under markup nodes or inside `if` blocks within the single rendered root subtree
- a `Slot` outlet MUST NOT appear inside `foreach` because repeated slot expansion semantics are undefined in v1
- `Slot` attributes are invalid
- `Slot` child nodes are invalid
- `Slot Name="..."` MUST fail with a clear "named slots are not supported yet" diagnostic
- a root-level `<Slot />` is invalid because components still produce one root node rather than fragments

### 12.4.1 Default Slot Transport Semantics

When a caller supplies child content to a component that declares a default slot outlet, that child content is transported as an ordered slot-content fragment separate from the component's public props.

The callee renders that fragment at the location of its sole `Slot` outlet.

Caller child order MUST be preserved.

If no caller child content is supplied, the default slot renders nothing.

CSXAML v1 does not define fallback content inside `Slot` itself. Any child nodes nested inside `<Slot> ... </Slot>` remain invalid until a later version specifies fallback semantics.

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

Quoted text is always literal source text.

For example, `Text="Title"` renders the exact string `Title`. It does not look up a variable, property, or resource named `Title`.

When the author intends a string-valued variable, property, method call, or interpolation, the expression form MUST be used instead:

```xml
<TextBlock Text={Title} />
```

Tooling SHOULD flag likely quoted-identifier mistakes when metadata expects a string and the quoted text matches an in-scope symbol name.

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

This does not prevent ordinary C# locals or local functions before the final render statement in a component body.

Those child-position forms can be added later if they genuinely improve the model, but they should not arrive at the cost of turning markup parsing into general C# parsing.

Authors who need richer branching SHOULD compute a local value or helper method before the final render statement and keep child-position markup intentionally narrow.

---

## 16. Native Element Semantics

### 16.1 Metadata-Driven Surface

Native elements are validated against generated control metadata.

This metadata model SHOULD apply to both built-in controls and supported external controls.

The language specification defines the metadata contract abstractly.

The exact built-in or external control coverage of a particular implementation is a versioned compatibility matter rather than part of the core language grammar.

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

### 16.1.2 DependencyProperty and Local-Value Semantics

When CSXAML applies a native property to a projected WinUI element, the effect MUST be understood in WinUI terms, not as a platform-neutral field assignment.

Unless a control adapter explicitly documents a different projection rule, setting a native property through CSXAML behaves like assigning a local value on the projected WinUI object.

That means WinUI's normal precedence rules remain authoritative for:

- local values
- styles
- templates
- inherited values
- animations
- resource-driven values

CSXAML does not override or flatten those precedence rules into a second framework-owned property system.

### 16.1.3 Attached Properties and Owner Lookup

Attached-property usage remains metadata-driven.

The author-facing source rule is that attached-property owners resolve through the same visible type context described in section 5.4.3.

If the current implementation only supports a narrower attached-property-owner slice than the normative source rule, that narrower slice MUST be called out explicitly in roadmap or prototype-coverage documentation rather than implied away in the language text.

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
- `OnTextChanged`
- `OnCheckedChanged`

This keeps the authoring model consistent and easy to discover.

### 16.3.1 Event Handler Binding

The value of an event attribute MUST be a C# expression that converts to the target delegate type under ordinary C# rules.

Compatible method groups, lambda expressions, anonymous functions, and delegate-valued expressions are all valid event values.

String handler names are invalid.

If an async lambda or async method group converts successfully to the target delegate type, it is valid.

Unhandled synchronous or asynchronous exceptions from event handlers MUST flow through the host UI exception pipeline and MUST NOT be silently swallowed by the CSXAML runtime.

### 16.3.2 Event Subscription Lifetime

On rerender, the runtime MUST ensure that the active event subscription corresponds to the latest rendered delegate value.

When the rendered handler changes, the previous subscription MUST be removed before or as the new handler is attached.

When a native node unmounts or is replaced, all subscriptions associated with that node MUST be removed.

The runtime MAY retain an existing subscription when the rendered delegate identity is unchanged and doing so preserves the same observable behavior.

### 16.3.3 Event Payload Normalization

Supported controls MAY expose normalized CSXAML event payload shapes that intentionally simplify richer platform event args into a more direct authoring delegate.

Normalization is metadata-defined and adapter-defined, not a heuristic applied from the event name alone.

When an implementation chooses that normalization, the normalized delegate shape becomes part of the supported compatibility surface for that control and MUST be documented by the implementation's metadata or compatibility documentation. Validation, emission, and tooling MUST use that same documented normalized delegate surface for that control.

A control MAY expose both a raw platform event shape and a normalized CSXAML event shape only when the supported metadata documents them as distinct events.

In the current built-in slice, that includes:

- `TextBox` exposing `OnTextChanged={Action<string>}`
- `CheckBox` exposing `OnCheckedChanged={Action<bool>}`

That normalization is not implied for every supported control.

Absent a documented normalized shape, a supported control's event surface follows the delegate shape exposed by its metadata and adapter contract. External controls MUST NOT assume automatic payload normalization.

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

### 16.6.1 Theme and Resource Propagation

CSXAML does not define a second theme or resource propagation model.

Theme changes, resource lookup, and style-object behavior follow the underlying WinUI object and resource system of the projected control tree.

A plain C# value expression such as `Background={Theme.CardBrush}` is not automatically equivalent to WinUI `ThemeResource` semantics.

If authors need live theme-reactive behavior, they SHOULD use ordinary WinUI resource objects, host-level theme services, or explicit rerender-triggering state rather than assuming that a plain value expression acquires automatic theme invalidation.

Hostless logical-tree rendering MAY preserve deferred theme, style, or resource values without resolving them until projection time.

Cross-platform host semantics are out of scope for this draft v1 specification. WinUI remains the authoritative rendering model.

### 16.6.2 DataContext Posture

CSXAML does not use `DataContext` as its primary component data-flow model.

For v1 authoring, the primary model is:

- typed component props
- explicit `State<T>`
- explicit `inject`

Native controls that rely on `DataContext` for their own behavior remain a WinUI interop concern. CSXAML does not synthesize a second binding-path system or promise that `DataContext`-driven native experiences "just work" without explicit host or control support.

When a supported native control exposes `DataContext` as an ordinary property, authors MAY set it explicitly like any other native property. CSXAML does not automatically project component props or `State<T>` values into `DataContext`, and it does not synthesize subtree binding conventions on the author's behalf.

Controls, templates, and third-party libraries that fundamentally depend on ambient `DataContext` behavior remain an interop boundary unless the host, metadata, or explicit author code bridges that gap.

### 16.6.3 Controlled Input Pattern

For interactive inputs, the normative v1 pattern is controlled input:

- the displayed value comes from props or state
- the control raises a typed change event
- component code writes the new value back into state explicitly
- rerender reprojects the latest value

For example:

```csharp
State<string> Name = new State<string>("Draft");

render <TextBox
    Text={Name.Value}
    OnTextChanged={text => Name.Value = text} />;
```

Built-in controlled input support in the current v1 slice includes:

- `TextBox.Text` with `OnTextChanged={Action<string>}`
- `CheckBox.IsChecked` with `OnCheckedChanged={Action<bool>}`

When the runtime reapplies a controlled value to a retained native control, it MUST compare the newly rendered value with the currently applied controlled value using the control adapter's equality rule and MUST NOT perform a redundant native assignment when the values are observably equal under that rule.

Suppressing feedback loops alone is not enough. When a controlled adapter does need to assign a new value to a retained interactive control, it SHOULD preserve focus, selection, and caret state where the underlying platform APIs make that practical.

CSXAML v1 does not guarantee that every IME or composition path is fully preserved across controlled writes. Implementations SHOULD avoid forcing projection-time writes during active composition when the platform exposes reliable composition state, and remaining gaps MUST be documented as compatibility limitations rather than implied away.

### 16.6.4 Deferred WinUI Interop Surfaces

The following areas are intentionally not claimed as fully abstracted by the v1 CSXAML language surface:

- large-list virtualization through `ListView`, `ItemsRepeater`, or similar template-driven controls
- `ControlTemplate`
- `VisualStateManager`
- `Storyboard` and named animation targets
- `Frame` and `Page` navigation abstractions
- `x:Name`-style symbolic targeting

These remain either direct WinUI interop concerns or future design work. The language text MUST NOT imply parity with them until dedicated design and implementation work lands.

### 16.6.5 Imperative Element Handles

Imperative element handles or `ref`-style access are important future design areas for interop with focus management, scrolling APIs, animations, and other imperative WinUI features.

They are not part of the normative v1 language surface yet and SHOULD be treated as explicit future work rather than assumed ambient capability.

### 16.7 Retained-Mode Reconciliation and Performance Boundary

CSXAML reconciles against its own managed logical and rendered tree, not by treating live WinUI controls as the source of truth.

The runtime SHOULD retain compatible native elements across rerenders when identity matches by control kind, key, and applicable sibling position rules.

Child collections SHOULD be patched in a way that preserves retained native children when only a subset of children changes or sibling order changes.

Ordinary reconciliation SHOULD NOT depend on reading back arbitrary live WinUI property state across the managed/unmanaged boundary as part of diffing.

This retained-mode model is appropriate for ordinary component trees and small-to-medium repeated UI. It is not a replacement for WinUI virtualization primitives on very large item surfaces.

`foreach` is a repeated-child construct, not a virtualization mechanism. Authors SHOULD NOT expect `foreach` over thousands of items to match the behavior or performance profile of `ListView`, `ItemsRepeater`, or other native virtualization-aware controls.

V1 does not promise parity with native XAML data-template and `INotifyCollectionChanged`-optimized large-list behavior.

A first-class virtualized collection abstraction is deliberately deferred rather than implied by `foreach`.

### 16.8 Runtime Application Model

Runtime native property application SHOULD use:

- generated metadata
- explicit adapter logic
- explicit conversion helpers

Runtime native property application SHOULD NOT use reflection as the primary hot-path mechanism.

For supported external controls, runtime creation and patching SHOULD flow through generated adapters or an explicit registration model derived from the same metadata.

A late-bound reflection fallback MAY exist for experiments or narrow escape hatches, but it SHOULD NOT define the core v1 runtime path.

### 16.8.1 Native Projection Failure Semantics

If native projection of a render pass throws, that projection pass aborts and the exception MUST surface through the runtime projection failure path.

When projection of a child subtree fails, later siblings in that failing native projection walk are not processed for that pass.

CSXAML v1 does not guarantee transactional rollback of in-place native mutations that were already applied before the failure occurred.

Implementations SHOULD leave the previously attached host root in place rather than clearing host content as implicit recovery, but they MAY still have partially updated retained native objects beneath that root because rollback is not guaranteed.

The last successfully committed logical tree remains the managed reconciliation baseline for later passes. A failing native projection pass may therefore leave a temporary mismatch between that logical baseline and partially mutated retained native objects until a later successful pass re-establishes convergence.

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
- explicit sibling keys SHOULD be unique within their parent reconciliation domain
- repeated renders SHOULD supply keys whenever identity matters

### 18.1.1 Matching Rules

Within a single parent, keyed children are matched by the tuple:

- element category (native or component)
- resolved control or component identity
- key value

Unkeyed children of the same category and identity are matched by relative order after keyed matches are removed from consideration.

Duplicate explicit keys among siblings in the same parent and element category are invalid.

When duplicate explicit keys are statically obvious from source, implementations SHOULD report them as compile-time diagnostics. When they are only known after evaluation, implementations MUST fail deterministically at runtime. They MUST NOT silently pick first-wins or last-wins reconciliation behavior.

If the key, element category, or resolved control/component identity changes, the subtree is replaced rather than preserved.

When a child instance is preserved by this matching, its component state and injected bindings are retained.

Implementations SHOULD update preserved native nodes in place rather than destroying and recreating them unless the host control contract requires replacement.

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

### 19.1.1 Diagnostic Categories

Implementations SHOULD distinguish at least these diagnostic classes:

- syntax diagnostics
- name-resolution and metadata-validation diagnostics
- C# semantic diagnostics projected back from generated code
- runtime activation diagnostics
- runtime projection diagnostics

### 19.1.2 Recovery and Source Mapping

Parser and validator implementations SHOULD continue after local failures when safe so a single pass can report multiple actionable diagnostics.

When a diagnostic originates from a direct, stable, author-written CSXAML span, the implementation MUST map that diagnostic back to the original `.csxaml` span.

When synthesis, lowering, Roslyn behavior, or generated helper code make the original span lossy or non-unique, the implementation SHOULD map the diagnostic back on a best-effort basis and MAY fall back to a generated-file location when no stable source span can be recovered.

Duplicate-name and ambiguity diagnostics SHOULD point at the conflicting source span and SHOULD mention the original conflicting declaration or candidate when that information is available.

### 19.2 Tooling Surface

The language design SHOULD naturally support:

- tag completion
- native prop and event completion
- component prop completion
- C#-aware completion and navigation inside helper code, `inject` declarations, `State<T>` declarations, and expression islands
- source squiggles
- go-to-definition
- formatting and indentation
- generated/source mapping

High-quality `.csxaml` authoring depends on a real language-service or LSP story, not just generated-code diagnostics after build.

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

### 20.4 Conformance Expectations

Implementations SHOULD maintain a conformance suite that covers at least:

- comments and trivia around `render <...>;`
- rejection of `return <...>;`
- rejection of `return ( <...> );`
- helper-code scanning across raw and interpolated raw strings that contain `render`
- `using static` support for ordinary C# lookup without participation in tag resolution
- nested generics, lambdas, interpolated strings, and raw strings inside C# islands
- ambiguous tag and import resolution
- slot misuse, slot-under-`foreach` rejection, and named-slot rejection
- duplicate sibling key rejection, including deterministic runtime failure for dynamic cases
- `Key` forwarding prohibition and keyed matching behavior
- attached-property owner resolution
- render-phase state-write failure behavior
- controlled input update flows, equality suppression, and retained text-input selection preservation where supported
- documented event-payload normalization behavior
- post-unmount async state-write behavior
- render and projection failure behavior for aborted passes
- direct versus best-effort source mapping of C# diagnostics back to `.csxaml`

### 20.5 Trim and AOT Posture

For the supported v1 feature slice, generated code and the intended runtime path SHOULD be trim-safe and AOT-compatible.

Metadata-driven control handling exists partly to support that goal.

Reflection MAY appear in clearly bounded interop or escape-hatch paths, but it SHOULD NOT define the ordinary hot-path model for built-in rendering, state invalidation, or retained reconciliation.

Future language and runtime features SHOULD be evaluated against trim and AOT compatibility rather than convenience alone.

---

## 21. Developer Experience Guidelines

The language should feel pleasant in everyday use.

### 21.1 Mental Model

The recommended mental model is:

- write structure in tags
- declare ambient app services explicitly with `inject`
- compute local values in ordinary C# helper code before the final render statement
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
- explicit component-level `inject` declarations
- explicit `State<T>`
- `render` of a markup root
- native and component tags
- string literal and `{...}` expression attribute values
- `if` child blocks
- constrained `foreach` child blocks
- metadata-driven native property and event validation
- metadata-driven common layout property support for the current curated controls
- metadata-driven `Style` support for built-in controls and the current supported external-control slice
- owner-qualified attached-property syntax for the built-in slice
- ordinary file-level namespace imports, aliases, and `using static` directives carried through generation and tooling; static imports participate only in ordinary C# lookup, not tag resolution
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
- basic retained-component lifecycle semantics including first mount, retained rerender, unmount disposal, post-unmount invalidation no-op behavior, and event-subscription rebinding on retained native elements
- controlled-input runtime support for the current built-in `TextBox` and `CheckBox` event/value surface, including no-op reapply suppression and `TextBox` selection/focus preservation for ordinary controlled updates
- default-slot validation including duplicate-slot rejection, root-slot rejection, attribute/child-content rejection, and `foreach`-slot rejection
- deterministic duplicate-key rejection for sibling native elements during projection and sibling component elements during component reconciliation

Known gaps relative to the intended v1 experience:

- whole-file comment support needs further hardening beyond the helper-code and boundary scanners
- external control namespace syntax, referenced-assembly discovery, and generated runtime registration are implemented for the current supported slice; [docs/external-control-interop.md](docs/external-control-interop.md) describes that slice and its current limitations, while broader external control shape coverage is still in progress
- the v1 styling story is intentionally thin and currently stops at reusable values plus WinUI `Style` support; richer styling constructs remain future work
- external attached-property owner discovery beyond the current loaded metadata slice is not yet implemented
- IME composition hardening for controlled text input is not yet complete across every WinUI input path
- controls, templates, and libraries that assume ambient `DataContext`, deep templating, or virtualization remain interop boundaries rather than first-class v1 CSXAML abstractions
- named slots, slot fallback content, and fragment-root slot pass-through are intentionally deferred
- dedicated source-level lifecycle hooks and per-instance cancellation syntax are not yet finished
- parser/validator diagnostics, direct source-authored build failures, deterministic source-map sidecars under `obj`, and staged runtime exception context are now implemented for the current slice; fuller debugger integration and broader IDE coverage are still ahead
- formatting support is not yet defined end to end
- repo-local project-system build behavior is now explicit and deterministic, but NuGet-delivered build packaging and richer IDE design-time tooling are still ahead

---

## 23. Example

This is representative of the language direction the project is aiming for:

```csharp
component Element TodoBoard {
    inject ITodoService todoService;

    State<List<TodoItemModel>> Items = new State<List<TodoItemModel>>(todoService.LoadItems());

    void ToggleItem(string itemId) {
        var updatedItems = Items.Value
            .Select(item => item.Id == itemId ? item with { IsDone = !item.IsDone } : item)
            .ToList();
        Items.Value = updatedItems;
        todoService.SaveItems(updatedItems);
    }

    render <StackPanel Spacing={12}>
        <TextBlock Text="Todo Board" FontSize={24} />
        foreach (var item in Items.Value) {
            <TodoCard
                Key={item.Id}
                Title={item.Title}
                IsDone={item.IsDone}
                OnToggle={() => ToggleItem(item.Id)} />
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
