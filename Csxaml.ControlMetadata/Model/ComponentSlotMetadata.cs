namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes a named slot accepted by a compiled CSXAML component.
/// </summary>
/// <param name="Name">The named slot used by property-content call-site syntax.</param>
public sealed record ComponentSlotMetadata(string Name);
