namespace DukomPlinUtility.Models;

public enum AppModule
{
    Dashboard,
    WalkBy,
    NbIot,
    Zgrade,
    Logs,
    Settings,
    About
}

public static class AppModuleExtensions
{
    public static AppModule FromString(string? value)
    {
        return Enum.TryParse<AppModule>(value, ignoreCase: true, out var module)
            ? module
            : AppModule.Dashboard;
    }
}
