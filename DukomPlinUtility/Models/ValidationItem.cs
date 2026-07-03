namespace DukomPlinUtility.Models;

public sealed class ValidationItem
{
    public string UserCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Meter { get; set; } = string.Empty;
    public string Reading { get; set; } = string.Empty;
    public string PreviousReading { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLevel { get; set; } = "OK";
    public string Message { get; set; } = string.Empty;
}
