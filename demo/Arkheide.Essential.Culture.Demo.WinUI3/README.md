# Arkheide.Essential.Culture WinUI 3 Demo

这个示例展示 x64 Unpackaged WinUI 3 应用如何手动管理 `WinUILocalizationApplicator`，使用 `x:Bind` 强类型 token，并跟踪窗口与独立 `ContentDialog`。

## 依赖与启动方式

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.WinUI" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 还直接引用 `Microsoft.WindowsAppSDK`，并通过项目引用验证当前源码。

项目关键设置：

```xml
<UseWinUI>true</UseWinUI>
<WindowsPackageType>None</WindowsPackageType>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
<PlatformTarget>x64</PlatformTarget>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

Visual Studio 启动时选择 [`Properties/launchSettings.json`](Properties/launchSettings.json) 中的 `Arkheide.Essential.Culture.Demo.WinUI3 (Unpackaged)` 配置。

## Applicator 生命周期

[`App.xaml.cs`](App.xaml.cs) 创建一个应用级 Applicator：

```csharp
private readonly WinUILocalizationApplicator applicator;
private Window? window;

public App()
{
    InitializeComponent();
    Localizer.Current.SetCulture("en-US");
    applicator = new WinUILocalizationApplicator();
}

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    window = new MainWindow(applicator);
    applicator.Attach(window);
    window.Activate();
}
```

`Attach(window)` 必须在窗口所属 UI 线程、`Activate()` 之前调用。它会立即处理窗口标题和视觉树，在文化变化后通过该窗口的 `DispatcherQueue` 刷新，并在窗口关闭后停止跟踪。应用最终关闭时调用 `Dispose()`。

## XAML 强类型键

WinUI XAML 使用静态属性的 `x:Bind`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture"
        Title="{x:Bind culture:Key.App_Title, Mode=OneTime}">
  <TextBlock Text="{x:Bind culture:Key.Greeting, Mode=OneTime}" />
  <Button Content="{x:Bind culture:Key.Action_SwitchLanguage, Mode=OneTime}" />
</Window>
```

显式 `Mode=OneTime` 很重要：绑定只在初始化时交付 `Key.*` token，不会在 Applicator 写入译文后再次覆盖显示值。首次生成或修改 JSON 键后，如果 XAML 补全尚未更新，请先构建一次项目。

## 文化变化与对话框

窗口通过静态门面切换文化并格式化当前文化指示器：

```csharp
Localizer.Current.SetCulture(
    Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
);

CurrentCultureText.Text = Localizer.Parse(
    DemoKey.Current_Culture,
    Localizer.Current.Culture
);
```

`ContentDialog` 不属于窗口初始扫描到的视觉根，因此在显示前调用：

```csharp
var dialog = new ContentDialog
{
    Title = DemoKey.App_Title,
    Content = DemoKey.Greeting,
    CloseButtonText = DemoKey.Action_Close,
    XamlRoot = Root.XamlRoot,
};

applicator.Apply(dialog);
await dialog.ShowAsync();
```

这会让标题、正文和关闭按钮按当前文化解析。窗口关闭时会退订自己的 `Localizer.Current.Changed` 处理器。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.WinUI3\Arkheide.Essential.Culture.Demo.WinUI3.csproj
```

请验证：

- 窗口标题、标题文本、描述和按钮在首次激活前已本地化。
- 切换语言后所有静态 XAML 文本和文化指示器同步刷新。
- 连续打开 `ContentDialog` 多次后仍能正常切换语言。
- 对话框标题、正文和关闭按钮始终采用当前文化。
