using System.Diagnostics;
using System.Windows;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using ProjectHub.Presentation.Wpf.Services;
using ReactiveUI;

namespace ProjectHub.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Note: This is a code-behind file that only handles view-specific logic.
    /// All business logic should be in the ViewModel (MVVM pattern).
    /// </summary>
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        private readonly TrayIconService _trayIconService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly LocalizedStrings _localizedStrings;
        private bool _isClosing;

        public MainWindow(MainViewModel viewModel, IAppSettingsService appSettingsService, LocalizedStrings localizedStrings, TrayIconService trayIconService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _appSettingsService = appSettingsService;
            _localizedStrings = localizedStrings;
            // 初始化托盘图标服务
            var settings = _appSettingsService.Load();
            _trayIconService = trayIconService;
            _trayIconService.ShowWindowRequested += OnShowWindowRequested;
            _trayIconService.ExitRequested += OnExitRequested;
            _trayIconService.Show();
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        private void OnShowWindowRequested(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            });
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        private void OnExitRequested(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _isClosing = true;
                Close();
            });
        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            // 如果已经确定要关闭，直接返回
            if (_isClosing)
            {
                _trayIconService?.Dispose();
                return;
            }

            // 检查是否启用最小化到托盘
            var settings = _appSettingsService?.Load();
            if (settings?.MinimizeToTray == true && _trayIconService != null)
            {
                // 取消关闭，隐藏窗口
                e.Cancel = true;
                Hide();
                _trayIconService.ShowBalloonTip("ProjectHub", _localizedStrings?.Tray_MinimizedMessage ?? "程序已最小化到托盘");
            }
            else
            {
                // 直接关闭
                _trayIconService?.Dispose();
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _trayIconService?.Dispose();
        }
    }
}