using System.Diagnostics.CodeAnalysis;

namespace Csxaml.Runtime;

/// <summary>
/// Holds a typed reference to a projected native element.
/// </summary>
/// <typeparam name="TElement">The native element type expected by the reference.</typeparam>
public sealed class ElementRef<TElement> : ElementRef
    where TElement : class
{
    private TElement? _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementRef{TElement}"/> class.
    /// </summary>
    public ElementRef()
    {
    }

    /// <summary>
    /// Gets the currently projected native element, or <see langword="null"/> when the element is not mounted.
    /// </summary>
    public TElement? Current => _current;

    internal override object? CurrentObject => _current;

    internal override Type ElementType => typeof(TElement);

    /// <summary>
    /// Gets the current projected native element when it is mounted.
    /// </summary>
    /// <param name="element">The current element when this method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the reference currently points at a projected element.</returns>
    public bool TryGet([NotNullWhen(true)] out TElement? element)
    {
        element = _current;
        return element is not null;
    }

    internal override void Assign(object element, CsxamlSourceInfo? sourceInfo)
    {
        if (element is TElement typedElement)
        {
            _current = typedElement;
            return;
        }

        throw CsxamlRuntimeExceptionBuilder.Wrap(
            new InvalidOperationException(
                $"ElementRef expected '{typeof(TElement).FullName}' but received '{element.GetType().FullName}'."),
            "ref assignment",
            sourceInfo: sourceInfo);
    }

    internal override void Clear()
    {
        _current = null;
    }

    internal override void ClearIfCurrent(object element)
    {
        if (ReferenceEquals(_current, element))
        {
            _current = null;
        }
    }
}
