using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

public sealed class DeferredStyle
{
    private readonly Func<Style> _fallbackFactory;
    private readonly string? _resourceKey;
    private Style? _resolvedStyle;

    public DeferredStyle(Func<Style> fallbackFactory)
    {
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        _fallbackFactory = fallbackFactory;
    }

    public DeferredStyle(string resourceKey, Func<Style> fallbackFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceKey);
        ArgumentNullException.ThrowIfNull(fallbackFactory);
        _resourceKey = resourceKey;
        _fallbackFactory = fallbackFactory;
    }

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
