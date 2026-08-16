# Arkheide.Essential.Culture WPF Demo

这个示例展示 WPF 应用如何使用 `Localizer` 静态门面和无参 `WpfLocalizationApplicator`，在运行期间切换文化并刷新视觉树。

## 依赖

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Wpf" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 为验证源码，使用项目引用并显式链接共享 [`../Culture.json`](../Culture.json)。

## Applicator 生命周期

[`App.xaml.cs`](App.xaml.cs) 在 WPF 启动时创建一个应用级 Applicator：

```csharp
private WpfLocalizationApplicator? applicator;

protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    Localizer.Current.SetCulture("en-US");
    applicator = new WpfLocalizationApplicator();
    applicator.Start(Dispatcher);

    var window = new MainWindow();
    applicator.Apply(window);
    MainWindow = window;
    window.Show();
}
```

`Start(Dispatcher)` 订阅 Loaded 和文化变化；`Apply(window)` 在 `Show()` 前立即扫描窗口，避免首帧显示 `Key.*`。应用退出时释放 Applicator：

```csharp
protected override void OnExit(ExitEventArgs e)
{
    applicator?.Dispose();
    base.OnExit(e);
}
```

也可以显式调用 `Stop()`。这里清理的是 WPF 事件订阅；`Localizer` 核心本身无需释放。

## XAML 强类型键

Generator 默认产生 `Arkheide.Essential.Culture.Key`。[`MainWindow.xaml`](MainWindow.xaml) 映射 `Arkheide.Essential.Culture` CLR 命名空间并使用 `x:Static`：

```xml
<Window xmlns:culture="clr-namespace:Arkheide.Essential.Culture"
        Title="{x:Static culture:Key.App_Title}">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
  <Button Content="{x:Static culture:Key.Action_SwitchLanguage}" />
</Window>
```

生成属性返回 `Key.*` token。`x:Static` 提供强类型成员解析；Applicator 保存 token、解析当前译文，并在语言变化后重新应用。首次生成或修改键后，如果 XAML 补全没有更新，请先构建一次项目。

## 动态文本

不直接保留 token 的格式化文本由代码刷新：

```csharp
CultureText.Text = Localizer.Parse(
    Arkheide.Essential.Culture.Key.Current_Culture,
    Localizer.Current.Culture
);
```

窗口订阅 `Localizer.Current.Changed` 刷新当前文化指示器，并在关闭时退订。语言按钮调用 `Localizer.Current.SetCulture(...)`；问候按钮使用 `Localizer.Parse(...)` 解析 MessageBox 标题和正文。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Wpf\Arkheide.Essential.Culture.Demo.Wpf.csproj
```

请验证：

- 首帧显示译文而不是 `Key.*`。
- 标题、描述、按钮和窗口标题随语言切换。
- 当前文化文本正确格式化。
- MessageBox 标题和内容使用当前语言。
- 重复打开 MessageBox 后仍可继续动态切换语言。
