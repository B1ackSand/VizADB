using System.IO;
using System.Windows;
using Microsoft.Win32;
using VizADB.Models;
using VizADB.Services;

namespace VizADB;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly AdbService _adbService;
    private readonly ScrcpyService _scrcpyService;

    public SettingsWindow(SettingsService settingsService, AdbService adbService, ScrcpyService scrcpyService)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _adbService = adbService;
        _scrcpyService = scrcpyService;

        AdbBox.Text = adbService.AdbPath == "adb" ? string.Empty : adbService.AdbPath;
        ScrcpyBox.Text = scrcpyService.ScrcpyPath == "scrcpy" ? string.Empty : scrcpyService.ScrcpyPath;
    }

    private void BrowseAdb_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 adb 可执行文件",
            Filter = "adb 可执行文件|adb.exe|所有文件|*.*",
            FileName = "adb.exe",
        };
        if (dialog.ShowDialog(this) == true)
        {
            AdbBox.Text = dialog.FileName;
        }
    }

    private void BrowseScrcpy_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 scrcpy 可执行文件",
            Filter = "scrcpy 可执行文件|scrcpy.exe|所有文件|*.*",
            FileName = "scrcpy.exe",
        };
        if (dialog.ShowDialog(this) == true)
        {
            ScrcpyBox.Text = dialog.FileName;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var adbPath = AdbBox.Text.Trim();
        var scrcpyPath = ScrcpyBox.Text.Trim();

        _adbService.AdbPath = adbPath;
        _scrcpyService.ScrcpyPath = scrcpyPath;

        _settingsService.Save(new AppSettings
        {
            AdbPath = _adbService.AdbPath == "adb" ? string.Empty : _adbService.AdbPath,
            ScrcpyPath = _scrcpyService.ScrcpyPath == "scrcpy" ? string.Empty : _scrcpyService.ScrcpyPath,
        });

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
