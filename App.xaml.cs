using System.Windows;
using VizADB.Services;
using VizADB.ViewModels;

namespace VizADB;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var adbPath = ToolLocator.FindExecutable("adb") ?? "adb";
        var scrcpyPath = ToolLocator.FindExecutable("scrcpy") ?? "scrcpy";

        var adbService = new AdbService(adbPath);
        var scrcpyService = new ScrcpyService(scrcpyPath);

        var window = new MainWindow
        {
            DataContext = new MainViewModel(adbService, scrcpyService)
        };

        MainWindow = window;
        window.Show();
    }
}
