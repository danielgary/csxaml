namespace Csxaml.Tooling.Core.CSharp;

internal readonly record struct CsxamlProjectedPropertyType(string TypeName, string? CoercionMethodName)
{
    public static CsxamlProjectedPropertyType Plain(string typeName)
    {
        return new(typeName, null);
    }
}
