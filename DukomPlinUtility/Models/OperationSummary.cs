namespace DukomPlinUtility.Models;

public sealed class OperationSummary
{
    public string Module { get; set; } = string.Empty;
    public string LastRun { get; set; } = "Nije pokrenuto";
    public string MainStat { get; set; } = "-";
    public string SecondaryStat { get; set; } = string.Empty;
    public string WarningStat { get; set; } = string.Empty;
    public string Status { get; set; } = "Ready";
    public string StatusLevel { get; set; } = "OK";
}
