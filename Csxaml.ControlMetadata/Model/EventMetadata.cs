namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a native event that can be assigned from CSXAML markup.
/// </summary>
/// <param name="ClrEventName">The CLR event name to subscribe to, or <see langword="null"/> for synthetic bindings.</param>
/// <param name="ExposedName">The event name exposed to CSXAML markup.</param>
/// <param name="HandlerTypeName">The fully qualified CLR delegate type expected by the event.</param>
/// <param name="ExposedInCsxaml">A value indicating whether the event is intended to be used directly in CSXAML.</param>
/// <param name="ValueKindHint">A hint used by tooling and emitters when validating event values.</param>
/// <param name="BindingKind">The generated binding shape used for the event.</param>
public sealed record EventMetadata(
    string? ClrEventName,
    string ExposedName,
    string HandlerTypeName,
    bool ExposedInCsxaml,
    ValueKindHint ValueKindHint,
    EventBindingKind BindingKind);
