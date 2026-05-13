using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using ProjectHub.Application.Localization;

namespace ProjectHub.Presentation.Wpf.Services;

/// <summary>
/// 系统托盘服务 - 管理通知区域图标和菜单
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly LocalizedStrings _localizedStrings;
    private bool _isDisposed;

    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ExitRequested;

    public TrayIconService(LocalizedStrings localizedStrings)
    {
        _localizedStrings = localizedStrings;

        // 创建托盘图标
        _taskbarIcon = new TaskbarIcon
        {
            IconSource = new System.Windows.Media.Imaging.BitmapImage(
                new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "projecthub.ico"), UriKind.Absolute)),
            ToolTipText = "ProjectHub"
        };

        // 创建上下文菜单
        var contextMenu = new System.Windows.Controls.ContextMenu();
        
        var showItem = new System.Windows.Controls.MenuItem { Header = _localizedStrings.Tray_ShowWindow };
        showItem.Click += (s, e) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(showItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = _localizedStrings.Tray_Exit };
        exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = contextMenu;

        // 双击托盘图标显示窗口
        _taskbarIcon.TrayMouseDoubleClick += (s, e) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 显示托盘图标
    /// </summary>
    public void Show()
    {
        _taskbarIcon.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 隐藏托盘图标
    /// </summary>
    public void Hide()
    {
        _taskbarIcon.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 显示气球提示
    /// </summary>
    public void ShowBalloonTip(string title, string message)
    {
        _taskbarIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _taskbarIcon.Dispose();
            _isDisposed = true;
        }
    }
}
