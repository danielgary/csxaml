namespace Csxaml.Tooling.Core.Markup;

public static partial class CsxamlMarkupContextAnalyzer
{
    private static bool IsTagNameCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character is '_' or ':' or '.';
    }

    private static bool TrySplitPropertyContent(
        string tagText,
        out string ownerName,
        out string propertyPrefix)
    {
        var dotIndex = tagText.LastIndexOf('.');
        if (dotIndex <= 0)
        {
            ownerName = string.Empty;
            propertyPrefix = string.Empty;
            return false;
        }

        ownerName = tagText[..dotIndex];
        propertyPrefix = tagText[(dotIndex + 1)..];
        return true;
    }

    private static CsxamlMarkupContext None()
    {
        return new CsxamlMarkupContext(
            CsxamlMarkupContextKind.None,
            string.Empty,
            null,
            null,
            Array.Empty<string>(),
            null);
    }

    private sealed record TagState(
        CsxamlMarkupContextKind Kind,
        string PrefixText,
        string? Qualifier,
        string? TagName,
        IReadOnlyList<string> ExistingAttributes,
        string? PropertyContentOwner);
}
