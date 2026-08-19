using System.ComponentModel;
using System.Diagnostics;

namespace VizADB.Services;

public class ScrcpyService
{
    private string _scrcpyPath;

    public ScrcpyService(string scrcpyPath)
    {
        _scrcpyPath = Normalize(scrcpyPath);
    }

    public string ScrcpyPath
    {
        get => _scrcpyPath;
        set => _scrcpyPath = Normalize(value);
    }

    public Process Start(string serial, bool disableAudio)
    {
        var arguments = $"-s {serial}{(disableAudio ? " --no-audio" : string.Empty)}";

        var startInfo = new ProcessStartInfo
        {
            FileName = _scrcpyPath,
            Arguments = arguments,
            UseShellExecute = false,
        };

        try
        {
            return Process.Start(startInfo) ??
                   throw new InvalidOperationException("无法启动 scrcpy 进程。");
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"无法启动 scrcpy（{_scrcpyPath}），请确认 scrcpy 已安装且路径正确。", ex);
        }
    }

    private static string Normalize(string path) =>
        string.IsNullOrWhiteSpace(path) ? "scrcpy" : path.Trim();
}
