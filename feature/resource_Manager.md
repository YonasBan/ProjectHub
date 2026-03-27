好问题，这是跨平台项目里最容易一团乱的部分。核心思路是：**资源文件放共享库，平台只管绑定语法**。

---

## 整体架构

```
MyApp.Resources/          ← 独立类库，存所有 .resx 文件
MyApp.Application/        ← ILocalizationService 接口
MyApp.Presentation.Wpf/   ← WPF 绑定实现
MyApp.Presentation.Avalonia/  ← Avalonia 绑定实现  
MyApp.Presentation.Maui/      ← MAUI 绑定实现
```

---

## 第一步：共享资源库

新建一个普通 Class Library，放所有 `.resx`：

```
MyApp.Resources/
  Strings.resx          ← 默认（中文或英文）
  Strings.en-US.resx
  Strings.zh-CN.resx
  Strings.ja-JP.resx
```

`.resx` 内容示例：

```xml
<!-- Strings.zh-CN.resx -->
<data name="Confirm" xml:space="preserve">
  <value>确认</value>
</data>
<data name="DeleteConfirm" xml:space="preserve">
  <value>确定要删除「{0}」吗？</value>
</data>
<data name="Order_PlaceSuccess" xml:space="preserve">
  <value>下单成功，订单号：{0}</value>
</data>
```

资源文件命名用 `模块_Key` 的方式，避免一个文件太大。

---

## 第二步：Application 层定义接口

```csharp
// Application/Interfaces/ILocalizationService.cs
public interface ILocalizationService
{
    string this[string key] { get; }
    string GetFormatted(string key, params object[] args);

    // 运行时切换语言
    CultureInfo CurrentCulture { get; }
    Task SetCultureAsync(CultureInfo culture);

    // 语言切换时通知 ViewModel 刷新
    IObservable<CultureInfo> CultureChanged { get; }
}
```

```csharp
// 给 ViewModel 用的扩展，方便格式化
public static class LocalizationExtensions
{
    public static string Fmt(this ILocalizationService loc, string key, params object[] args)
        => string.Format(loc[key], args);
}
```

---

## 第三步：共享实现（三个平台都能用）

用 `Microsoft.Extensions.Localization`，三个平台行为完全一致：

```csharp
// 放在 MyApp.Resources 或 Application 里
public class ResxLocalizationService : ILocalizationService, INotifyPropertyChanged
{
    private readonly ResourceManager _resourceManager;
    private readonly Subject<CultureInfo> _cultureChanged = new();

    public ResxLocalizationService()
    {
        // 指向共享资源库的 Strings 类
        _resourceManager = new ResourceManager(
            "MyApp.Resources.Strings",
            typeof(Strings).Assembly);
    }

    public string this[string key]
    {
        get
        {
            var value = _resourceManager.GetString(key, CurrentCulture);
            // 找不到 key 时不崩溃，返回 [key] 方便调试
            return value ?? $"[{key}]";
        }
    }

    public string GetFormatted(string key, params object[] args)
        => string.Format(this[key], args);

    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

    public IObservable<CultureInfo> CultureChanged => _cultureChanged.AsObservable();

    public async Task SetCultureAsync(CultureInfo culture)
    {
        CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;

        _cultureChanged.OnNext(culture);

        // MAUI 需要额外处理主线程
        await Task.CompletedTask;
    }
}
```

三个平台注册同一个实现：

```csharp
services.AddSingleton<ILocalizationService, ResxLocalizationService>();
```

---

## 第四步：ViewModel 里使用

```csharp
public class DeleteConfirmViewModel : DialogViewModelBase<bool>
{
    private readonly ILocalizationService _loc;

    public string Title => _loc["DeleteConfirm_Title"];
    public string Message => _loc.GetFormatted("DeleteConfirm_Message", ItemName);
    public string ConfirmText => _loc["Confirm"];
    public string CancelText => _loc["Cancel"];

    public string ItemName { get; }

    public DeleteConfirmViewModel(string itemName, ILocalizationService loc)
    {
        ItemName = itemName;
        _loc = loc;

        // 语言切换时刷新所有属性
        loc.CultureChanged
           .ObserveOn(RxApp.MainThreadScheduler)
           .Subscribe(_ => this.RaisePropertyChanged(string.Empty)) // 空字符串 = 通知所有属性
           .DisposeWith(Disposables);

        ConfirmCommand = ReactiveCommand.Create(() => Close(true));
    }
}
```

