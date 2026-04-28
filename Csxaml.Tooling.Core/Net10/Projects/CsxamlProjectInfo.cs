namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Describes project metadata used by CSXAML tooling services.
/// </summary>
/// <param name="ProjectFilePath">The full project file path.</param>
/// <param name="ProjectDirectory">The full directory path that contains the project file.</param>
/// <param name="AssemblyName">The project assembly name.</param>
/// <param name="DefaultNamespace">The project root namespace used when no CSXAML namespace directive is present.</param>
/// <param name="TargetFramework">The target framework used to locate build outputs.</param>
public sealed record CsxamlProjectInfo(
    string ProjectFilePath,
    string ProjectDirectory,
    string AssemblyName,
    string DefaultNamespace,
    string TargetFramework);
