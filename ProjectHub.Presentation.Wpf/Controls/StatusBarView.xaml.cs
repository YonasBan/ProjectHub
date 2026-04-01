using System.Windows.Controls;
using ProjectHub.Application.Localization;
using Splat;

namespace ProjectHub.Presentation.Wpf.Controls;

public partial class StatusBarView : UserControl
{
    private readonly LocalizedStrings _l = Locator.Current.GetService<LocalizedStrings>()!;
    public LocalizedStrings L => _l;

    public StatusBarView()
    {
        InitializeComponent();
        InitializeTexts();
    }

    private void InitializeTexts()
    {
        StatusReadyText.Text = L.Status_Ready;
        ProjectCountText.Text = string.Format(L.Status_ProjectCount, 45);
        FolderCountText.Text = string.Format(L.Status_FolderCount, 15);
        WorkspaceCountText.Text = string.Format(L.Status_WorkspaceCount, 8);
        TagCountText.Text = string.Format(L.Status_TagCount, 8);
        TaggedProjectsText.Text = string.Format(L.Status_TaggedProjects, 156);
    }
}
