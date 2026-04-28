namespace Csxaml.Benchmarks;

internal static class WinUiSmokeCommand
{
    private const string CommandName = "--winui-smoke";
    private const string IterationsArgument = "--iterations";

    public static bool TryRun(string[] args)
    {
        if (!args.Contains(CommandName, StringComparer.Ordinal))
        {
            return false;
        }

        var iterations = ParseIterations(args);
        var report = WinUiSmokeApplication.Run(iterations);
        WinUiSmokeReportWriter.Write(report);
        Console.WriteLine(
            $"WinUI smoke complete: {report.Status} ({report.Scenarios.Count} scenario(s), iterations={report.Iterations}).");
        Console.WriteLine(
            $"Artifacts: {RepoPaths.BenchmarkArtifactsDirectory}");
        return true;
    }

    private static int ParseIterations(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (!string.Equals(args[i], IterationsArgument, StringComparison.Ordinal))
            {
                continue;
            }

            if (int.TryParse(args[i + 1], out var iterations) && iterations > 0)
            {
                return iterations;
            }
        }

        return 50;
    }
}
