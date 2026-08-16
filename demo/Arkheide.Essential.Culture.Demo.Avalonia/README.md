# Arkheide.Essential.Culture Avalonia Demo

这个示例展示 Avalonia 应用如何手动管理 `AvaloniaLocalizationApplicator`，使用强类型 XAML token，并在文化变化后同时刷新视觉树和 ViewModel 属性。

## 依赖

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Avalonia" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 为验证源码，使用项目引用并显式链接共享 [`../Culture.json`](../Culture.json)。

## Applicator 生命周期

[`App.axaml.cs`](App.axaml.cs) 在桌面生命周期初始化时创建 Applicator、ViewModel 和窗口：

```csharp
private AvaloniaLocalizationApplicator? applicator;
private AvaloniaDemoViewModel? viewModel;

public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        Localizer.Current.SetCulture("en-US");
        applicator = new AvaloniaLocalizationApplicator();
        applicator.Start(this);

        viewModel = new AvaloniaDemoViewModel();
        var window = new MainWindow(viewModel, applicator);
        applicator.Apply(window);
        desktop.MainWindow = window;
        desktop.Exit += Desktop_Exit;
    }

    base.OnFrameworkInitializationCompleted();
}
```

`Start(this)` 监听后续视觉树加载和文化变化；`Apply(window)` 在首次呈现前立即处理窗口。应用退出时释放 ViewModel 和 Applicator：

```csharp
private void Desktop_Exit(
    object? sender,
    ControlledApplicationLifetimeExitEventArgs e)
{
    viewModel?.Dispose();
    applicator?.Dispose();
}
```

对话框是独立根，[`MainWindow.axaml.cs`](MainWindow.axaml.cs) 会在显示前调用 `applicator.Apply(dialog)`。

## XAML 强类型键

[`MainWindow.axaml`](MainWindow.axaml) 使用 `using:Arkheide.Essential.Culture` 和 `x:Static`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture"
        Title="{x:Static culture:Key.App_Title}">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
  <Button Content="{x:Static culture:Key.Action_SwitchLanguage}" />
</Window>
```

`x:Static` 在编译时解析 Generator 产生的公开静态属性。属性值仍是 `Key.*` token；Applicator 保存 token、写入当前译文，并在 `Localizer.Current.Changed` 后刷新。

## ViewModel 动态属性

当前文化文本需要格式化参数，因此 ViewModel 直接解析：

```csharp
public string CurrentCulture =>
    Localizer.Parse(DemoKeys.Current_Culture, Localizer.Current.Culture);
```

这类属性不保留在视觉属性中的原始 token，所以 ViewModel 订阅 `Localizer.Current.Changed` 并触发 `PropertyChanged`；`Dispose()` 中会退订事件。语言命令调用 `Localizer.Current.SetCulture(...)`。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Avalonia\Arkheide.Essential.Culture.Demo.Avalonia.csproj
```

请验证：

- 主窗口标题、正文和按钮在首次显示时已经本地化。
- 切换语言后 XAML 文本和 ViewModel 当前文化文本同时刷新。
- 问候对话框的标题、正文和关闭按钮使用当前语言。
- 关闭应用后没有遗留的文化变化订阅。
