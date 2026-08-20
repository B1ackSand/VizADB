using System.IO;
using System.Windows;
using VizADB.Services;
using VizADB.ViewModels;

namespace VizADB;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = new SettingsService().Load();
        var adbPath = ResolvePath(settings.AdbPath, "adb");
        var scrcpyPath = ResolvePath(settings.ScrcpyPath, "scrcpy");

        var settingsService = new SettingsService();
        var adbService = new AdbService(adbPath);
        var scrcpyService = new ScrcpyService(scrcpyPath);

        var window = new MainWindow
        {
            DataContext = new MainViewModel(adbService, scrcpyService),
            SettingsService = settingsService,
            AdbService = adbService,
            ScrcpyService = scrcpyService,
        };

        MainWindow = window;
        window.Show();
    }

    private static string ResolvePath(string? configured, string name)
    {
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        return ToolLocator.FindExecutable(name) ?? name;
    }
}
