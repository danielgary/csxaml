namespace Csxaml.Generator;

internal static class PropertyContentName
{
    public static bool TrySplit(string tagName, out string ownerName, out string propertyName)
    {
        var dotIndex = tagName.LastIndexOf('.');
        if (dotIndex <= 0 || dotIndex == tagName.Length - 1)
        {
            ownerName = string.Empty;
            propertyName = string.Empty;
            return false;
        }

        ownerName = tagName[..dotIndex];
        propertyName = tagName[(dotIndex + 1)..];
        return true;
    }
}
