using System.Xml.Linq;

namespace Csxaml.Tooling.Core.Projects;

public static class CsxamlProjectFileReader
{
    public static CsxamlProjectInfo Read(string projectFilePath)
    {
        var document = XDocument.Load(projectFilePath);
        var propertyGroups = document.Root?.Elements().Where(element => element.Name.LocalName == "PropertyGroup").ToList()
            ?? new List<XElement>();

        var assemblyName = ReadProperty(propertyGroups, "AssemblyName")
            ?? Path.GetFileNameWithoutExtension(projectFilePath);
        var defaultNamespace = ReadProperty(propertyGroups, "RootNamespace")
            ?? assemblyName;
        var targetFramework = ReadProperty(propertyGroups, "TargetFramework")
            ?? "net10.0";

        return new CsxamlProjectInfo(
            projectFilePath,
            Path.GetDirectoryName(projectFilePath) ?? Directory.GetCurrentDirectory(),
            assemblyName,
            defaultNamespace,
            targetFramework);
    }

    private static string? ReadProperty(IEnumerable<XElement> propertyGroups, string propertyName)
    {
        return propertyGroups
            .SelectMany(group => group.Elements())
            .FirstOrDefault(element => element.Name.LocalName == propertyName)?
            .Value
            .Trim();
    }
}
