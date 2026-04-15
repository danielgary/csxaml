namespace Csxaml.Tooling.Core.Projects;

public sealed record CsxamlProjectInfo(
    string ProjectFilePath,
    string ProjectDirectory,
    string AssemblyName,
    string DefaultNamespace,
    string TargetFramework);