---

## 第五步：XAML 绑定（各平台差异在这里）

### Avalonia

```csharp
// 注册一个全局的 Localizer 供 XAML 用
public class Localizer : ReactiveObject
{
    private static Localizer? _instance;
    public static Localizer Instance => _instance ??= new();

    private readonly ILocalizationService _loc;

    public Localizer()
    {
        _loc = Locator.Current.GetService<ILocalizationService>()!;
        _loc.CultureChanged
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Item)));
    }

    // XAML 用索引器访问
    public string this[string key] => _loc[key];

    // 让 XAML 绑定能感知到属性变化
    public Indexer Item => new(_loc);
}

public class Indexer : ReactiveObject
{
    private readonly ILocalizationService _loc;
    public Indexer(ILocalizationService loc) => _loc = loc;
    public string this[string key] => _loc[key];
}
```

```xml
<!-- Avalonia XAML -->
<Window xmlns:loc="clr-namespace:MyApp.Localization">
  <!-- 方式一：绑定 ViewModel 属性（推荐，最干净） -->
  <TextBlock Text="{Binding Title}"/>
  
  <!-- 方式二：直接在 XAML 里用 Localizer -->
  <Button Content="{Binding [Confirm], 
          Source={x:Static loc:Localizer.Instance}}"/>
</Window>
```

### MAUI

```csharp
// MAUI 有内置的 IStringLocalizer 支持，或用 markup extension
public class TranslateExtension : IMarkupExtension<string>
{
    private static ILocalizationService? _loc;

    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        _loc ??= serviceProvider.GetService<ILocalizationService>();
        return _loc?[Key] ?? $"[{Key}]";
    }

    object IMarkupExtension.ProvideValue(IServiceProvider sp) => ProvideValue(sp);
}
```

```xml
<!-- MAUI XAML -->
<ContentPage xmlns:loc="clr-namespace:MyApp.Maui.Localization">
  <!-- 方式一：绑定 ViewModel 属性（推荐） -->
  <Label Text="{Binding Title}"/>
  
  <!-- 方式二：Markup Extension（静态文本用这个） -->
  <Button Text="{loc:Translate Key=Confirm}"/>
</ContentPage>
```

---

## 运行时切换语言

```csharp
// 语言设置 ViewModel
public class LanguageSettingsViewModel : ReactiveObject
{
    private readonly ILocalizationService _loc;

    public IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("中文", "zh-CN"),
        new("English", "en-US"),
        new("日本語", "ja-JP"),
    ];

    public ReactiveCommand<LanguageOption, Unit> SwitchLanguageCommand { get; }

    public LanguageSettingsViewModel(ILocalizationService loc)
    {
        _loc = loc;

        SwitchLanguageCommand = ReactiveCommand.CreateFromTask<LanguageOption>(
            async lang => await _loc.SetCultureAsync(
                new CultureInfo(lang.Code)));
    }
}

public record LanguageOption(string DisplayName, string Code);
```

---

## 各平台差异总结

| | Avalonia | MAUI |
|---|---|---|
| 资源文件 | 共享 .resx ✅ | 共享 .resx ✅ |
| XAML 静态文本 | `x:Static` 或 Localizer 绑定 | `TranslateExtension` |
| ViewModel 属性绑定 | 完全一样 ✅ | 完全一样 ✅ |
| 运行时切换 | 通知 RaisePropertyChanged | 同上，需确保主线程 |
| 特殊注意 | 无 | iOS/Android 需设置 `Thread.CurrentThread.CurrentUICulture` |

**绝大部分代码（资源文件 + 接口 + ViewModel）三个平台完全共享，只有 XAML 绑定语法略有差异**，而且 ViewModel 属性绑定那条路在所有平台上写法完全一致，推荐优先用这种方式。