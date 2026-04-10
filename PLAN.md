Milestone 3 - Metadata-Driven Native Props and Stronger Component Props
Objective

Implement a metadata-driven property system for native controls and tighten the generated component prop model so CSXAML can:

expose a broader set of native control properties in markup
expose native events in a consistent way
validate props/events against control metadata
generate runtime nodes from generic attributes rather than hardcoded control-specific constructors
pass typed props from parent components to child components cleanly
patch supported native properties at runtime
update the Todo demo to support green/red visual state for done/not done cards

This milestone is complete when the Todo demo expresses card coloring through CSXAML props using the new metadata-driven native property system, and component props are generated/passed in a strongly typed and maintainable way.

Non-Negotiable Constraints
Keep using C# for generator, runtime, and metadata tooling.
Do not use runtime reflection as the primary runtime property application mechanism.
Use reflection at build time to generate stable metadata tables.
Do not expose all WinUI surface area blindly. Discover broadly, then filter to a curated supported subset.
Do not expand the syntax with unrelated features.
Do not collapse parser, validator, metadata generation, emitter, and runtime patching into large files.
Keep event metadata separate from ordinary property metadata.
Preserve explicit framework-reserved props such as Key outside the native prop bag.
Prefer a metadata-driven generic native element model over endlessly expanding specialized constructors.
Keep the Todo demo simple. Do not turn Milestone 3 into a styling framework milestone.
Core Success Criteria

Milestone 3 is complete only when all of these are true:

Native markup attributes are parsed generically for supported native elements.
A reflection-based metadata generation step produces stable control/property/event metadata tables.
Generator validation uses those metadata tables to validate native props and native events.
Runtime uses emitted prop/event data plus control adapters to create and patch native controls.
Parent components pass typed props to child components using generated prop types or equivalent strongly typed structures.
Native events are exposed consistently, such as OnClick.
The Todo demo updates TodoCard visuals using color/styling props:
green when done
red when not done
Invalid props surface clear diagnostics during generation/build.
Code remains split into small, understandable files.
Scope
In Scope
generic attribute parsing for native elements
reflection-based metadata generation for a curated set of WinUI controls
validation of native props and events using metadata tables
generic native property/event bags in runtime nodes
runtime control adapters that create and patch supported properties/events
typed component props generation and parent-to-child prop passing cleanup
Todo demo visual update using native props for color/styling
tests for metadata, validation, prop passing, and runtime property application
Out of Scope
complete WinUI property parity
arbitrary object graph construction from markup
advanced converters for every WinUI type
styling DSL
resource dictionaries
data templates
implicit styles
triggers/visual states
Roslyn source generators
IDE tooling implementation
full language server implementation
runtime reflection-driven property patching in the hot path
broad theme system
Key Design Decisions

1. Reflection is a metadata source, not the runtime execution model

Reflection should be used in a build-time metadata generator to inspect selected WinUI types and emit stable metadata tables.

Runtime property application should use those metadata tables and explicit control adapters.

2. Native elements should use a generic prop/event model

Do not keep expanding specialized node constructors like:

TextBlockNode(string Text)
ButtonNode(string Content, Action OnClick)

Move toward a generic native element model:

native type/tag name
reserved framework props like Key
property bag
event bag
children 3. Component props stay strongly typed

Do not move component props to dynamic dictionaries.

Each generated component should still have strongly typed props or equivalent generated parameter handling.

4. Native props and native events are distinct

A native event is not just another property.

Treat them separately in metadata, validation, emission, and runtime binding.

5. Support a curated native control set first

Metadata generation should focus first on a small set of controls needed now:

StackPanel
Border if needed for card coloring
TextBlock
Button

Optional if already present or trivial to support:

TextBox
CheckBox

Do not attempt all WinUI controls in this milestone.

Architectural Changes Required

1. Add a metadata generation project or assembly

Create a new project or clear sub-area responsible for generating control metadata from reflection.

Recommended name:

Csxaml.ControlMetadata.Generator

This tool should inspect selected WinUI types and emit a stable artifact consumed by generator/runtime.

