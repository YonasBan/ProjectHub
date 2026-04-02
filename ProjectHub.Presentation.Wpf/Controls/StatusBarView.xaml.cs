using System.Windows.Controls;

namespace ProjectHub.Presentation.Wpf.Controls;

/// <summary>
/// 状态栏视图 (WPF 特定实现)
/// 
/// DDD 设计要点:
/// - 位于 Presentation 层，仅包含 WPF 特定代码
/// - 所有数据通过绑定从 MainViewModel 获取
/// - 语言切换由 ViewModelBase 中的订阅自动处理
/// </summary>
public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
    }
}
