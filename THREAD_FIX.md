# WPF 跨线程问题修复 / WPF Cross-Thread Fix

## 问题 / Problem

点击切换语言按钮时出现错误：
```
System.InvalidOperationException: 调用线程无法访问此对象，因为另一个线程拥有该对象。
```

## 原因 / Root Cause

WPF 的 UI 元素只能由 UI 线程（Dispatcher 线程）访问。当 `CultureChanged` 事件触发时，如果订阅回调在后台线程执行，会导致此错误。

## 解决方案 / Solution

### 1. 使用 `ObserveOn` 确保在 UI 线程执行订阅回调

```csharp
// ❌ 错误 - 可能在后台线程执行
_localizationService.CultureChanged
    .Subscribe(_ => UpdateTestText())
    .DisposeWith(Disposables);

// ✅ 正确 - 确保在 UI 线程执行
_localizationService.CultureChanged
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(_ => UpdateTestText())
    .DisposeWith(Disposables);
```

### 2. 命令执行确保输出到 UI 线程

```csharp
// ❌ 错误 - 默认在线程池执行
SwitchToChineseCommand = CreateCommand(SwitchToChineseAsync);

// ✅ 正确 - 明确指定输出调度器
SwitchToChineseCommand = ReactiveCommand.CreateFromTask(
    SwitchToChineseAsync,
    outputScheduler: RxApp.MainThreadScheduler);
```

## ReactiveUI 调度器 / ReactiveUI Schedulers

| 调度器 | 用途 | 说明 |
|---------|------|------|
| `RxApp.MainThreadScheduler` | UI 线程 | 在 UI 线程执行，更新 UI 属性 |
| `TaskPoolScheduler.Default` | 后台线程池 | 执行耗时任务 |
| `ImmediateScheduler.Instance` | 立即执行 | 在当前线程同步执行 |

## 最佳实践 / Best Practices

### 1. 更新 UI 属性
```csharp
// 使用 ObserveOn(RxApp.MainThreadScheduler)
someObservable
    .ObserveOn(RxApp.MainThreadScheduler)
    .Subscribe(value => MyUiProperty = value);
```

### 2. 命令创建
```csharp
// 如果命令执行会更新 UI，使用 outputScheduler
public ReactiveCommand<Unit, Unit> MyCommand { get; }

MyCommand = ReactiveCommand.CreateFromTask(
    ExecuteAsync,
    outputScheduler: RxApp.MainThreadScheduler);
```

### 3. 属性更新方法
```csharp
// 确保在 UI 线程调用 RaisePropertyChanged
RxApp.MainThreadScheduler.Schedule(() =>
{
    this.RaisePropertyChanged(nameof(MyProperty));
});
```

## 跨平台兼容性 / Cross-Platform Compatibility

这些方法在所有平台都适用：
- ✅ WPF
- ✅ Avalonia
- ✅ MAUI

`RxApp.MainThreadScheduler` 会自动适配当前平台。

## 测试验证 / Test Verification

1. 启动应用
2. 点击 "切换到中文" 按钮
3. 点击 "Switch to English" 按钮
4. 观察文本是否正确切换，无报错
