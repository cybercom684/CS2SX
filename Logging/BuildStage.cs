namespace CS2SX.Logging;

public enum StageStatus
{
    Waiting, Running, Done, Failed, Warning
}

public sealed class BuildStage
{
    public string Name { get; init; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public StageStatus Status { get; set; } = StageStatus.Waiting;
    public int Progress
    {
        get; set;
    }   // 0–100
    public string Elapsed { get; set; } = string.Empty;
}