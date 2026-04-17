namespace Csxaml.Benchmarks;

internal static class RepoPaths
{
    public static string RepositoryRoot { get; } = ResolveRepositoryRoot();

    public static string BenchmarkArtifactsDirectory { get; } =
        Path.Combine(RepositoryRoot, "artifacts", "benchmarks");

    private static string ResolveRepositoryRoot()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    }
}
