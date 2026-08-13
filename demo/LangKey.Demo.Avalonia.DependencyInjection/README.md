# LangKey Avalonia + DI Demo

这个示例展示 Avalonia 专用适配器如何通过 Generic Host、DI、XAML token 和视觉树刷新实现本地化。

## 环境与依赖

- .NET 10 SDK
- Avalonia 12.1.0
- `ArkheideSystem.LangKey.Avalonia`（自动传递 Avalonia Runtime、DI、Core 和 Generator）
- `Microsoft.Extensions.Hosting` 10.0.8

Avalonia Desktop 可跨平台运行；示例显式使用 Segoe UI，非 Windows 系统可能选择字体 fallback。

真实应用只需安装一个 LangKey 包：

```xml
<PackageReference Include="ArkheideSystem.LangKey.Avalonia" Version="1.0.0" />
```

Demo 中额外的 Generator `ProjectReference` 只用于直接验证仓库源码，并不代表 NuGet 使用步骤。

## DI 组合

[`App.axaml.cs`](App.axaml.cs) 在桌面生命周期启动时注册：

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(this);
builder.Services.AddSingleton<DemoCultureSource>();
builder.Services.AddLangKeyAvalonia<App, DemoCultureSource>("LangKey.json");
builder.Services.AddSingleton<AvaloniaDemoViewModel>();
builder.Services.AddTransient<MainWindow>();
```

构建并启动 Host 后，在首次呈现前主动应用一次窗口：

```csharp
host = builder.Build();
host.Start();

var window = host.Services.GetRequiredService<MainWindow>();
host.Services.GetRequiredService<ILangKeyAvaloniaApplicator>().Apply(window);
desktop.MainWindow = window;
```

HostedService 负责启动 Loaded 监听和文化变化刷新；应用退出时停止并释放 Host。

## Avalonia XAML token

[`MainWindow.axaml`](MainWindow.axaml) 直接在 Avalonia 显示属性中使用稳定 token：

```xml
<Window Title="LangKey.App_Title">
  <TextBlock Text="LangKey.Greeting" />
  <Button Content="LangKey.Action_SwitchLanguage" />
</Window>
```

Applicator 会保存原 token，并处理 Window Title、TextBlock Text、Content、Header、Placeholder、ToolTip 和 Automation Name 等常见属性。文化变化后，它在 Avalonia UI 线程重新扫描活动窗口；动态加载的控件也会通过 Loaded 处理。

需要参数的当前文化文本仍由 [`AvaloniaDemoViewModel`](AvaloniaDemoViewModel.cs) 使用 Resolver 格式化：

```csharp
public string CurrentCulture =>
    resolver.Format(LangKey.Current_Culture, resolver.Current);
```

ViewModel 只为这个动态格式化属性响应 `Changed`；无需为全部静态 XAML 文本重复编写绑定属性：

```xml
<TextBlock Text="{Binding CurrentCulture}" />
<Button Content="LangKey.Action_SwitchLanguage"
        Command="{Binding SwitchCultureCommand}" />
```

相关文件：

- [`MainWindow.axaml`](MainWindow.axaml)
- [`MainWindow.axaml.cs`](MainWindow.axaml.cs)
- [`DemoCultureSource`](../Shared/DemoCultureSource.cs)
- [`AvaloniaDemoViewModel`](AvaloniaDemoViewModel.cs)

## 对话框与样式

问候按钮显示 Avalonia 模态窗口 [`GreetingDialog.axaml`](GreetingDialog.axaml)。显示前调用 `applicator.Apply(dialog)`，保证首帧已经完成翻译。弹窗：

- 相对主窗口居中。
- 标题、正文、关闭按钮都使用 `LangKey.*` token，并由同一个 Applicator 解析。
- 使用直接根布局，不额外嵌套 Border。

[`App.axaml`](App.axaml) 在 FluentTheme 后添加全局 Button 内容居中样式。

## 运行

```powershell
dotnet run --project demo\LangKey.Demo.Avalonia.DependencyInjection\LangKey.Demo.Avalonia.DependencyInjection.csproj
```

主窗口为 1280×720 并居中。验证语言切换、所有绑定刷新、按钮文字对齐和模态弹窗。共享资源位于 [`../LangKey.json`](../LangKey.json)。
