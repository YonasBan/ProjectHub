using System.Windows;
using System.Windows.Controls;
using ProjectHub.Application.Interfaces;
using ProjectHub.Application.Localization;
using ProjectHub.Application.ViewModels;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class StatusBarView : UserControl
{
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    private readonly ILocalizationService? _localizationService = Locator.Current.GetService<ILocalizationService>();
    public LocalizedStrings L => _l;

    public StatusBarView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        
        // 订阅语言改变事件
        _localizationService?.CultureChanged.Subscribe(_ => 
        {
            if (DataContext is MainViewModel vm)
                UpdateTexts(vm);
        });
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Projects.CollectionChanged += (_, _) => UpdateTexts(vm);
            vm.WorkFolders.CollectionChanged += (_, _) => UpdateTexts(vm);
            vm.WorkSpaces.CollectionChanged += (_, _) => UpdateTexts(vm);
            UpdateTexts(vm);
        }
    }

    private void UpdateTexts(MainViewModel vm)
    {
        StatusReadyText.Text = L.Status_Ready;
        ProjectCountText.Text = string.Format(L.Status_ProjectCount, vm.TotalProjectCount);
        FolderCountText.Text = string.Format(L.Status_FolderCount, vm.TotalFolderCount);
        WorkspaceCountText.Text = string.Format(L.Status_WorkspaceCount, vm.TotalWorkspaceCount);
        TagCountText.Text = string.Format(L.Status_TagCount, vm.TagCount);
        TaggedProjectsText.Text = string.Format(L.Status_TaggedProjects, vm.FavoriteProjectCount);
    }
}
