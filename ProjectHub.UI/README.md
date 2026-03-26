# ProjectHub UI Layer (ReactiveUI Implementation)

## Responsibilities
- **Views**: XAML view definitions
- **ViewModels**: MVVM pattern using ReactiveUI
- **Converters**: Value converters
- **Behaviors**: Attached behaviors
- **Controls**: Custom controls

## Design Principles
- **UI Framework Agnostic**: Ready for Avalonia migration
- **Dependency Injection**: ViewModels resolved through DI container
- **Reactive Programming**: Uses ReactiveUI for reactive MVVM pattern
- **Testability**: ViewModels do not directly depend on WPF/Avalonia APIs

## Architecture

### ViewModel Hierarchy

```
ViewModelBase (ReactiveObject)
├── MainViewModel
│   ├── Properties: Projects, Groups, Tags, SelectedGroup, SearchKeyword
│   └── Commands: LoadProjectsCommand, SearchProjectsCommand, RefreshCommand
├── ProjectViewModel
│   └── Properties: Id, Name, Type, Path, GroupName, Tags, etc.
├── GroupViewModel
│   └── Properties: Id, Name, Description, ProjectCount, etc.
└── TagViewModel
    └── Properties: Id, Name, Color, ProjectCount
```

### Base Class: ViewModelBase

All ViewModels inherit from `ViewModelBase` which provides:

- **ReactiveUI Integration**: Inherits from `ReactiveObject`
- **Common Properties**: Title, IsLoading, ErrorMessage, HasError
- **Error Handling**: `ExecuteWithErrorHandlerAsync()` method
- **Command Factory**: `CreateCommand()` helper methods

```csharp
public abstract class ViewModelBase : ReactiveObject
{
    // Common properties with change notification
    public string Title { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError { get; set; }
    
    // Error handling helper
    protected Task ExecuteWithErrorHandlerAsync(Func<Task> action, string operationName);
    
    // Command factory
    protected ReactiveCommand<Unit, Unit> CreateCommand(Func<Task> executeAction);
}
```

## ReactiveUI Usage

### Creating Commands

Use the `CreateCommand()` helper method with **method group syntax**:

```csharp
// ✅ Correct - Method group syntax
LoadProjectsCommand = CreateCommand(LoadProjectsAsync);

// ❌ Wrong - Don't use lambda with underscore for parameterless methods
LoadProjectsCommand = CreateCommand(_ => LoadProjectsAsync());
```

### Property Change Notification

Use `RaiseAndSetIfChanged()` for property setters:

```csharp
private string _searchKeyword = string.Empty;
public string SearchKeyword
{
    get => _searchKeyword;
    set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
}
```

### Async Command Execution

Commands created with `CreateCommandFromTask()` handle async/await automatically:

```csharp
public ReactiveCommand<Unit, Unit> LoadProjectsCommand { get; }

public MainViewModel(ILogger<MainViewModel> logger)
    : base(logger)
{
    LoadProjectsCommand = CreateCommand(LoadProjectsAsync);
}

private async Task LoadProjectsAsync()
{
    await ExecuteWithErrorHandlerAsync(async () =>
    {
        // Your async logic here
        var projects = await _projectAppService.GetAllAsync();
        Projects = new ObservableCollection<ProjectViewModel>(
            projects.Select(p => new ProjectViewModel(p))
        );
    }, "Load Projects");
}
```

## ⚠️ Important: Avalonia Migration Preparation

### Current Implementation Uses WPF, But Note:

1. **DO NOT** use WPF-specific APIs in ViewModels
2. **DO NOT** reference UI libraries in Models
3. Keep all UI-specific logic in Views/Converters/Behaviors
4. Use ReactiveUI (supports both WPF and Avalonia)
5. Data binding syntax remains consistent

### Migration Points When Switching to Avalonia:

- XAML namespace declarations (`xmlns`)
- Some control name differences
- Styling system variations
- Rendering engine difference (DirectX vs Skia)

## File Structure

```
ProjectHub.UI/
├── ViewModels/
│   ├── ViewModelBase.cs          # Base class for all ViewModels
│   ├── MainViewModel.cs          # Main window ViewModel
│   ├── ProjectViewModel.cs       # Project presentation model
│   └── GroupAndTagViewModels.cs  # Group and Tag presentation models
├── Views/
│   └── MainWindow.xaml           # Main window view (to be implemented)
├── Converters/
│   └── (to be added)             # Value converters
└── Behaviors/
    └── (to be added)             # Attached behaviors
```

## Dependencies

### NuGet Packages

- **ReactiveUI.WPF** (20.1.1) - Reactive MVVM framework
- **ReactiveMarbles.ObservableEvents.SourceGenerator** (1.3.1) - Observable event generation
- **System.Reactive** (6.0.1) - Reactive Extensions
- **Microsoft.Extensions.Logging.Abstractions** (8.0.0) - Logging abstraction

### Project References

- ProjectHub.Core
- ProjectHub.Domain
- ProjectHub.Application

## Testing

### Unit Testing ViewModels

ViewModels are designed for testability:

```csharp
[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public async Task LoadProjectsCommand_ShouldLoadProjects()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MainViewModel>>();
        var viewModel = new MainViewModel(mockLogger.Object);
        
        // Act
        await viewModel.LoadProjectsCommand.Execute(Unit.Default);
        
        // Assert
        Assert.IsTrue(viewModel.Projects.Count > 0);
        Assert.IsFalse(viewModel.IsLoading);
        Assert.IsFalse(viewModel.HasError);
    }
}
```

## Best Practices

### DO:
- ✅ Use method group syntax for commands: `CreateCommand(MethodAsync)`
- ✅ Use `RaiseAndSetIfChanged()` for property changes
- ✅ Handle errors with `ExecuteWithErrorHandlerAsync()`
- ✅ Keep ViewModels platform-agnostic
- ✅ Inject dependencies through constructor
- ✅ Use observable collections for UI-bound lists

### DON'T:
- ❌ Use WPF/Avalonia specific types in ViewModels
- ❌ Create commands with `_ => MethodAsync()` for parameterless methods
- ❌ Manually raise PropertyChanged events (use RaiseAndSetIfChanged)
- ❌ Reference UI frameworks directly in ViewModels
- ❌ Put business logic in ViewModels (delegate to Application Services)

## Future Enhancements

### To Be Added:
1. **Value Converters**: For data formatting and conversion
2. **Behaviors**: For attaching complex behavior to UI elements
3. **Dialog Services**: For showing dialogs/message boxes
4. **Navigation Service**: For view navigation (if needed)
5. **Validation**: Implement INotifyDataErrorInfo for validation

---

<div align="center">

**🎊 ProjectHub UI Layer - ReactiveUI MVVM Implementation**

**Cross-platform ready! Supports both WPF and Avalonia!** 🚀

</div>
