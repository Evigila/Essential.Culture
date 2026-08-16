# Arkheide.Essential.Culture Demo

本目录包含四个彼此独立的示例项目，共用一份 [`Culture.json`](Culture.json) 和少量框架无关辅助代码。

| 示例 | 展示内容 |
| --- | --- |
| [Console](Arkheide.Essential.Culture.Demo.Console/README.md) | `Localizer.Parse`、带参解析与交互式文化切换 |
| [WPF](Arkheide.Essential.Culture.Demo.Wpf/README.md) | `WpfLocalizationApplicator` 的 `Start`、`Apply` 和释放 |
| [Avalonia](Arkheide.Essential.Culture.Demo.Avalonia/README.md) | `AvaloniaLocalizationApplicator`、强类型 XAML 键和 ViewModel 刷新 |
| [WinUI 3](Arkheide.Essential.Culture.Demo.WinUI3/README.md) | `WinUILocalizationApplicator`、窗口 Attach 和 `x:Bind` 强类型键 |

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

三个 UI 示例均使用 Generator 产生的 CLR 属性：WPF/Avalonia 使用 `x:Static`，WinUI 3 使用 `x:Bind Mode=OneTime`。这些属性提供的是稳定 `Key.*` token；Applicator 保存 token、写入当前译文，并在 `Localizer.Current.Changed` 后重新应用。

每个 UI 示例都由应用显式创建并释放一个 Applicator。核心 `Localizer` 从输出目录懒加载资源文件，本身不需要初始化或清理。
