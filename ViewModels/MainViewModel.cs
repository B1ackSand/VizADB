using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using VizADB.Commands;
using VizADB.Models;
using VizADB.Services;

namespace VizADB.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AdbService _adbService;
    private readonly ScrcpyService _scrcpyService;

    private bool _refreshing;
    private bool _isBusy;
    private string _ip = string.Empty;
    private string _port = string.Empty;
    private string? _targetSerial;
    private string _statusText = "未连接";
    private Brush _statusColor = Brushes.Gray;
    private string _log = string.Empty;

    public MainViewModel(AdbService adbService, ScrcpyService scrcpyService)
    {
        _adbService = adbService;
        _scrcpyService = scrcpyService;

        ConnectCommand = new RelayCommand(_ => _ = ConnectAsync(), () => !IsBusy && !IsConnected);
        DisconnectCommand = new RelayCommand(_ => _ = DisconnectAsync(), () => !IsBusy && HasTarget);
        RebootCommand = new RelayCommand(_ => _ = RebootAsync(), () => !IsBusy && IsOnline);
        ShowScreenCommand = new RelayCommand(_ => LaunchScrcpy(false), () => !IsBusy && IsOnline);
        ShowScreenMutedCommand = new RelayCommand(_ => LaunchScrcpy(true), () => !IsBusy && IsOnline);

        NotifyCanExecuteChanged();
    }

    public ObservableCollection<AdbDevice> Devices { get; } = new();

    public RelayCommand ConnectCommand { get; }
    public RelayCommand DisconnectCommand { get; }
    public RelayCommand RebootCommand { get; }
    public RelayCommand ShowScreenCommand { get; }
    public RelayCommand ShowScreenMutedCommand { get; }

    public string Ip
    {
        get => _ip;
        set { if (SetField(ref _ip, value)) NotifyCanExecuteChanged(); }
    }

    public string Port
    {
        get => _port;
        set { if (SetField(ref _port, value)) NotifyCanExecuteChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { if (SetField(ref _isBusy, value)) NotifyCanExecuteChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public Brush StatusColor
    {
        get => _statusColor;
        private set => SetField(ref _statusColor, value);
    }

    public string Log
    {
        get => _log;
        private set => SetField(ref _log, value);
    }

    public bool IsConnected =>
        _targetSerial is not null &&
        Devices.Any(d => string.Equals(d.Serial, _targetSerial, StringComparison.OrdinalIgnoreCase) && d.IsOnline);

    public bool IsOnline => IsConnected;

    public bool HasTarget => _targetSerial is not null;

    public async Task RefreshStatusAsync()
    {
        if (_refreshing) return;
        _refreshing = true;

        try
        {
            var devices = await _adbService.GetDevicesAsync();

            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }

            if (_targetSerial is null)
            {
                var wireless = devices.FirstOrDefault(d => d.Serial.Contains(':'));
                if (wireless is not null)
                {
                    _targetSerial = wireless.Serial;
                }
                else
                {
                    SetStatus("未连接", Brushes.Gray);
                    NotifyCanExecuteChanged();
                    return;
                }
            }

            var target = devices.FirstOrDefault(d =>
                string.Equals(d.Serial, _targetSerial, StringComparison.OrdinalIgnoreCase));

            if (target is null)
            {
                SetStatus("未连接（目标设备不在列表中）", Brushes.Gray);
            }
            else
            {
                switch (target.State)
                {
                    case "device":
                        SetStatus($"已连接：{_targetSerial}", Brushes.Green);
                        break;
                    case "offline":
                        SetStatus("设备离线 (offline)", Brushes.OrangeRed);
                        break;
                    case "unauthorized":
                        SetStatus("设备未授权 (unauthorized)，请在设备屏幕上确认", Brushes.Orange);
                        break;
                    default:
                        SetStatus($"状态异常：{target.State}", Brushes.OrangeRed);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            SetStatus("ADB 执行失败", Brushes.Red);
            AppendLog($"错误：{ex.Message}");
        }
        finally
        {
            _refreshing = false;
            NotifyCanExecuteChanged();
        }
    }

    public async Task ConnectAsync()
    {
        var ip = Ip?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(ip))
        {
            AppendLog("错误：请输入设备 IP 地址。");
            return;
        }

        var portText = string.IsNullOrWhiteSpace(Port) ? "5555" : Port.Trim();
        if (!int.TryParse(portText, out var port) || port is < 1 or > 65535)
        {
            AppendLog($"错误：端口号无效（{portText}），请输入 1-65535 的整数，默认为 5555。");
            return;
        }

        IsBusy = true;
        _targetSerial = $"{ip}:{port}";
        SetStatus("连接中...", Brushes.Goldenrod);

        try
        {
            AppendLog($"$ adb connect {ip}:{port}");
            var output = await _adbService.ConnectAsync(ip, port);
            AppendLog(output);

            var success = output.Contains("connected to", StringComparison.OrdinalIgnoreCase)
                          || output.Contains("already connected", StringComparison.OrdinalIgnoreCase);

            if (success)
            {
                AppendLog("连接成功，正在获取状态...");
            }
            else
            {
                AppendLog("未确认连接成功，请查看上方输出。");
            }
        }
        catch (Exception ex)
        {
            SetStatus("连接失败", Brushes.Red);
            AppendLog($"错误：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshStatusAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_targetSerial is null) return;

        var (ip, port) = ParseTarget(_targetSerial);
        IsBusy = true;

        try
        {
            AppendLog($"$ adb disconnect {_targetSerial}");
            var output = await _adbService.DisconnectAsync(ip, port);
            AppendLog(output);

            _targetSerial = null;
            SetStatus("已断开连接", Brushes.Gray);
        }
        catch (Exception ex)
        {
            SetStatus("断开失败", Brushes.Red);
            AppendLog($"错误：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshStatusAsync();
    }

    public async Task RebootAsync()
    {
        if (_targetSerial is null) return;

        var confirm = MessageBox.Show(
            $"确认重启设备 {_targetSerial} 吗？",
            "重启设备",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var (ip, port) = ParseTarget(_targetSerial);
        IsBusy = true;

        try
        {
            AppendLog($"$ adb -s {_targetSerial} reboot");
            var output = await _adbService.RebootAsync(ip, port);
            if (!string.IsNullOrEmpty(output))
            {
                AppendLog(output);
            }
            AppendLog("已发送重启指令，等待设备重新连接...");
        }
        catch (Exception ex)
        {
            AppendLog($"错误：{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshStatusAsync();
    }

    public void LaunchScrcpy(bool disableAudio)
    {
        if (_targetSerial is null || !IsOnline)
        {
            AppendLog("请先连接设备后再启动画面。");
            return;
        }

        try
        {
            _scrcpyService.Start(_targetSerial, disableAudio);

            var mode = disableAudio ? "（禁用音频）" : "（含音频）";
            var noAudio = disableAudio ? " --no-audio" : string.Empty;
            AppendLog($"已启动 scrcpy{mode}：{_scrcpyService.ScrcpyPath} -s {_targetSerial}{noAudio}");
        }
        catch (Exception ex)
        {
            AppendLog($"启动 scrcpy 失败：{ex.Message}");
        }
    }

    public void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Log = string.IsNullOrEmpty(Log) ? line : $"{Log}{Environment.NewLine}{line}";
    }

    private void SetStatus(string text, Brush color)
    {
        StatusText = text;
        StatusColor = color;
    }

    private void NotifyCanExecuteChanged()
    {
        ConnectCommand.RaiseCanExecuteChanged();
        DisconnectCommand.RaiseCanExecuteChanged();
        RebootCommand.RaiseCanExecuteChanged();
        ShowScreenCommand.RaiseCanExecuteChanged();
        ShowScreenMutedCommand.RaiseCanExecuteChanged();
    }

    private static (string Ip, int Port) ParseTarget(string serial)
    {
        var index = serial.LastIndexOf(':');
        if (index > 0 && int.TryParse(serial[(index + 1)..], out var port))
        {
            return (serial[..index], port);
        }
        return (serial, 5555);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
