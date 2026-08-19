using System.Windows;
using System.Windows.Threading;
using VizADB.ViewModels;

namespace VizADB;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _statusTimer;

    public MainWindow()
    {
        InitializeComponent();

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
}
