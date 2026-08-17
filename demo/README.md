# Arkheide.Essential.Culture Demo

本目录包含四个彼此独立的示例项目，共用一份 [`Culture.json`](Culture.json) 和少量框架无关辅助代码。

| 示例 | 展示内容 |
| --- | --- |
| [Console](Arkheide.Essential.Culture.Demo.Console/README.md) | `Localizer.Parse`、带参解析与交互式文化切换 |
| [WPF](Arkheide.Essential.Culture.Demo.Wpf/README.md) | `Localize` 参数 Binding 和无需扫描的文化刷新 |
| [Avalonia](Arkheide.Essential.Culture.Demo.Avalonia/README.md) | `Localize` 参数 Binding、强类型 XAML 键和即时文化切换 |
| [WinUI 3](Arkheide.Essential.Culture.Demo.WinUI3/README.md) | `Localize` 参数附加属性、窗口 Attach 和即时文化切换 |

## 构建

在仓库根目录执行：

```powershell
dotnet build demo\Arkheide.Essential.Culture.Demo.slnx
```

分别运行：

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Console\Arkheide.Essential.Culture.Demo.Console.csproj
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Wpf\Arkheide.Essential.Culture.Demo.Wpf.csproj
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Avalonia\Arkheide.Essential.Culture.Demo.Avalonia.csproj
dotnet run --project demo\Arkheide.Essential.Culture.Demo.WinUI3\Arkheide.Essential.Culture.Demo.WinUI3.csproj
```

WinUI 3 示例为 x64 Unpackaged 应用，也可以在 Visual Studio 中选择其 `Unpackaged` 启动配置。

## 仓库内引用方式

Demo 使用源码 `ProjectReference` 同时验证当前仓库代码。源码引用不会执行已打包 NuGet 的全部传递构建资产，因此每个示例显式：

- 引用 Generator 项目作为 Analyzer。
- 把共享的 `Culture.json` 链接为 `AdditionalFiles`。
- 把同一文件复制到应用输出目录。

真实应用安装 Core、WPF、Avalonia 或 WinUI 包后，只需把 `Culture.json` 放在应用项目根目录；Generator 和复制约定会自动传递。

三个 UI 示例均只使用 Generator 产生的单命名空间 `Localize` API。WPF/Avalonia 由 `MultiBinding` 同时跟踪格式参数与文化变化；WinUI 3 由 MarkupExtension 提供内部标记，再由参数附加属性和窗口 Host 完成解析。共享 `Greeting` 键包含 `{0}`，三个示例都传入 `ProductName`，可直接验证参数与文化切换组合行为。

WPF/Avalonia 不需要应用级本地化对象。WinUI 3 因框架缺少 `MultiBinding`，仍需在窗口激活前登记刷新协调器。核心 `Localizer` 从输出目录懒加载资源文件，本身不需要初始化或清理。
