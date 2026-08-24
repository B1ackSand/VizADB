using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VizADB.Services;
using VizADB.ViewModels;

namespace VizADB;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _statusTimer;

    public MainWindow()
    {
        InitializeComponent();
        LoadAppIcon();

        // scrcpy 等 GPU 窗口切换后 WPF 偶发局部不重绘，强制在窗口激活时刷新画面
        Activated += (_, _) => InvalidateVisual();

        _statusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _statusTimer.Tick += async (_, _) =>
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshStatusAsync();
            }
        };

        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.RefreshStatusAsync();
            }
            _statusTimer.Start();
        };
    }

    private void LoadAppIcon()
    {
        var asm = typeof(MainWindow).Assembly;
        using var stream = asm.GetManifestResourceStream("VizADB.Assets.VizADB.ico");
        if (stream is null) return;

        var icon = new BitmapImage();
        icon.BeginInit();
        icon.StreamSource = stream;
        icon.DecodePixelWidth = 32;
        icon.EndInit();
        icon.Freeze();
        Icon = icon;
    }

    public SettingsService? SettingsService { get; set; }
    public AdbService? AdbService { get; set; }
    public ScrcpyService? ScrcpyService { get; set; }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        if (SettingsService is null || AdbService is null || ScrcpyService is null)
        {
            return;
        }

        var window = new SettingsWindow(SettingsService, AdbService, ScrcpyService)
        {
            Owner = this,
        };
        window.ShowDialog();
    }
}
