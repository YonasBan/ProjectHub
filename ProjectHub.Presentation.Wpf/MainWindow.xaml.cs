using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using ProjectHub.Presentation.Wpf.Helpers;
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
        private readonly ProjectHub.Application.Interfaces.IDialogService _dialogService;
        private readonly ProjectHub.Application.Interfaces.IFileAssociationService _fileAssociationService;
        private bool _isClosing;

        public MainWindow(MainViewModel viewModel, IAppSettingsService appSettingsService, LocalizedStrings localizedStrings, TrayIconService trayIconService, ProjectHub.Application.Interfaces.IDialogService dialogService, ProjectHub.Application.Interfaces.IFileAssociationService fileAssociationService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _appSettingsService = appSettingsService;
            _localizedStrings = localizedStrings;
            _dialogService = dialogService;
            _fileAssociationService = fileAssociationService;
            // 初始化托盘图标服务
            var settings = _appSettingsService.Load();
            _trayIconService = trayIconService;
            _trayIconService.ShowWindowRequested += OnShowWindowRequested;
            _trayIconService.ExitRequested += OnExitRequested;
            _trayIconService.Show();

            // 处理从右键菜单启动时的文件路径参数
            HandleCommandLineArgs();
            
            // 注册窗口消息处理，用于接收其他实例发送的参数
            SourceInitialized += MainWindow_SourceInitialized;
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

        #region 命令行参数处理

        /// <summary>
        /// 处理命令行参数（从右键菜单启动时传入文件路径）
        /// </summary>
        private async void HandleCommandLineArgs()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                
                // 查找 --open-file 参数
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--open-file" && i + 1 < args.Length)
                    {
                        var filePath = args[i + 1];
                        Debug.WriteLine($"从右键菜单打开文件: {filePath}");
                        
                        // 等待窗口加载完成后打开添加项目对话框
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await OpenAddProjectDialogWithFile(filePath);
                        }, System.Windows.Threading.DispatcherPriority.Loaded);
                        
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理命令行参数失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开添加项目对话框并预填充文件路径
        /// </summary>
        private async Task OpenAddProjectDialogWithFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath) && !System.IO.Directory.Exists(filePath))
                {
                    Debug.WriteLine($"文件不存在: {filePath}");
                    return;
                }

                // 获取 ViewModel
                var viewModel = DataContext as MainViewModel;
                if (viewModel == null)
                {
                    Debug.WriteLine("无法获取 MainViewModel");
                    return;
                }
                
                // 创建 ProjectDialogViewModel
                var projectDialogVm = new ProjectHub.Application.ViewModels.DialogViewModel.ProjectDialogViewModel(
                    null!, // logger
                    _dialogService, 
                    _fileAssociationService,
                    null // projectAppService
                );
                projectDialogVm.InitializeForAdd();
                
                // 预填充文件路径
                projectDialogVm.ProjectPath = filePath;
                
                // 自动提取项目名称
                if (string.IsNullOrWhiteSpace(projectDialogVm.ProjectName))
                {
                    projectDialogVm.ProjectName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                }
                
                // 如果是文件夹，设置为 OpenFolder 模式
                if (System.IO.Directory.Exists(filePath))
                {
                    projectDialogVm.LaunchType = ProjectHub.Domain.Entities.LaunchType.OpenFolder;
                }
                else
                {
                    // 检测默认程序
                    var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                    var defaultProgram = _fileAssociationService.GetDefaultProgramByExtension(extension);
                    if (!string.IsNullOrEmpty(defaultProgram))
                    {
                        projectDialogVm.DefaultProgram = defaultProgram;
                    }
                }
                
                // 显示对话框
                var result = await _dialogService.ShowDialogAsync<ProjectHub.Application.ViewModels.DialogViewModel.ProjectDialogViewModel, ProjectHub.Application.DTOs.ProjectDto?>(projectDialogVm);
                
                if (result.Confirmed && result.Value != null)
                {
                    Debug.WriteLine($"成功添加项目: {result.Value.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开添加项目对话框失败: {ex.Message}");
            }
        }

        #endregion

        #region 窗口控制按钮事件

        /// <summary>
        /// 标题栏拖动
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // 双击标题栏最大化/还原
                MaximizeRestoreButton_Click(null, null);
            }
            else
            {
                // 如果窗口是最大化状态，先还原
                if (WindowState == WindowState.Maximized)
                {
                    // 计算鼠标位置相对于窗口的比例
                    var mousePos = e.GetPosition(this);
                    var widthRatio = mousePos.X / ActualWidth;
                    
                    // 还原窗口
                    WindowState = WindowState.Normal;
                    
                    // 计算新的窗口位置，使鼠标保持在标题栏的相同相对位置
                    var newLeft = mousePos.X - (widthRatio * RestoreBounds.Width);
                    Left = newLeft;
                    Top = 0;
                }
                
                // 拖动窗口
                DragMove();
            }
        }

        /// <summary>
        /// 最小化按钮
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 最大化/还原按钮
        /// </summary>
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeRestoreButton.Content = "\xE739"; // 最大化图标
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeRestoreButton.Content = "\xE923"; // 还原图标
            }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region 窗口消息处理

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_COPYDATA)
            {
                Debug.WriteLine($"接收到 WM_COPYDATA 消息");
                
                var copyData = Marshal.PtrToStructure<Win32Helper.COPYDATASTRUCT>(lParam);
                var data = Marshal.PtrToStringUni(copyData.lpData);
                
                Debug.WriteLine($"接收到的数据: {data}");
                
                if (!string.IsNullOrEmpty(data))
                {
                    // 解析参数
                    var args = data.Split('|');
                    Debug.WriteLine($"解析后的参数数量: {args.Length}");
                    
                    // 查找 --open-file 参数后面的文件路径
                    string? filePath = null;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "--open-file" && i + 1 < args.Length)
                        {
                            // 清理文件路径，移除可能的引号和空白字符
                            filePath = args[i + 1].Trim('"', ' ', '\t', '\r', '\n');
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(filePath) && (System.IO.File.Exists(filePath) || System.IO.Directory.Exists(filePath)))
                    {
                        Debug.WriteLine($"找到有效文件/文件夹: {filePath}，打开对话框");
                        
                        // 显示窗口
                        Show();
                        WindowState = WindowState.Normal;
                        Activate();
                        
                        // 异步打开对话框
                        _ = OpenAddProjectDialogWithFile(filePath);
                    }
                    else
                    {
                        Debug.WriteLine($"未找到有效的文件路径或文件不存在");
                        Debug.WriteLine($"清理后的文件路径: '{filePath}'");
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            Debug.WriteLine($"文件是否存在: {System.IO.File.Exists(filePath)}");
                            Debug.WriteLine($"文件夹是否存在: {System.IO.Directory.Exists(filePath)}");
                        }
                        // 即使没有文件，也要显示窗口
                        Show();
                        WindowState = WindowState.Normal;
                        Activate();
                    }
                    
                    handled = true;
                    return IntPtr.Zero;
                }
            }
            
            return IntPtr.Zero;
        }

        #endregion
    }
}