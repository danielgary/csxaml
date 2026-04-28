using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

/// <summary>
/// Resolves a WinUI style lazily from application resources or a fallback factory.
/// </summary>
public sealed class DeferredStyle
{
    private readonly Func<Style> _fallbackFactory;
    private readonly string? _resourceKey;
    private Style? _resolvedStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredStyle"/> class with a fallback style factory.
    /// </summary>
    /// <param name="fallbackFactory">The factory used when no resource lookup is required.</param>
    public DeferredStyle(Func<Style> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        _fallbackFactory = fallbackFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredStyle"/> class with a resource key and fallback style factory.
    /// </summary>
    /// <param name="resourceKey">The application resource key to resolve first.</param>
    /// <param name="fallbackFactory">The factory used when the resource key is not available.</param>
    public DeferredStyle(string resourceKey, Func<Style> fallbackFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceKey);
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        _resourceKey = resourceKey;
        _fallbackFactory = fallbackFactory;
    }

    /// <summary>
    /// Resolves and caches the style instance.
    /// </summary>
    /// <returns>The resolved resource style or fallback style.</returns>
    public Style Resolve()
    {
        if (_resolvedStyle is not null)
        {
            return _resolvedStyle;
        }

        if (TryResolveResourceStyle(out var resourceStyle))
        {
            return _resolvedStyle = resourceStyle;
        }

        _resolvedStyle = _fallbackFactory();
        return _resolvedStyle;
    }

    private bool TryResolveResourceStyle([NotNullWhen(true)] out Style? style)
    {
        style = null;
        if (string.IsNullOrEmpty(_resourceKey))
        {
            return false;
        }

        try
        {
            style = Application.Current?.Resources[_resourceKey] as Style;
            return style is not null;
        }
        catch (Exception exception) when (exception is ArgumentException or COMException)
        {
            return false;
        }
    }
}