2. Introduce a shared metadata model

Create a shared model for:

control metadata
property metadata
event metadata
type/conversion hints

This model should be consumed by:

metadata generator
code generator validator
runtime control adapter layer
future tooling 3. Update the parser/AST to treat native attributes generically

For native elements, parser should capture attributes generically rather than hardcoding per-control props.

4. Add a validation layer based on metadata tables

The generator should validate whether a given native property or event is supported for a given native tag.

5. Replace or evolve native node emission

Generated code should emit generic native prop/event structures rather than specialized constructor arguments only.

6. Add runtime control adapters

Runtime should create and patch native controls using adapter classes informed by supported metadata.

7. Tighten component prop generation

If Milestone 2 introduced typed props, this milestone should clean up any awkwardness and standardize component prop generation/emission so parent-to-child passing is obvious and maintainable.

Required Metadata Model

Create a stable metadata representation with at least the following concepts.

ControlMetadata

Fields should include:

TagName
ClrTypeName
BaseTypeName or inheritance chain
Properties
Events
PropertyMetadata

Fields should include:

Name
ClrTypeName
IsWritable
IsDependencyProperty if discoverable
IsAttached
ExposedInCsxaml
ValueKindHint
EventMetadata

Fields should include:

ClrEventName
ExposedName such as OnClick
HandlerTypeName
ExposedInCsxaml
ValueKindHint

Include enough hints for validation/coercion planning, such as:

String
Bool
Int
Double
Enum
Object
Brush
Thickness
Unknown

This does not need to solve all conversions. It only needs enough for the supported milestone surface.

Metadata Generation Requirements
Inputs

A curated list of WinUI control CLR types, such as:

Microsoft.UI.Xaml.Controls.StackPanel
Microsoft.UI.Xaml.Controls.TextBlock
Microsoft.UI.Xaml.Controls.Button
Microsoft.UI.Xaml.Controls.Border if needed
Discovery behavior

Use reflection to inspect:

public writable CLR properties
public events
optionally dependency properties when easy to correlate
Filtering behavior

The generator must not expose everything automatically.

It should filter to a supported subset using rules such as:

public writable property
declarative-safe
not explicitly excluded
type is supported or allowed as Object
event is supported for the current runtime/event model
Outputs

Emit one stable metadata artifact.

Recommended output options:

generated C# metadata tables in a shared assembly
JSON metadata artifact loaded by generator/runtime

Preferred for Milestone 3: generated C# source because it is easy to consume from both generator/runtime without additional loading complexity.

Syntax and Parsing Rules
Native attributes

The parser should capture native element attributes generically.

Example:

<Button
    Content="Toggle"
    Background={SomeBrush}
    Foreground={SomeOtherBrush}
    FontSize={18}
    OnClick={Toggle} />

Parser should capture:

tag name
attribute list
for each attribute:
name
value kind
raw value / parsed expression payload
Attribute value kinds

Support at least:

string literal: "Done"
expression: {item.IsDone ? DoneBrush : NotDoneBrush}
numeric literal inside expression is acceptable
boolean expression via {...}

Do not add extra literal syntaxes unless already present.

Reserved props

Keep framework-reserved props separate from native props:

Key
any future framework-only reserved names

Reserved props should not flow into the native property bag.

Event naming

Native events exposed in CSXAML should use normalized names such as:

OnClick

The metadata layer should map:

OnClick -> WinUI Click
Component Props Requirements

Milestone 3 must also standardize component prop passing.

Requirements
Each component with parameters must generate a stable typed props structure or equivalent.
Parent components must pass child component props through generated strongly typed construction, not dynamic bags.
Generator output for component prop passing should be readable and deterministic.
Validation should catch:
missing required props
duplicate props
unknown props on component tags
Preferred shape

A generated props record is acceptable and recommended:

public sealed record TodoCardProps(
string Title,
bool IsDone,
Action OnToggle
);

And generated parent usage should lower cleanly into that props type.

If your current shape is already similar, keep it and tighten it rather than redesigning for novelty.

Runtime Model Changes
Native element node

