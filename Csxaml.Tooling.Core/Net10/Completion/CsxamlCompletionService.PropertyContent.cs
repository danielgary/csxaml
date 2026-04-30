using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.Completion;

public sealed partial class CsxamlCompletionService
{
    private static IReadOnlyList<CsxamlCompletionItem> CompletePropertyContentTags(
        CsxamlMarkupContext context,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        var ownerName = context.PropertyContentOwner ?? throw new InvalidOperationException(
            "Property-content completion requires an owner tag.");
        var resolvedTag = new CsxamlTagSymbolResolver().Resolve(
            ownerName,
            usingDirectives,
            currentNamespace,
            workspace);
        if (resolvedTag.Control is not null)
        {
            return Order(GetNativePropertyContentItems(ownerName, resolvedTag.Control, context.PrefixText));
        }

        if (resolvedTag.Component is not null)
        {
            return Order(
                resolvedTag.Component.Metadata.NamedSlots
                    .Where(slot => MatchesPrefix(slot.Name, context.PrefixText))
                    .Select(slot => CsxamlMarkupCompletionItemFactory.CreatePropertyContentItem(
                        ownerName,
                        slot.Name,
                        $"Named slot on {resolvedTag.Component.Metadata.Name}")));
        }

        return Array.Empty<CsxamlCompletionItem>();
    }

    private static IEnumerable<CsxamlCompletionItem> GetNativePropertyContentItems(
        string ownerName,
        Csxaml.ControlMetadata.ControlMetadata control,
        string prefix)
    {
        var items = new List<CsxamlCompletionItem>();
        if (!string.IsNullOrWhiteSpace(control.Content.DefaultPropertyName) &&
            control.Content.Kind != ControlContentKind.None &&
            MatchesPrefix(control.Content.DefaultPropertyName, prefix))
        {
            items.Add(CsxamlMarkupCompletionItemFactory.CreatePropertyContentItem(
                ownerName,
                control.Content.DefaultPropertyName,
                $"{control.Content.Kind} content property"));
        }

        items.AddRange(
            control.Properties
                .Where(IsPropertyContentCandidate)
                .Where(property => MatchesPrefix(property.Name, prefix))
                .Select(property => CsxamlMarkupCompletionItemFactory.CreatePropertyContentItem(
                    ownerName,
                    property.Name,
                    property.ClrTypeName)));
        return items;
    }

    private static bool IsPropertyContentCandidate(PropertyMetadata property)
    {
        return property.ValueKindHint == ValueKindHint.Object ||
            string.Equals(
                property.ClrTypeName,
                "Microsoft.UI.Xaml.Controls.UIElementCollection",
                StringComparison.Ordinal);
    }
}
