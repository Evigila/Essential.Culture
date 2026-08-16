# Arkheide.Essential.Culture Avalonia Demo

这个示例展示 Avalonia 应用如何管理 `AvaloniaLocalizationApplicator`、使用强类型 XAML token，并即时切换文化。

## 依赖

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Avalonia" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 为验证源码，使用项目引用并显式链接共享 [`../Culture.json`](../Culture.json)。

## Applicator 生命周期

[`App.axaml.cs`](App.axaml.cs) 在桌面生命周期初始化时创建 Applicator 和窗口：

```csharp
private AvaloniaLocalizationApplicator? applicator;

public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        applicator = new AvaloniaLocalizationApplicator();
        applicator.Start(this);

        var window = new MainWindow(applicator);
        applicator.Apply(window);
        desktop.MainWindow = window;
        desktop.Exit += Desktop_Exit;
    }

    base.OnFrameworkInitializationCompleted();
}
```

`Start(this)` 监听后续视觉树加载和文化变化；`Apply(window)` 在首次呈现前立即处理窗口。应用退出时释放 Applicator：

```csharp
private void Desktop_Exit(
    object? sender,
    ControlledApplicationLifetimeExitEventArgs e)
{
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

## 当前文化文本

当前文化文本需要一个格式化参数，因此窗口仅保留一个简短刷新函数：

```csharp
private void UpdateCultureText() =>
    CurrentCultureText.Text = Localizer.Parse(
        DemoKey.Current_Culture,
        Localizer.Current.Culture
    );
```

窗口在文化变化时调用该函数，并在关闭时退订事件。示例不引入 ViewModel、Command 或额外 MVVM 基础设施；语言按钮直接调用 `Localizer.Current.SetCulture(...)`。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Avalonia\Arkheide.Essential.Culture.Demo.Avalonia.csproj
```

请验证：

- 主窗口标题、正文和按钮在首次显示时已经本地化。
- 切换语言后 XAML 文本和当前文化文本同时刷新。
- 问候对话框的标题、正文和关闭按钮使用当前语言。
- 关闭应用后没有遗留的文化变化订阅。