Introduce or standardize a generic native element node shape.

It should contain at least:

TagName
Key
Properties
Events
Children

Example conceptual shape:

public sealed class NativeElementNode : Node
{
public required string TagName { get; init; }
public string? Key { get; init; }
public IReadOnlyList<NativePropValue> Properties { get; init; } = [];
public IReadOnlyList<NativeEventValue> Events { get; init; } = [];
public IReadOnlyList<Node> Children { get; init; } = [];
}
Prop values

Each property should capture:

Name
Value
optional ValueKindHint
Event values

Each event should capture:

exposed name such as OnClick
handler delegate
Component nodes

Keep component nodes strongly typed and separate from native element nodes.

Runtime Control Adapter Layer

Add a control adapter layer that owns native control creation and patching.

Responsibilities

For each supported native tag:

create native control instance
apply supported properties
wire supported events
patch changed properties during reconciliation
patch or rebind events safely
Recommended shape

One adapter per supported control type, for example:

ButtonControlAdapter
TextBlockControlAdapter
StackPanelControlAdapter
BorderControlAdapter

Or a small adapter registry plus per-type implementations.

Do not do this

Do not put all property application in one massive switch statement file.

Validation Requirements

Generator validation must use metadata tables to validate native tags, props, and events.

Validate
known native tag
prop exists on supported control metadata
event exists on supported control metadata
reserved props are used only where legal
duplicate native props are rejected
duplicate events are rejected
Error examples
unknown prop on native control
unsupported event on control
reserved prop misused
invalid component prop
duplicate attribute name

Diagnostics can be plain build errors for now.

Todo Demo Changes

Update the Todo demo so cards visually reflect done/not done.

Goal

Use the new native property system to pass color/styling props in CSXAML.

Recommended implementation

Use Border around each todo card and set its background or border brush based on IsDone.

Done card
green background or green border
Not done card
red background or red border

Keep it simple and obvious.

Example target authoring shape

TodoCard.csxaml should be able to express something like:

component Element TodoCard(string Title, bool IsDone, Action OnToggle) {
return <Border Background={IsDone ? TodoColors.DoneBackground : TodoColors.NotDoneBackground}>
<StackPanel>
<TextBlock Text={Title} Foreground={TodoColors.CardForeground} />
if (IsDone) {
<TextBlock Text="Done" Foreground={TodoColors.DoneForeground} />
}
if (!IsDone) {
<TextBlock Text="Not Done" Foreground={TodoColors.NotDoneForeground} />
}
<Button Content="Toggle" OnClick={OnToggle} />
</StackPanel>
</Border>;
}

Exact syntax can vary slightly depending on your current grammar, but the demo must visibly show green/red based on state.

Supporting code

It is acceptable to add a small C# helper such as TodoColors with static brushes if brush literal syntax is not yet supported.

That is preferable to inventing a brush-construction language in this milestone.

Implementation Phases
Phase 1 - Introduce control metadata generator
Deliverables
new metadata generation project or clear module
reflection over curated WinUI control list
generated metadata artifact
shared metadata model
Acceptance criteria
metadata tables are generated deterministically
metadata can answer:
what props exist for Button
what events exist for Button
what props exist for Border
what props exist for TextBlock
Phase 2 - Update parser and AST for generic native attributes
Deliverables
native element attributes parsed generically
reserved props separated conceptually
attribute value kinds preserved
Acceptance criteria
native element tags can carry arbitrary supported attributes in syntax
parser no longer depends on control-specific prop hardcoding for supported native tags
Phase 3 - Add metadata-driven validation
Deliverables
validator consumes metadata tables
native prop validation
native event validation
native tag validation
duplicate attribute checks
Acceptance criteria
invalid native props fail build with clear diagnostics
invalid native events fail build with clear diagnostics
valid native props/events pass
Phase 4 - Update emission model for generic native props/events
Deliverables
generated code emits generic native prop structures
generated code emits generic native event structures
reserved props like Key handled separately
component prop emission standardized/tightened
Acceptance criteria
generated code remains readable and deterministic
parent-to-child component prop passing is strongly typed and clear
native element emission no longer depends only on specialized constructor signatures
Phase 5 - Add runtime control adapter layer
Deliverables
adapter registry
adapters for:
StackPanel
TextBlock
Button
Border if used in the Todo demo
property application logic
event binding logic
minimal patch/update logic for supported properties
Acceptance criteria
supported native properties apply correctly
supported events bind correctly
rerender/patch works for changed supported props
Phase 6 - Update Todo demo
Deliverables
Todo demo updated to use native prop syntax for visual styling
done/not done cards clearly green/red
TodoColors helper or similar if needed
no hand-authored custom rendering path outside the normal CSXAML flow
Acceptance criteria
green when done
red when not done
toggle still works
demo is expressed through the new metadata-driven prop model
Phase 7 - Tests and cleanup
Deliverables
metadata generation tests
validator tests
emitter snapshot/golden tests
runtime property application tests
component prop passing tests
todo demo smoke test if available
Acceptance criteria
regressions in metadata/validation/emission are covered
bug fixes follow failing-test-first discipline
Required Project/File Structure

