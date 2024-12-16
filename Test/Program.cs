// 获取当前应用的名称和路径
using System.Diagnostics;
using System.Runtime.InteropServices;
using StartupManagerClassLibrary;

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
    type = StartupType.StartupFolder; // 示例选择
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
    Console.WriteLine("不支持的操作系统。");
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


Console.WriteLine("按任意键退出。");
Console.ReadKey();