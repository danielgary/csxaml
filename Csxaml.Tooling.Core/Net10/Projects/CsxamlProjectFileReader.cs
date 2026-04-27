using System.Xml.Linq;

namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Reads basic CSXAML tooling metadata from an SDK-style project file.
/// </summary>
public static class CsxamlProjectFileReader
{
    /// <summary>
    /// Reads project metadata from a project file.
    /// </summary>
    /// <param name="projectFilePath">The project file path to read.</param>
    /// <returns>The project metadata needed by CSXAML tooling services.</returns>
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
