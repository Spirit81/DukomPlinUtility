using System.Windows;
using DukomPlinUtility.Models;

namespace DukomPlinUtility.Services;

public static class WindowSettingsService
{
    public static void Apply(Window window, AppSettings settings)
    {
        window.Width = Safe(settings.WindowWidth, 1800);
        window.Height = Safe(settings.WindowHeight, 950);
        window.Left = Safe(settings.WindowLeft, 80);
        window.Top = Safe(settings.WindowTop, 60);

        if (IsOffScreen(window.Left, window.Top))
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (settings.WindowMaximized)
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    public static void Capture(Window window, AppSettings settings)
    {
        if (window.WindowState == WindowState.Normal)
        {
            settings.WindowLeft = Safe(window.Left, 80);
            settings.WindowTop = Safe(window.Top, 60);
            settings.WindowWidth = Safe(window.Width, 1800);
            settings.WindowHeight = Safe(window.Height, 950);
        }

        settings.WindowMaximized = window.WindowState == WindowState.Maximized;
    }

    private static bool IsOffScreen(double left, double top)
    {
        return double.IsNaN(left) || double.IsInfinity(left) ||
               double.IsNaN(top) || double.IsInfinity(top) ||
               left < -10000 || top < -10000;
    }

    private static double Safe(double value, double fallback)
    {
        return double.IsNaN(value) || double.IsInfinity(value) || value <= 0
            ? fallback
            : value;
    }
}
