namespace Csxaml.Generator;

internal sealed record ProjectGenerationContext(
    string AssemblyName,
    string DefaultComponentNamespace,
    string InternalGeneratedNamespace,
    CsxamlApplicationMode ApplicationMode = CsxamlApplicationMode.Hybrid)
{
    public static ProjectGenerationContext CreateDefault()
    {
        return new ProjectGenerationContext(
            "TestProject",
            "TestProject",
            "TestProject.__CsxamlGenerated");
    }
}