Keep responsibilities split.

Suggested project layout
/Csxaml.ControlMetadata
/Model
ControlMetadata.cs
PropertyMetadata.cs
EventMetadata.cs
ValueKindHint.cs

/Csxaml.ControlMetadata.Generator
/Discovery
/Filtering
/Emission

/Csxaml.Generator
/Cli
/Syntax
/Parsing
/Ast
/Validation
/Emission
/Diagnostics

/Csxaml.Runtime
/Nodes
/Components
/Rendering
/Adapters
/Hosting
/Reconciliation

/Csxaml.Demo
/Components
/Models
/Support

If you do not want a separate metadata project, at least keep metadata model and metadata generation clearly separated from runtime and generator logic.

File-Size and Maintainability Rules

Because maintainability is already a concern, enforce these during implementation:

preferred file size: under 200 lines
warning threshold: 300 lines
hard stop: 400 lines

Do not allow:

tokenizer + parser + validator in one file
metadata discovery + filtering + emission in one file
runtime node definitions + adapter logic + control patching in one file

Split early.

Testing Requirements
Metadata generator tests
discovers expected props for curated controls
discovers expected events for curated controls
filtering behaves as expected
generated metadata artifact is stable
Generator/validator tests
valid native props accepted
invalid native props rejected
valid native events accepted
invalid native events rejected
reserved props handled correctly
component prop passing validates correctly
Emitter tests
generated native prop/event emission snapshots
generated component prop emission snapshots
Runtime tests
adapter creates correct native control type
supported properties apply
supported properties patch correctly
events bind and rebind safely
Demo-level tests
todo card green when done
todo card red when not done
toggle preserves expected visual updates
Stop Conditions

The agent must stop and refactor before continuing if any of these happen:

Reflection logic is being copied into runtime hot paths.
Native props and native events are being treated as one undifferentiated bag.
Component props are drifting toward dynamic dictionaries.
Property application is becoming one giant switch file.
Metadata generation, validation, and runtime application are being mixed together.
The Todo demo change requires special-case code outside the normal CSXAML pipeline.
Files grow into large, multi-responsibility implementations.
Definition of Done

Milestone 3 is done when:

reflection-based metadata tables exist for the supported native control set
parser captures native attributes generically
validator uses metadata tables for native prop/event validation
generated code emits generic native prop/event structures
runtime uses control adapters to create/patch native controls
parent-to-child component prop passing is strongly typed and clean
Todo cards visually show green for done and red for not done using CSXAML props
invalid native props/events fail with clear diagnostics
the implementation remains split into small, understandable files
Final Instruction to the Agent

Implement this as a focused infrastructure milestone. The primary goal is to establish a maintainable metadata-driven native property system and solid component prop passing. Do not broaden the language unnecessarily. Do not chase full WinUI coverage. Use the Todo demo as the proof that the new native prop system works by coloring cards green/red based on done state.

If you want, I can also turn this into a tighter implementation-plan.md with explicit file-by-file deliverables and suggested commit breakdowns.s
