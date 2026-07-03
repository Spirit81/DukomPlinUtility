namespace DukomPlinUtility.Models;

public sealed class ZgradeIssue
{
    public string UserCode { get; set; } = string.Empty;
    public string Meter { get; set; } = string.Empty;
    public string SourceReading { get; set; } = string.Empty;
    public string NewReading { get; set; } = string.Empty;
    public string Difference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
