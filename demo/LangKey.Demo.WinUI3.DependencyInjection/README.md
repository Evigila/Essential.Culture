# LangKey WinUI 3 + DI Demo

这个示例展示 Unpackaged WinUI 3 应用如何通过专用 LangKey 适配器、DI、强类型 XAML 键和窗口跟踪实现本地化。

## 环境与依赖

- Windows 10 1809 或更高版本
- .NET 10 SDK/Runtime
- Windows App SDK 2.3.1
- `Microsoft.Extensions.DependencyInjection` 10.0.8
- `ArkheideSystem.LangKey.WinUI`（自动传递 WinUI Runtime、DI、Core 和 Generator）
- x64

项目文件：[`LangKey.Demo.WinUI3.DependencyInjection.csproj`](LangKey.Demo.WinUI3.DependencyInjection.csproj)。

真实应用只需安装一个 LangKey 包：

```xml
<PackageReference Include="ArkheideSystem.LangKey.WinUI" Version="1.0.0" />
```

Demo 中额外的 Generator `ProjectReference` 只用于直接验证仓库源码，并不代表 NuGet 使用步骤。

## WinUI 部署配置

示例是 Unpackaged 应用：

```xml
<WindowsPackageType>None</WindowsPackageType>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
<PlatformTarget>x64</PlatformTarget>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

Windows App SDK Runtime 会复制到输出目录，因此不要求系统安装完全相同的 Windows App Runtime；代价是输出体积增大。该属性不包含 .NET Runtime，目标机仍需安装 .NET 10，除非使用完整的 .NET self-contained 发布。

Visual Studio 启动时选择 [`Properties/launchSettings.json`](Properties/launchSettings.json) 中的 `LangKey.Demo.WinUI3.DependencyInjection (Unpackaged)` 配置。

## DI 与生命周期

[`App.xaml.cs`](App.xaml.cs) 注册：

```csharp
services.AddSingleton<DemoCultureSource>();
services.AddLangKeyWinUI<DemoCultureSource>("LangKey.json");
services.AddSingleton<MainWindow>();
```

`OnLaunched` 从容器解析窗口，在 `Activate()` 前将其交给 WinUI Applicator：

```csharp
var window = services.GetRequiredService<MainWindow>();
services.GetRequiredService<ILangKeyWinUIApplicator>().Attach(window);
window.Activate();
```

`Attach` 必须在窗口所属 UI 线程调用。它会立即处理 Window Title 和视觉树，在文化变化后使用窗口自己的 DispatcherQueue 刷新，并在窗口关闭时自动解除跟踪；应用关闭时释放 Provider。

## WinUI 强类型 XAML 键

项目通过 `LangKeyNamespace` 指定 Generator 输出命名空间：

```xml
<LangKeyNamespace>ArkheideSystem.LangKey.Demo.Shared</LangKeyNamespace>
```

[`MainWindow.xaml`](MainWindow.xaml) 导入该命名空间，并用静态属性的 `x:Bind` 提供稳定 token：

```xml
<Window xmlns:keys="using:ArkheideSystem.LangKey.Demo.Shared"
        Title="{x:Bind keys:LangKey.App_Title, Mode=OneTime}">
  <TextBlock Text="{x:Bind keys:LangKey.Greeting, Mode=OneTime}" />
  <Button Content="{x:Bind keys:LangKey.Action_SwitchLanguage, Mode=OneTime}"
        Click="SwitchLanguage_Click" />
</Window>
```

输入 `keys:LangKey.` 时，WinUI XAML 编辑器可以根据生成类的公开静态成员列出键，并在编译期检查成员是否存在。新增或修改 JSON 键后，如果补全尚未刷新，请先构建一次项目。

显式 `Mode=OneTime` 很重要：绑定只在窗口初始化时交付 token，不会在 Applicator 写入译文后再次覆盖显示值。Applicator 保存原 token，并处理 Text、Content、ContentDialog 按钮文本、Placeholder、ToolTip 和 Automation Name 等依赖属性。文化源变化后，Parser 触发 Resolver 的 `Changed`，Applicator 自动重新应用附加窗口。

需要参数的当前文化文本仍由窗口使用 Resolver 显式格式化。

问候按钮创建原生 `ContentDialog`，其标题、正文和关闭按钮先写入 `LangKey.*` token，并在 `ShowAsync()` 前调用 `applicator.Apply(dialog)`。实现见 [`MainWindow.xaml.cs`](MainWindow.xaml.cs)。

## 窗口尺寸

WinUI `Window` 没有 WPF/Avalonia 的 `WindowStartupLocation`。示例使用 `AppWindow` 与 `DisplayArea.WorkArea`，在当前显示器工作区内将窗口设置为 1280×720 并居中。

## 运行

命令行：

```powershell
dotnet run --project demo\LangKey.Demo.WinUI3.DependencyInjection\LangKey.Demo.WinUI3.DependencyInjection.csproj
```

Visual Studio：

1. 将本项目设为启动项目。
2. 选择 Unpackaged 启动配置。
3. 启动调试。

共享资源位于 [`../LangKey.json`](../LangKey.json)，共享文化源位于 [`../Shared`](../Shared)。
