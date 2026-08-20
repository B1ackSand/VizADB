using System.IO;

namespace VizADB.Services;

public static class ToolLocator
{
    public static string? FindExecutable(string name)
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, $"{name}.exe");
        if (File.Exists(bundled))
        {
            return bundled;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathValue))
        {
            foreach (var dir in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var exe = Path.Combine(dir, $"{name}.exe");
                    if (File.Exists(exe))
                    {
                        return exe;
                    }

                    var raw = Path.Combine(dir, name);
                    if (File.Exists(raw))
                    {
                        return raw;
                    }
                }
                catch (Exception)
                {
                    // 跳过无法访问的目录
                }
            }
        }

        if (OperatingSystem.IsWindows() && name.Equals("adb", StringComparison.OrdinalIgnoreCase))
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var candidates = new[]
            {
                Path.Combine(localAppData, "Android", "Sdk", "platform-tools", "adb.exe"),
                Path.Combine(localAppData, "Android", "Sdk", "platform-tools", "adb.bat"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "platform-tools", "adb.exe"),
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}
