namespace Csxaml.Runtime;

/// <summary>
/// Represents a CSXAML element reference.
/// </summary>
/// <remarks>
/// Author code should usually use <see cref="ElementRef{TElement}"/>. This
/// non-generic base lets generated code accept any typed element reference while
/// preserving runtime target-type validation.
/// </remarks>
public abstract class ElementRef
{
    private protected ElementRef()
    {
    }

    internal abstract object? CurrentObject { get; }

    internal abstract Type ElementType { get; }

    internal abstract void Assign(object element, CsxamlSourceInfo? sourceInfo);

    internal abstract void Clear();

    internal abstract void ClearIfCurrent(object element);
}
