using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using IWshRuntimeLibrary;
using File = System.IO.File; // 需要添加对 Windows Script Host Object Model 的引用

namespace StartupManagerClassLibrary
{
    /// <summary>
    /// 启动类型
    /// </summary>
    public enum StartupType
    {
        /// <summary>
        /// 当前用户注册表（仅 Windows）
        /// </summary>
        Registry,
        /// <summary>
        /// 所有用户注册表（仅 Windows，需管理员权限）
        /// </summary>
        AllUsersRegistry,
        /// <summary>
        /// 启动文件夹（仅 Windows）
        /// </summary>
        StartupFolder,
        /// <summary>
        /// 桌面环境 Autostart（仅 Linux）
        /// </summary>
        AutostartDesktop,
        /// <summary>
        /// Launch Agent（仅 macOS）
        /// </summary>
        LaunchAgent
    }

    /// <summary>
    /// 启动管理器
    /// </summary>
    public class StartupManager
    {
        private readonly string appName;
        private readonly string appPath;
        private readonly StartupType startupType;

        /// <summary>
        /// 创建启动管理器
        /// </summary>
        /// <param name="applicationName">应用名称</param>
        /// <param name="applicationPath">应用程序路径</param>
        /// <param name="type">启动类型</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public StartupManager(string applicationName, string applicationPath, StartupType type)
        {
            if (string.IsNullOrEmpty(applicationName))
                throw new ArgumentException("Application name cannot be null or empty.", nameof(applicationName));

            if (string.IsNullOrEmpty(applicationPath))
                throw new ArgumentException("Application path cannot be null or empty.", nameof(applicationPath));

            if (!File.Exists(applicationPath))
                throw new FileNotFoundException("The application executable was not found.", applicationPath);

            appName = applicationName;
            appPath = applicationPath;
            startupType = type;
        }

