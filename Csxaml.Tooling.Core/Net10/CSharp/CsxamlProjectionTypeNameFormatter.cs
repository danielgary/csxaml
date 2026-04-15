namespace Csxaml.Tooling.Core.CSharp;

internal static class CsxamlProjectionTypeNameFormatter
{
    public static string Format(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return "object";
        }

        return typeName.Contains('.', StringComparison.Ordinal) &&
               !typeName.StartsWith("global::", StringComparison.Ordinal)
            ? $"global::{typeName}"
            : typeName;
    }
}
