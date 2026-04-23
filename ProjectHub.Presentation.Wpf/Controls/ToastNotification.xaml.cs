using ProjectHub.Application.Interfaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProjectHub.Presentation.Wpf.Controls;

/// <summary>
/// Toast 通知弹出框 - 自动消失的通知提示
/// </summary>
public partial class ToastNotification : Window
{
    private readonly int _durationMs;
    private readonly System.Timers.Timer _timer;

    public ToastNotification(string message, NotificationType type, int durationMs = 3000)
    {
        this.Owner = System.Windows.Application.Current.MainWindow;
        InitializeComponent();
        _durationMs = durationMs;

        // 设置消息
        MessageTextBlock.Text = message;

        // 根据类型设置图标和颜色
        SetupByType(type);

        // 创建定时器
        _timer = new System.Timers.Timer(durationMs);
        _timer.Elapsed += (s, e) =>
        {
            _timer.Stop();
            Dispatcher.Invoke(CloseAnimation);
        };
        _timer.AutoReset = false;
    }

    /// <summary>
    /// 根据通知类型设置样式
    /// </summary>
    private void SetupByType(NotificationType type)
    {
        var resources = Resources;

        switch (type)
        {
            case NotificationType.Success:
                IconPath.Data = (PathGeometry)resources["SuccessIcon"];
                IconPath.Fill = (Brush)FindResource("SuccessBrush") ?? Brushes.Green;
                break;
            case NotificationType.Warning:
                IconPath.Data = (PathGeometry)resources["WarningIcon"];
                IconPath.Fill = (Brush)FindResource("WarningBrush") ?? Brushes.Orange;
                break;
            case NotificationType.Error:
                IconPath.Data = (PathGeometry)resources["ErrorIcon"];
                IconPath.Fill = (Brush)FindResource("ErrorBrush") ?? Brushes.Red;
                break;
            default: // Info
                IconPath.Data = (PathGeometry)resources["InfoIcon"];
                IconPath.Fill = (Brush)FindResource("AccentBlueBrush") ?? Brushes.Blue;
                break;
        }
    }

    /// <summary>
    /// 显示通知
    /// </summary>
    public new void Show()
    {
        // 先显示窗口以计算实际大小
        base.Show();
        
        // 更新布局以获取实际高度
        UpdateLayout();
        
        // 定位到主窗体中间
        PositionWindow();

        // 启动进入动画
        BeginAnimation();

        // 启动定时器
        _timer.Start();
    }

    /// <summary>
    /// 定位窗口到主窗体中间
    /// </summary>
    private void PositionWindow()
    {
        // 获取主窗体
        var mainWindow = System.Windows.Application.Current.MainWindow;
        
        if (mainWindow != null && mainWindow.IsLoaded)
        {
            // 在主窗体中间显示
            Left = mainWindow.Left + (mainWindow.Width - Width) / 2;
            Top = mainWindow.Top + (mainWindow.Height - Height) / 2;
        }
        else
        {
            // 主窗体不可用时，在屏幕中间显示
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    /// <summary>
    /// 进入动画
    /// </summary>
    private void BeginAnimation()
    {
        // 简单的淡入动画
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };

        BeginAnimation(OpacityProperty, fadeAnimation);
    }

    /// <summary>
    /// 关闭动画
    /// </summary>
    private void CloseAnimation()
    {
        Dispatcher.Invoke(() =>
        {
            // 淡出动画
            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            fadeAnimation.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fadeAnimation);
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _timer?.Dispose();
    }
}