        /// <summary>
        /// 检查当前应用是否已经设置为开机自启。
        /// </summary>
        /// <returns></returns>
        public bool IsStartupEnabled()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsStartupEnabledWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return IsStartupEnabledLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return IsStartupEnabledMacOS();
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system.");
            }
        }

        /// <summary>
        /// 启用开机自启。
        /// </summary>
        public void EnableStartup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                EnableStartupWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                EnableStartupLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                EnableStartupMacOS();
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system.");
            }
        }

        /// <summary>
        /// 禁用开机自启。
        /// </summary>
        public void DisableStartup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DisableStartupWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                DisableStartupLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                DisableStartupMacOS();
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system.");
            }
        }

        #region Windows Methods

        private const string registryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private bool IsStartupEnabledWindows()
        {
            switch (startupType)
            {
                case StartupType.Registry:
                    return IsStartupEnabledInRegistry(Registry.CurrentUser);

                case StartupType.AllUsersRegistry:
                    return IsStartupEnabledInRegistry(Registry.LocalMachine);

                case StartupType.StartupFolder:
                    return IsStartupEnabledInStartupFolder();

                default:
                    throw new InvalidOperationException("Unsupported startup type for Windows.");
            }
        }

        private void EnableStartupWindows()
        {
            switch (startupType)
            {
                case StartupType.Registry:
                    SetStartupInRegistry(Registry.CurrentUser, $"\"{appPath}\"");
                    break;

                case StartupType.AllUsersRegistry:
                    EnsureRunAsAdministrator();
                    SetStartupInRegistry(Registry.LocalMachine, $"\"{appPath}\"");
                    break;

                case StartupType.StartupFolder:
                    string shortcutPath = GetWindowsStartupShortcutPath();
                    if (!File.Exists(shortcutPath))
                    {
                        CreateWindowsShortcut(shortcutPath, appPath);
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unsupported startup type for Windows.");
            }
        }

        private void DisableStartupWindows()
        {
            switch (startupType)
            {
                case StartupType.Registry:
                    RemoveStartupFromRegistry(Registry.CurrentUser);
                    break;

                case StartupType.AllUsersRegistry:
                    EnsureRunAsAdministrator();
                    RemoveStartupFromRegistry(Registry.LocalMachine);
                    break;

                case StartupType.StartupFolder:
                    string shortcutPath = GetWindowsStartupShortcutPath();
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unsupported startup type for Windows.");
            }
        }

        private bool IsStartupEnabledInRegistry(RegistryKey registryKey)
        {
            using (RegistryKey key = registryKey.OpenSubKey(registryRunPath, writable: false))
            {
                if (key == null)
                    return false;

                object value = key.GetValue(appName);
                return value != null && value.ToString() == $"\"{appPath}\"";
            }
        }

        private bool IsStartupEnabledInStartupFolder()
        {
            string shortcutPath = GetWindowsStartupShortcutPath();
            return File.Exists(shortcutPath);
        }

        private string GetWindowsStartupShortcutPath()
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(startupFolderPath, $"{appName}.lnk");
        }

        private void SetStartupInRegistry(RegistryKey registryKey, string value)
        {
            using (RegistryKey key = registryKey.OpenSubKey(registryRunPath, writable: true) ?? registryKey.CreateSubKey(registryRunPath))
            {
                key.SetValue(appName, value);
            }
        }

        private void RemoveStartupFromRegistry(RegistryKey registryKey)
        {
            using (RegistryKey key = registryKey.OpenSubKey(registryRunPath, writable: true))
            {
                if (key != null)
                {
                    key.DeleteValue(appName, throwOnMissingValue: false);
                }
            }
        }

        private void CreateWindowsShortcut(string shortcutPath, string targetPath)
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.Description = $"{appName}启动项";
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Save();
        }

        private void EnsureRunAsAdministrator()
        {
            if (!IsRunAsAdministrator())
            {
                // 获取当前可执行文件路径
                string exeName = Process.GetCurrentProcess().MainModule.FileName;

                // 创建启动信息
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Verb = "runas" // 请求提升权限
                };

                try
                {
                    Process.Start(startInfo);
                    Environment.Exit(0); // 退出当前非管理员进程
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    throw new UnauthorizedAccessException("需要管理员权限来设置所有用户的开机自启。");
                }
            }
        }

        private bool IsRunAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Linux Methods

        private bool IsStartupEnabledLinux()
        {
            switch (startupType)
            {
                case StartupType.AutostartDesktop:
                    return IsAutostartEnabledLinux();
                default:
                    throw new InvalidOperationException("Unsupported startup type for Linux.");
            }
        }

        private void EnableStartupLinux()
        {
            switch (startupType)
            {
                case StartupType.AutostartDesktop:
                    SetAutostartLinux();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported startup type for Linux.");
            }
        }

        private void DisableStartupLinux()
        {
            switch (startupType)
            {
                case StartupType.AutostartDesktop:
                    RemoveAutostartLinux();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported startup type for Linux.");
            }
        }

        private bool IsAutostartEnabledLinux()
        {
            string autostartPath = GetLinuxAutostartFilePath();
            return File.Exists(autostartPath);
        }

        private void SetAutostartLinux()
        {
            string autostartPath = GetLinuxAutostartFilePath();
            string desktopEntry = $"[Desktop Entry]\nType=Application\nExec=\"{appPath}\"\nHidden=false\nNoDisplay=false\nX-GNOME-Autostart-enabled=true\nName={appName}\nComment=Autostart {appName}";

            Directory.CreateDirectory(Path.GetDirectoryName(autostartPath));
            File.WriteAllText(autostartPath, desktopEntry);
        }

        private void RemoveAutostartLinux()
        {
            string autostartPath = GetLinuxAutostartFilePath();
            if (File.Exists(autostartPath))
            {
                File.Delete(autostartPath);
            }
        }

        private string GetLinuxAutostartFilePath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string autostartDir = Path.Combine(home, ".config", "autostart");
            return Path.Combine(autostartDir, $"{appName}.desktop");
        }

        #endregion

        #region macOS Methods

        private bool IsStartupEnabledMacOS()
        {
            switch (startupType)
            {
                case StartupType.LaunchAgent:
                    return IsLaunchAgentEnabledMacOS();
                default:
                    throw new InvalidOperationException("Unsupported startup type for macOS.");
            }
        }

        private void EnableStartupMacOS()
        {
            switch (startupType)
            {
                case StartupType.LaunchAgent:
                    SetLaunchAgentMacOS();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported startup type for macOS.");
            }
        }

        private void DisableStartupMacOS()
        {
            switch (startupType)
            {
                case StartupType.LaunchAgent:
                    RemoveLaunchAgentMacOS();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported startup type for macOS.");
            }
        }

        private bool IsLaunchAgentEnabledMacOS()
        {
            string plistPath = GetMacOSLaunchAgentFilePath();
            return File.Exists(plistPath);
        }

        private void SetLaunchAgentMacOS()
        {
            string plistPath = GetMacOSLaunchAgentFilePath();
            string plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple Computer//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
    <key>Label</key>
    <string>{appName}</string>
    <key>ProgramArguments</key>
    <array>
        <string>{appPath}</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>";

            Directory.CreateDirectory(Path.GetDirectoryName(plistPath));
            File.WriteAllText(plistPath, plistContent);

            // 加载 Launch Agent
            Process.Start("launchctl", $"load \"{plistPath}\"");
        }

        private void RemoveLaunchAgentMacOS()
        {
            string plistPath = GetMacOSLaunchAgentFilePath();
            if (File.Exists(plistPath))
            {
                // 卸载 Launch Agent
                Process.Start("launchctl", $"unload \"{plistPath}\"");
                File.Delete(plistPath);
            }
        }

        private string GetMacOSLaunchAgentFilePath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string launchAgentsDir = Path.Combine(home, "Library", "LaunchAgents");
            return Path.Combine(launchAgentsDir, $"{appName}.plist");
        }

        #endregion
    }
}
