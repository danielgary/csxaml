namespace Csxaml.Benchmarks;

internal sealed record WinUiSmokeReport(
    DateTimeOffset CapturedAtUtc,
    int Iterations,
    string Status,
    string? Note,
    IReadOnlyList<WinUiSmokeScenarioResult> Scenarios)
{
    public static WinUiSmokeReport CreateUnavailable(int iterations, string note)
    {
        return new WinUiSmokeReport(
            DateTimeOffset.UtcNow,
            iterations,
            "unavailable",
            note,
            Array.Empty<WinUiSmokeScenarioResult>());
    }
}

internal sealed record WinUiSmokeScenarioResult(
    string Name,
    string Status,
    double AverageMicroseconds,
    bool RootRetained,
    bool PrimaryControlRetained,
    bool FocusPreserved,
    bool SelectionPreserved,
    string? Note);
