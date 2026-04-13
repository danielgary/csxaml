namespace Csxaml.Runtime;

internal sealed class RenderedChildMatcher
{
    private readonly Dictionary<string, RenderedNativeElement> _keyedChildren = new(StringComparer.Ordinal);
    private readonly HashSet<RenderedNativeElement> _matchedChildren = new(ReferenceEqualityComparer.Instance);
    private readonly IReadOnlyList<RenderedNativeElement> _previousChildren;
    private readonly Queue<RenderedNativeElement> _unkeyedChildren = new();

    public RenderedChildMatcher(IReadOnlyList<RenderedNativeElement> previousChildren)
    {
        _previousChildren = previousChildren;

        foreach (var child in previousChildren)
        {
            if (child.Key is null)
            {
                _unkeyedChildren.Enqueue(child);
                continue;
            }

            _keyedChildren.TryAdd(child.Key, child);
        }
    }

    public void DisposeUnmatched()
    {
        foreach (var child in _previousChildren)
        {
            if (_matchedChildren.Contains(child))
            {
                continue;
            }

            child.Dispose();
        }
    }

    public RenderedNativeElement? TakeMatch(NativeElementNode node)
    {
        // Keyed siblings match by key regardless of index.
        // Unkeyed siblings consume the next unkeyed slot in order.
        return node.Key is null
            ? TakeUnkeyedMatch(node)
            : TakeKeyedMatch(node);
    }

    private RenderedNativeElement? TakeKeyedMatch(NativeElementNode node)
    {
        if (!_keyedChildren.Remove(node.Key!, out var match))
        {
            return null;
        }

        if (!TagMatches(match, node))
        {
            return null;
        }

        _matchedChildren.Add(match);
        return match;
    }

    private RenderedNativeElement? TakeUnkeyedMatch(NativeElementNode node)
    {
        if (_unkeyedChildren.Count == 0)
        {
            return null;
        }

        var match = _unkeyedChildren.Dequeue();
        if (!TagMatches(match, node))
        {
            return null;
        }

        _matchedChildren.Add(match);
        return match;
    }

    private static bool TagMatches(RenderedNativeElement existing, NativeElementNode node)
    {
        return string.Equals(existing.TagName, node.TagName, StringComparison.Ordinal);
    }
}
