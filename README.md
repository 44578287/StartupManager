# 跨平台开机自启管理器 (StartupManager)

一个用于在 **Windows**、**Linux** 和 **macOS** 平台上设置应用程序开机自启的通用 C# 类库。`StartupManager` 提供了简单易用的接口，支持多种开机自启方式，帮助开发者轻松实现应用程序的自启功能。(此代码基本由GPT o1构成 有Bug欢迎反馈)

## 主要功能

- **跨平台支持**：兼容 Windows、Linux 和 macOS。
- **多种开机自启方式**：
  - **Windows**：
    - 注册表 (`HKCU` 和 `HKLM`)
    - 启动文件夹
  - **Linux**：
    - 桌面环境 Autostart (`.desktop` 文件)
  - **macOS**：
    - Launch Agents (`.plist` 文件)
- **权限管理**：在需要管理员权限的情况下，自动请求权限提升。
- **简便接口**：提供启用、禁用和检查开机自启状态的方法。

## 目录

- [特性](#特性)
- [支持平台](#支持平台)
- [安装](#安装)
- [使用说明](#使用说明)
  - [示例代码](#示例代码)
- [依赖项](#依赖项)
- [注意事项](#注意事项)
- [贡献](#贡献)
- [许可证](#许可证)

## 特性

- **简单易用**：只需几行代码即可集成到项目中。
- **灵活选择**：根据需求选择不同的开机自启方式和范围。
- **自动权限请求**：在需要提升权限时，自动提示用户进行权限提升。
- **跨平台兼容**：统一的接口，适用于多个操作系统。

## 支持平台

- **Windows**
  - 注册表自启（当前用户或所有用户）
  - 启动文件夹自启
- **Linux**
  - 桌面环境 Autostart（支持 GNOME、KDE 等主流桌面环境）
- **macOS**
  - Launch Agents 自启

## 安装

### 使用 NuGet 包管理器

你可以通过 NuGet 包管理器将 `StartupManagerClassLibrary` 添加到你的项目中：

```bash
Install-Package StartupManagerClassLibrary
```

### 手动集成 

1. 下载或克隆本项目的代码。
 
2. 将 `StartupManager.cs` 文件添加到你的项目中。
 
3. 确保添加对 **Windows Script Host Object Model**  的引用（仅限 Windows 平台）： 
  - 在 **Visual Studio**  中，右键点击项目 -> **“添加”**  -> **“引用...”** 。
 
  - 选择 **“COM”**  标签页。
 
  - 勾选 **“Windows Script Host Object Model”** 。
 
  - 点击 **“确定”** 。

## 使用说明 

### 引入命名空间 


```csharp
using StartupManagerClassLibrary;
```
初始化 `StartupManager`创建 `StartupManager` 的实例时，需要提供应用程序的名称、路径以及选择的启动类型。

```csharp
string appName = "自启动测试";
string appPath = Process.GetCurrentProcess().MainModule!.FileName;

// 根据操作系统选择启动类型
StartupType type;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // 选择使用注册表或启动文件夹
    // StartupType.Registry：当前用户注册表
    // StartupType.AllUsersRegistry：所有用户注册表（需要管理员权限）
    // StartupType.StartupFolder：启动文件夹
    type = StartupType.Registry; // 示例选择
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Linux 使用桌面环境 Autostart
    type = StartupType.AutostartDesktop;
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    // macOS 使用 Launch Agents
    type = StartupType.LaunchAgent;
}
else
{
    throw new PlatformNotSupportedException("Unsupported operating system.");
}

StartupManager startupManager = new StartupManager(appName, appPath, type);
```

### 启用开机自启 


```csharp
try
{
    bool isEnabled = startupManager.IsStartupEnabled();
    Console.WriteLine($"开机自启状态: {(isEnabled ? "已启用" : "未启用")}");

    if (!isEnabled)
    {
        startupManager.EnableStartup();
        Console.WriteLine("已启用开机自启。");
    }
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"权限错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"发生异常: {ex.Message}");
}
```

### 禁用开机自启 


```csharp
try
{
    bool isEnabled = startupManager.IsStartupEnabled();
    Console.WriteLine($"开机自启状态: {(isEnabled ? "已启用" : "未启用")}");

    if (isEnabled)
    {
        startupManager.DisableStartup();
        Console.WriteLine("已禁用开机自启。");
    }
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"权限错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"发生异常: {ex.Message}");
}
```

## 示例代码 
以下是一个完整的示例程序，展示如何使用 `StartupManager` 类来管理开机自启：

```csharp
using System;
using System.Runtime.InteropServices;
using StartupManagerClassLibrary;

class Program
{
    static void Main(string[] args)
    {
        // 获取当前应用的名称和路径
        string appName = "自启动测试";
        string appPath = Process.GetCurrentProcess().MainModule!.FileName;

        // 根据操作系统选择启动类型
        StartupType type;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 选择使用注册表或启动文件夹
            // StartupType.Registry：当前用户注册表
            // StartupType.AllUsersRegistry：所有用户注册表（需要管理员权限）
            // StartupType.StartupFolder：启动文件夹
            type = StartupType.Registry; // 示例选择
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux 使用桌面环境 Autostart
            type = StartupType.AutostartDesktop;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS 使用 Launch Agents
            type = StartupType.LaunchAgent;
        }
        else
        {
            Console.WriteLine("Unsupported operating system.");
            return;
        }

        StartupManager startupManager = new StartupManager(appName, appPath, type);

        try
        {
            // 检查是否已经设置为开机自启
            bool isEnabled = startupManager.IsStartupEnabled();
            Console.WriteLine($"开机自启状态: {(isEnabled ? "已启用" : "未启用")}");

            // 启用开机自启
            if (!isEnabled)
            {
                startupManager.EnableStartup();
                Console.WriteLine("已启用开机自启。");
            }

            // 禁用开机自启
            /*
            if (isEnabled)
            {
                startupManager.DisableStartup();
                Console.WriteLine("已禁用开机自启。");
            }
            */
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"权限错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生异常: {ex.Message}");
        }
    }
}
```

## 依赖项 
 
- **Windows** ： 
  - **Windows Script Host Object Model** ： 
    - 需要在项目中添加对 **Windows Script Host Object Model**  的引用。
 
    - 在 **Visual Studio**  中，右键点击项目 -> **“添加”**  -> **“引用...”**  -> **“COM”**  标签页 -> 选择 **“Windows Script Host Object Model”**  -> 点击 **“确定”** 。
 
- **Linux & macOS** ：
  - 无需额外依赖，但需要确保应用程序路径正确且具有执行权限。
 
  - 在 **macOS**  和 **Linux**  上，可能需要确保应用具有执行权限：

```bash
chmod +x /path/to/your/application
```

## 注意事项 
 
1. **权限问题** ： 
  - **Windows** ： 
    - 设置 `AllUsersRegistry` 需要管理员权限。
 
    - 程序会自动请求权限提升，如果用户拒绝，将抛出 `UnauthorizedAccessException`。
 
  - **Linux & macOS** ：
    - 用户级别的开机自启通常不需要额外权限。

    - 系统级别的自启可能需要更高权限，上述实现仅处理用户级别的自启。
 
2. **桌面环境差异** （仅限 Linux）： 
  - 不同的桌面环境（如 GNOME、KDE、XFCE）对 `.desktop` 文件的支持可能有所不同，但大多数主流桌面环境都支持 `~/.config/autostart/` 中的 `.desktop` 文件。

  - 如果用户使用的桌面环境不支持此方法，可能需要采用其他自启机制，如 systemd 用户服务。
 
3. **macOS Launch Agents** ： 
  - `launchctl` 命令用于加载和卸载 Launch Agents。

  - 用户可能需要重启或注销后，才能看到效果。
 
4. **应用路径** ： 
  - 确保传递给 `StartupManager` 的 `applicationPath` 是应用程序的完整路径，并且在目标系统上是可执行的。
 
5. **安全性** ：
  - 确保应用程序路径安全，避免潜在的安全风险，例如路径注入或执行不可信的应用程序。
 
6. **错误处理** ：
  - 代码中已经添加了基本的异常捕获，可以根据需要扩展更详细的错误处理和日志记录。
 
  - 在跨平台环境中，某些操作可能失败（例如，`launchctl` 不存在或不可用），需要根据具体情况进行处理。

## 贡献 

欢迎贡献者参与本项目！如果你发现任何问题或有改进建议，请按照以下步骤进行：
 
1. **Fork 本仓库** 。
 
2. **创建特性分支** ：`git checkout -b feature/你的特性`。
 
3. **提交更改** ：`git commit -m '添加了一个新特性'`。
 
4. **推送到分支** ：`git push origin feature/你的特性`。
 
5. **创建 Pull Request** 。
