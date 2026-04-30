namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes the default child-content property for a native control.
/// </summary>
/// <param name="DefaultPropertyName">The property that receives default child content, or <see langword="null"/>.</param>
/// <param name="Kind">The child-content shape supported by the property.</param>
/// <param name="PropertyTypeName">The CLR type name of the content property, when known.</param>
/// <param name="ItemTypeName">The CLR item type name for collection content, when known.</param>
/// <param name="Source">The source used to discover the content metadata.</param>
public sealed record ControlContentMetadata(
    string? DefaultPropertyName,
    ControlContentKind Kind,
    string? PropertyTypeName,
    string? ItemTypeName,
    ControlContentSource Source)
{
    /// <summary>
    /// Gets metadata for a control that does not accept default child content.
    /// </summary>
    public static ControlContentMetadata None { get; } =
        new(null, ControlContentKind.None, null, null, ControlContentSource.Unknown);

    /// <summary>
    /// Creates compatibility metadata from the older child-kind model.
    /// </summary>
    /// <param name="childKind">The older child-kind value.</param>
    /// <returns>Equivalent content metadata without a known property name.</returns>
    public static ControlContentMetadata FromChildKind(ControlChildKind childKind)
    {
        return childKind switch
        {
            ControlChildKind.None => None,
            ControlChildKind.Single => new(
                null,
                ControlContentKind.Single,
                null,
                null,
                ControlContentSource.Unknown),
            ControlChildKind.Multiple => new(
                null,
                ControlContentKind.Collection,
                null,
                null,
                ControlContentSource.Unknown),
            _ => throw new ArgumentOutOfRangeException(nameof(childKind), childKind, null)
        };
    }

    /// <summary>
    /// Converts the detailed content shape into the legacy child-kind value.
    /// </summary>
    /// <returns>The equivalent legacy child kind.</returns>
    public ControlChildKind ToChildKind()
    {
        return Kind switch
        {
            ControlContentKind.None => ControlChildKind.None,
            ControlContentKind.Single => ControlChildKind.Single,
            ControlContentKind.Collection => ControlChildKind.Multiple,
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, null)
        };
    }
}
