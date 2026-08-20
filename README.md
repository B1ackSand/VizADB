# VizADB

基于 .NET 8 / WPF 的 **ADB & scrcpy 设备管理工具**，运行于 Windows。通过 Wi-Fi / 局域网管理 Android 设备：连接、断开、重启、投屏。

## 功能

- **连接设备**：通过 `IP:端口` 连接 Android 设备（端口未填写时默认为 `5555`，输入框内有提示）
- **断开连接**：一键断开当前连接的设备
- **重启设备**：向设备发送重启指令（需二次确认）
- **实时状态**：每 3 秒自动刷新，显示 已连接 / 离线 / 未授权 / 未连接 等状态及设备列表
- **自动识别**：启动时自动识别并接管已连接的无线设备，无需重复连接
- **投屏显示（scrcpy）**：连接成功后显示设备画面，支持含音频 / 禁用音频（`--no-audio`）两种模式
- **日志面板**：实时记录所有操作与输出，便于排错

## 环境要求

- Windows 10 / 11（x64），**无需安装 .NET 运行时**（已内置在发布包中）
- `adb`（Android platform-tools）
- `scrcpy`

> adb / scrcpy 不随程序打包。程序启动时会自动从以下位置查找：
> 1. 可执行文件所在目录
> 2. 系统 `PATH`
> 3. Android SDK 常见安装位置（如 `%LOCALAPPDATA%\Android\Sdk\platform-tools`）
>
> 若未找到，可在程序右上角「设置」中手动指定 adb / scrcpy 可执行文件路径，配置会保存到 `%APPDATA%\VizADB\settings.json`。

## 使用方式

### 方式一：直接使用（推荐）

下载发布包并解压，运行 `VizADB.exe` 即可。

### 方式二：源代码构建

需要 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)：

```bash
dotnet run
```

发布为单文件可执行程序：

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

## 快速上手

1. 确保手机和电脑处于同一局域网络
2. 手机上开启「开发者选项」→「无线调试」或通过 USB 执行 `adb tcpip 5555`
3. 在程序「IP 地址」框输入手机的局域网 IP（如 `192.168.1.100`），点击「连接 ADB」
4. 手机上确认授权（如出现未授权提示）
5. 连接成功后即可使用「启动画面」投屏，或「重启设备」

## 项目结构

```
VizADB.csproj                          # 项目文件（net8.0-windows / WPF）
VizADB.sln                             # 解决方案文件
App.xaml(.cs)                          # 应用入口（手动组装服务，无 DI 容器）
MainWindow.xaml(.cs)                   # 主窗口（含 3s 状态刷新定时器）
SettingsWindow.xaml(.cs)               # 设置窗口（adb / scrcpy 路径配置）
Services/AdbService.cs                 # ADB 命令封装（connect/disconnect/reboot/devices）
Services/ScrcpyService.cs              # scrcpy 启动封装
Services/ToolLocator.cs                # adb / scrcpy 路径自动查找
Services/SettingsService.cs            # 配置文件读写（%APPDATA%\VizADB\settings.json）
ViewModels/MainViewModel.cs            # 主界面视图模型（全部业务逻辑）
Commands/RelayCommand.cs               # ICommand 实现
Converters/EmptyStringToVisibilityConverter.cs  # 输入框占位提示
Models/AdbDevice.cs                    # 设备模型
Models/AppSettings.cs                  # 配置模型
```

## 技术说明

- deepseek-v4-flash 生成
- 基于 `.NET 8` + `WPF`（`net8.0-windows`），无外部 NuGet 依赖
- 通过进程调用 `adb` / `scrcpy` 命令行工具实现设备管理
- UI 语言为简体中文
