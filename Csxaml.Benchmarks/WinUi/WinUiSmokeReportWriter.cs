using System.Text;
using System.Text.Json;

namespace Csxaml.Benchmarks;

internal static class WinUiSmokeReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static void Write(WinUiSmokeReport report)
    {
        File.WriteAllText(GetJsonPath(), JsonSerializer.Serialize(report, JsonOptions));
        File.WriteAllText(GetMarkdownPath(), BuildMarkdown(report));
    }

    private static string BuildMarkdown(WinUiSmokeReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# WinUI Projection Smoke");
        builder.AppendLine();
        builder.AppendLine($"- Captured (UTC): {report.CapturedAtUtc:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"- Iterations: {report.Iterations}");
        builder.AppendLine($"- Status: {report.Status}");
        if (!string.IsNullOrWhiteSpace(report.Note))
        {
            builder.AppendLine($"- Note: {report.Note}");
        }

        if (report.Scenarios.Count == 0)
        {
            return builder.ToString();
        }

        builder.AppendLine();
        builder.AppendLine("| Scenario | Status | Avg us/op | Root retained | Control retained | Focus preserved | Selection preserved | Note |");
        builder.AppendLine("| --- | --- | ---: | --- | --- | --- | --- | --- |");
        foreach (var scenario in report.Scenarios)
        {
            builder.AppendLine(
                $"| {scenario.Name} | {scenario.Status} | {scenario.AverageMicroseconds:F3} | {ToYesNo(scenario.RootRetained)} | {ToYesNo(scenario.PrimaryControlRetained)} | {ToYesNo(scenario.FocusPreserved)} | {ToYesNo(scenario.SelectionPreserved)} | {scenario.Note ?? string.Empty} |");
        }

        return builder.ToString();
    }

    private static string GetJsonPath()
    {
        return Path.Combine(RepoPaths.BenchmarkArtifactsDirectory, "winui-smoke.json");
    }

    private static string GetMarkdownPath()
    {
        return Path.Combine(RepoPaths.BenchmarkArtifactsDirectory, "winui-smoke.md");
    }

    private static string ToYesNo(bool value)
    {
        return value ? "yes" : "no";
    }
}
