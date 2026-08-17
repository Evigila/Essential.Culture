# Arkheide.Essential.Culture WinUI 3 Demo

这个示例展示 x64 Unpackaged WinUI 3 应用如何用 `WinUILocalizationHost` 管理窗口生命周期、用强类型参数化 `Localize` 编写 XAML，并用 `Localizer.Parse` 创建代码端译文。

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

## Host 生命周期

[`App.xaml.cs`](App.xaml.cs) 创建一个应用级 Host：

```csharp
private readonly WinUILocalizationHost localizationHost;
private Window? window;

public App()
{
    InitializeComponent();
    localizationHost = new WinUILocalizationHost();
}

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    window = new MainWindow();
    localizationHost.Attach(window);
    window.Activate();
}
```

`Attach(window)` 必须在窗口所属 UI 线程、`Activate()` 之前调用。首次有效激活会在 XAML 创建视觉树、`Localize` 写入内部标记后再发现一次属性，随后立即解除激活监听。文化变化只刷新已经登记的属性，不再重新扫描整棵视觉树；窗口关闭后会自动停止跟踪。应用最终关闭时调用 `Dispose()`。

## XAML 强类型本地化

WinUI XAML 使用 Generator 产生的 `Localize`。由于 WinUI XAML 编译器要求自定义 MarkupExtension 使用公开默认构造器，键必须写成 `Key=...`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture"
        Title="{culture:Localize Key=App_Title}">
  <TextBlock Text="{culture:Localize Key=Greeting}"
             culture:Localize.Argument0="{x:Bind ProductName, Mode=OneWay}" />
  <Button Content="{culture:Localize Key=Action_SwitchLanguage}" />
</Window>
```

`Localize` 向目标属性交付库内部标记，`Argument0`–`Argument2` 附加属性保存最新格式参数。Host 在首次发现、参数变化和文化变化时重新解析该标记。内部标记不是公共 C# API；代码端继续使用 `Localizer.Parse(Key.*)`。

XAML 的唯一标准入口是 `Localize`，Host 不会接管普通字符串或原始 `Key.*`。不要对使用 `Localize` 的同一目标属性再叠加其他 Binding，否则后续写入会覆盖标记或译文。首次生成或修改 JSON 键后，如果 XAML 补全尚未更新，请先构建一次项目。

## 文化变化与对话框

窗口通过静态门面切换文化，并更新绑定到 `Localize.Argument0` 的原始文化名称：

```csharp
Localizer.Current.SetCulture(
    Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
);

CurrentCulture = Localizer.Current.Culture;
PropertyChanged?.Invoke(
    this,
    new PropertyChangedEventArgs(nameof(CurrentCulture))
);
```

代码创建的 `ContentDialog` 使用 C# 唯一入口 `Localizer.Parse`，不把 `Key.*` token 交给 Host：

```csharp
var dialog = new ContentDialog
{
    Title = Localizer.Parse(DemoKey.App_Title),
    Content = Localizer.Parse(DemoKey.Greeting, ProductName),
    CloseButtonText = Localizer.Parse(DemoKey.Action_Close),
    XamlRoot = Root.XamlRoot,
};

await dialog.ShowAsync();
```

`Apply(root)` 仍保留给由独立 XAML 或模板创建、且内部含 `Localize` 标记的根对象。它只承担发现与生命周期登记，不是另一套翻译入口，也不会处理代码直接赋值的 `Key.*`。这类独立根采用弱登记，不会被窗口长期持有；窗口关闭时会退订自己的 `Localizer.Current.Changed` 处理器。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.WinUI3\Arkheide.Essential.Culture.Demo.WinUI3.csproj
```

请验证：

- 窗口首次显示时，标题栏、标题文本、描述和按钮已经本地化。
- 问候文本始终显示 `Arkheide`，不会残留 `{0}`。
- 切换语言后所有 XAML 文本和参数化文化指示器同步刷新。
- 连续打开 `ContentDialog` 多次后仍能正常切换语言。
- 对话框标题、正文和关闭按钮始终采用当前文化。
- 反复 Alt+Tab、最小化和恢复窗口后文案不回退为 token，也不会产生可感知卡顿。
- 连续打开并关闭对话框至少 20 次，主窗口仍可切换文化且内存不会随每次弹窗持续线性增长。
