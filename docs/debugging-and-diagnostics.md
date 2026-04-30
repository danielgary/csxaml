# Debugging and Diagnostics

CSXAML now tries hard to keep debugging source-first.

The normal path is:

1. look at the `.csxaml` diagnostic first
2. use the wrapped runtime message or mapped build error to identify the component, tag, and member
3. open generated code under `obj` only when you need the fallback lens

## Build Diagnostics

For source-authored C# regions, the generator emits narrow `#line` directives so normal compiler errors point back to the original `.csxaml` file.

That applies to:

- helper code before the final render statement
- state initializers
- component prop expressions
- native property and event expressions
- native `Ref={...}` expressions
- `if` conditions
- `foreach` collection expressions

Typical failures such as `MissingSymbol`, invalid member access, or bad delegate expressions should now land on the `.csxaml` line the author actually wrote.

## Source Maps

Each generated component also writes a sidecar map under:

```text
obj\<tfm>\Csxaml\Maps\
```

Each `.map.json` file records:

- the generated `.g.cs` path
- the original `.csxaml` path
- the component name
- generated-to-source regions for tags, props, helper code, control-flow blocks, and other emitted segments

These maps are deterministic and cleaned with the rest of the CSXAML intermediate output.

## Runtime Failures

Runtime exceptions are wrapped as `CsxamlRuntimeException` with staged frames.

Each frame can include:

- the render/projection stage
- the component name
- the source file and span
- the tag name
- the member name
- a short detail string

The original exception is preserved as the inner exception.

Typical stages include:

- `root render`
- `component render`
- `child component render`
- `root projection`
- `native element creation`
- `native element update`
- `native property read`
- `attached property application`
- `ref assignment`

## When To Open Generated Code

Generated code is still the fallback tool for cases where the failure is genuinely about synthetic scaffold code rather than the source-authored C# region.

Use this order:

1. start with the `.csxaml` diagnostic or runtime message
2. if the failure still points at generated code, open the matching `.map.json`
3. use the mapped region kind and label to find the nearest tag, property, or helper block
4. open the `.g.cs` file only for the final inspection step

Generated files remain intentionally boring and deterministic so that this last step is readable rather than mysterious.

## Known Limits

There are still narrow cases where the system does not pretend to know more than it does.

- internal scaffold or contract-drift compile failures may still point at generated `.g.cs`
- the sidecar maps can identify the nearest truthful source region, but they do not rewrite every possible compiler diagnostic
- this milestone improves debugging and provenance, not full debugger integration

If a failure stays attached to generated code, treat that as either an implementation bug in CSXAML or a case where the generated region is the only honest location to report.
