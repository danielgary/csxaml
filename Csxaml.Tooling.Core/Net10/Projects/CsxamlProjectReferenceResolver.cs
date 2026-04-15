using System.Xml.Linq;

namespace Csxaml.Tooling.Core.Projects;

public static class CsxamlProjectReferenceResolver
{
    public static IReadOnlyList<CsxamlProjectInfo> ResolveTransitive(CsxamlProjectInfo project)
    {
        var results = new List<CsxamlProjectInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Visit(project.ProjectFilePath, results, seen);
        return results;
    }

    private static void Visit(
        string projectFilePath,
        ICollection<CsxamlProjectInfo> results,
        ISet<string> seen)
    {
        foreach (var referencePath in ReadReferencePaths(projectFilePath))
        {
            var fullPath = Path.GetFullPath(referencePath);
            if (!seen.Add(fullPath) || !File.Exists(fullPath))
            {
                continue;
            }

            var info = CsxamlProjectFileReader.Read(fullPath);
            results.Add(info);
            Visit(fullPath, results, seen);
        }
    }

    private static IReadOnlyList<string> ReadReferencePaths(string projectFilePath)
    {
        var document = XDocument.Load(projectFilePath);
        var projectDirectory = Path.GetDirectoryName(projectFilePath) ?? Directory.GetCurrentDirectory();
        if (document.Root is null)
        {
            return Array.Empty<string>();
        }

        return document.Root
            .Elements()
            .Where(element => element.Name.LocalName == "ItemGroup")
            .SelectMany(group => group.Elements())
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFullPath(Path.Combine(projectDirectory, value!)))
            .ToList();
    }
}
