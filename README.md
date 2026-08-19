# VizADB

基于 .NET 8 / WPF 的 ADB & scrcpy 设备管理工具，运行于 Windows。

## 功能

- 通过 IP 地址 + 端口号连接 ADB 设备（端口未填写时默认为 `5555`，输入框内有提示）
- 断开 ADB 连接
- 通过 ADB 重启设备
- 实时显示当前连接状态（已连接 / 离线 / 未授权 / 未连接）及设备列表
- 连接成功后可使用 scrcpy 显示设备画面：
  - 含音频
  - 禁用音频（`scrcpy --no-audio`）

## 环境要求

- Windows 10/11
- .NET 8 SDK（构建运行）
- adb（Android platform-tools）
- scrcpy

> 程序会自动从 `PATH` 或常见安装位置（如 `%LOCALAPPDATA%\Android\Sdk\platform-tools`）查找 adb，未找到时可在地面上手动填写 adb / scrcpy 可执行文件路径。

## 构建与运行

```bash
dotnet run
```

发布单机版：

```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

## 项目结构

```
VizADB.csproj                     # 项目文件（net8.0-windows / WPF）
App.xaml(.cs)                     # 应用入口
MainWindow.xaml(.cs)              # 主窗口
Services/AdbService.cs            # ADB 命令封装（connect/disconnect/reboot/devices）
Services/ScrcpyService.cs         # scrcpy 启动封装
Services/ToolLocator.cs           # adb / scrcpy 路径自动查找
ViewModels/MainViewModel.cs       # 主界面视图模型
Commands/RelayCommand.cs          # ICommand 实现
Converters/EmptyStringToVisibilityConverter.cs  # 输入框占位提示
Models/AdbDevice.cs               # 设备模型
```
