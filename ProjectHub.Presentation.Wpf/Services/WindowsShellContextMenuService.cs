using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// Windows Shell 上下文菜单服务
/// 用于在文件资源管理器右键菜单中添加“用 ProjectHub 打开”选项
/// </summary>
public class WindowsShellContextMenuService : ProjectHub.Application.Interfaces.IShellContextMenuService
{
    private const string RegistryKeyPath = @"Software\Classes\*\shell\ProjectHub";
    private const string DirectoryRegistryKeyPath = @"Software\Classes\Directory\shell\ProjectHub";
    private const string AllFileSystemObjectsKeyPath = @"Software\Classes\AllFileSystemObjects\shell\ProjectHub";
    private const string CommandKeyPath = @"Software\Classes\*\shell\ProjectHub\command";
    private const string DirectoryCommandKeyPath = @"Software\Classes\Directory\shell\ProjectHub\command";
    private const string AllFileSystemObjectsCommandKeyPath = @"Software\Classes\AllFileSystemObjects\shell\ProjectHub\command";
    private const string MenuText = "用 ProjectHub 打开";

    /// <summary>
    /// 注册右键菜单
    /// </summary>
    public void Register()
    {
        try
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                throw new InvalidOperationException("无法获取当前程序路径");
            }

            // 获取 exe 所在目录下的图标路径
            var exeDirectory = Path.GetDirectoryName(exePath);
            var iconPath = Path.Combine(exeDirectory!, "projecthub.ico");

            // 注册到 HKEY_CURRENT_USER（不需要管理员权限）
            // 1. 为所有文件类型注册
            RegisterForFileType(RegistryKeyPath, CommandKeyPath, exePath, iconPath);

            // 2. 为目录注册
            RegisterForFileType(DirectoryRegistryKeyPath, DirectoryCommandKeyPath, exePath, iconPath);

            // 3. 为所有文件系统对象注册
            RegisterForFileType(AllFileSystemObjectsKeyPath, AllFileSystemObjectsCommandKeyPath, exePath, iconPath);

            Debug.WriteLine($"ProjectHub 右键菜单已注册: {exePath}");
            Debug.WriteLine($"图标路径: {iconPath}");

            // 刷新 Shell 图标缓存
            RefreshShell();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"注册右键菜单失败: {ex.Message}");
        }
    }

    private void RegisterForFileType(string keyPath, string commandKeyPath, string exePath, string iconPath)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                key.SetValue("", MenuText);
                // 如果图标文件存在，设置图标路径；否则不设置（使用默认图标）
                if (File.Exists(iconPath))
                {
                    key.SetValue("Icon", iconPath);
                }
                //key.SetValue("Position", "Top");  // 将菜单项放在顶部
            }

            // 注册命令
            using var commandKey = Registry.CurrentUser.CreateSubKey(commandKeyPath);
            if (commandKey != null)
            {
                // %1 代表选中的文件路径
                var command = $"\"{exePath}\" --open-file \"%1\"";
                commandKey.SetValue("", command);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"注册 {keyPath} 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 注销右键菜单
    /// </summary>
    public void Unregister()
    {
        try
        {
            // 删除所有注册的路径（如果存在）
            if (Registry.CurrentUser.OpenSubKey(RegistryKeyPath) != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, false);
                Debug.WriteLine($"已删除: {RegistryKeyPath}");
            }
            
            if (Registry.CurrentUser.OpenSubKey(DirectoryRegistryKeyPath) != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(DirectoryRegistryKeyPath, false);
                Debug.WriteLine($"已删除: {DirectoryRegistryKeyPath}");
            }
            
            if (Registry.CurrentUser.OpenSubKey(AllFileSystemObjectsKeyPath) != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(AllFileSystemObjectsKeyPath, false);
                Debug.WriteLine($"已删除: {AllFileSystemObjectsKeyPath}");
            }
            
            Debug.WriteLine("ProjectHub 右键菜单已注销");
            
            // 强制刷新 Shell
            RefreshShell();
            
            // 额外延迟刷新，确保生效
            System.Threading.Thread.Sleep(100);
            RefreshShell();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"注销右键菜单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查是否已注册
    /// </summary>
    public bool IsRegistered()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    #region Shell 刷新
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    private const int SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    /// <summary>
    /// 刷新 Windows Shell，使右键菜单更改生效
    /// </summary>
    private void RefreshShell()
    {
        try
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"刷新 Shell 失败: {ex.Message}");
        }
    }
    #endregion
}
