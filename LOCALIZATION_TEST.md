# 本地化功能测试 / Localization Test

## 测试步骤 / Test Steps

### 1. 启动应用
```
d:/work/ProjectHub> dotnet run --project ProjectHub.Presentation.Wpf
```

### 2. 测试功能
- 点击 "切换到中文" 按钮
- 点击 "Switch to English" 按钮
- 观察下方文本是否正确切换

### 3. 预期结果

#### 切换到中文时：
```
当前语言: zh-CN
测试文本: 创建项目
```

#### 切换到英文时：
```
当前语言: en-US
测试文本: [Project_Create]  (如果英文资源未定义则显示 key)
```

## 代码结构

### ViewModel (MainViewModel.cs)
```csharp
// 注入本地化服务
private readonly ILocalizationService _localizationService;

// 切换语言命令
public ReactiveCommand<Unit, Unit> SwitchToChineseCommand { get; }
public ReactiveCommand<Unit, Unit> SwitchToEnglishCommand { get; }

// 订阅语言变更
_localizationService.CultureChanged
    .Subscribe(_ => UpdateTestText())
    .DisposeWith(Disposables);
```

### XAML (MainWindow.xaml)
```xml
<!-- 切换语言按钮 -->
<Button Content="切换到中文" Command="{Binding SwitchToChineseCommand}" />
<Button Content="Switch to English" Command="{Binding SwitchToEnglishCommand}" />

<!-- 显示当前语言和测试文本 -->
<TextBlock Text="{Binding CurrentLanguageText}" />
<TextBlock Text="{Binding TestText}" />
```

## 支持的语言 / Supported Languages

- `zh-CN` - 简体中文 (默认)
- `en-US` - 英语

## 添加新语言

1. 在 `ProjectHub.Resources/Strings/` 下创建新的 `.resx` 文件
   - `Strings.ja-JP.resx` (日语)
   - `Strings.ko-KR.resx` (韩语)
   - `Strings.fr-FR.resx` (法语)

2. 添加对应的翻译内容

3. 在 ViewModel 中添加切换命令
   ```csharp
   SwitchToJapaneseCommand = CreateCommand(() => 
       _localizationService.SetCultureAsync(new CultureInfo("ja-JP")));
   ```

## 注意事项 / Notes

- 所有资源文件使用相同的 Key
- 找不到资源时会显示 `[KeyName]` 方便调试
- 语言切换会自动触发所有订阅 `CultureChanged` 的 UI 刷新
