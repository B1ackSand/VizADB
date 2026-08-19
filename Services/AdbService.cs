using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using VizADB.Models;

namespace VizADB.Services;

public class AdbService
{
    private string _adbPath;

    public AdbService(string adbPath)
    {
        _adbPath = Normalize(adbPath);
    }

    public string AdbPath
    {
        get => _adbPath;
        set => _adbPath = Normalize(value);
    }

    public async Task<IReadOnlyList<AdbDevice>> GetDevicesAsync()
    {
        var output = await RunAsync("devices");
        var devices = new List<AdbDevice>();

        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0
                || line.StartsWith("List of devices", StringComparison.Ordinal)
                || line.StartsWith('*'))
            {
                continue;
            }

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                devices.Add(new AdbDevice(parts[0], parts[1]));
            }
        }

        return devices;
    }

    public Task<string> ConnectAsync(string ip, int port) =>
        RunAsync($"connect {ip}:{port}");

    public Task<string> DisconnectAsync(string ip, int port) =>
        RunAsync($"disconnect {ip}:{port}");

    public Task<string> RebootAsync(string ip, int port) =>
        RunAsync($"-s {ip}:{port} reboot");

    private async Task<string> RunAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _adbPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        Process process;
        try
        {
            process = Process.Start(startInfo) ??
                      throw new InvalidOperationException("无法启动 adb 进程。");
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"无法启动 adb（{_adbPath}），请确认 adb 已安装且路径正确。", ex);
        }

        using (process)
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await stdoutTask;
            var error = await stderrTask;

            return $"{output}{error}".Trim();
        }
    }

    private static string Normalize(string path) =>
        string.IsNullOrWhiteSpace(path) ? "adb" : path.Trim();
}
