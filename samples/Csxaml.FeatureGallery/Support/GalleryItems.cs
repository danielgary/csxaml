namespace Csxaml.Samples.FeatureGallery;

public sealed record GalleryListItem(string Id, string Title, string Detail)
{
    public override string ToString()
    {
        return Title;
    }
}

public static class GalleryItems
{
    public static IReadOnlyList<GalleryListItem> SmallRows { get; } =
    [
        new("state", "State and controlled inputs", "Use State<T> as the source of truth."),
        new("slots", "Default and named slots", "Compose caller-provided child content."),
        new("refs", "Element refs", "Reach projected controls for focus or navigation."),
        new("events", "Typed event args", "Handle WinUI event args without sender plumbing.")
    ];

    public static IReadOnlyList<string> EventItems { get; } =
    [
        "SelectionChanged",
        "ItemClick",
        "ValueChanged",
        "QuerySubmitted",
        "PointerEntered"
    ];

    public static IReadOnlyList<string> VirtualizedRows { get; } =
        Enumerable.Range(1, 200)
            .Select(index => $"Virtualized row {index:000}")
            .ToArray();
}
