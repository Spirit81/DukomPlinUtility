namespace DukomPlinUtility.Models;

public sealed class AppSettings
{
    public string SharedSourceFile { get; set; } = string.Empty;
    public string LastOutputFolder { get; set; } = string.Empty;
    public string LastModule { get; set; } = "Dashboard";
    public double WindowLeft { get; set; } = 80;
    public double WindowTop { get; set; } = 60;
    public double WindowWidth { get; set; } = 1800;
    public double WindowHeight { get; set; } = 950;
    public bool WindowMaximized { get; set; }
}
