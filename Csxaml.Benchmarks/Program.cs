using BenchmarkDotNet.Running;

namespace Csxaml.Benchmarks;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        Directory.CreateDirectory(RepoPaths.BenchmarkArtifactsDirectory);

        if (WinUiSmokeCommand.TryRun(args))
        {
            return 0;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
            .Run(args, new BenchmarkConfig());
        return 0;
    }
}
