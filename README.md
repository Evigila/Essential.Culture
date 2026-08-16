# Arkheide.Essential.Culture

`Arkheide.Essential.Culture` 是一个面向 .NET 的 JSON 文本本地化组件。它使用一份 `Culture.json` 管理全部文化，通过 Source Generator 生成强类型键，并为 WPF、Avalonia 和 WinUI 3 提供运行时界面刷新。

## 功能

- 使用单个 `Culture.json` 管理资源键、文化和格式化文本。
- 生成 `Arkheide.Essential.Culture.Key` 静态属性，为 C# 和 XAML 提供强类型键与编译期检查。
- 通过 `Localizer.Parse(...)` 解析当前文化文本，通过 `Localizer.Current` 切换文化和监听变化。
- 支持父文化查找、`en-US` fallback 和复合格式化。
- WPF、Avalonia 和 WinUI 3 Applicator 会保存原始 token，并在文化变化后重新应用译文。
- 默认从应用输出目录懒加载 `Culture.json`；核心无需显式初始化或清理。

## 包

公开包只有以下五个：

| 包 | 用途 | 目标框架 |
| --- | --- | --- |
| `Arkheide.Essential.Culture` | 核心解析、文化状态与 `Localizer` 静态门面 | `net10.0` |
| `Arkheide.Essential.Culture.Generator` | 从 `Culture.json` 生成强类型 `Arkheide.Essential.Culture.Key`；通常无需单独安装 | `netstandard2.0` Analyzer |
| `Arkheide.Essential.Culture.Wpf` | WPF 视觉树本地化 Applicator | `net10.0-windows` |
| `Arkheide.Essential.Culture.Avalonia` | Avalonia 视觉树本地化 Applicator | `net10.0` |
| `Arkheide.Essential.Culture.WinUI` | WinUI 3 窗口与视觉树本地化 Applicator | `net10.0-windows10.0.19041.0` |

框架包都会传递 Core，Core 再传递 Generator。Generator 只参与编译，不会成为应用的运行时程序集依赖。

```powershell
dotnet add package Arkheide.Essential.Culture
dotnet add package Arkheide.Essential.Culture.Wpf
dotnet add package Arkheide.Essential.Culture.Avalonia
dotnet add package Arkheide.Essential.Culture.WinUI
```

普通 .NET 应用只安装 Core；UI 应用直接安装对应框架包。

## 快速开始

在应用项目根目录创建 `Culture.json`：

```json
{
  "Greeting": {
    "en-US": "Hello, {0}!",
    "zh-CN": "你好，{0}！"
  }
}
```

NuGet 包会自动把该文件交给 Generator，并在构建和发布时复制到输出目录。Generator 默认产生：

```csharp
namespace Arkheide.Essential.Culture;

public static class Key
{
    public static string Greeting => "Key.Greeting";
}
```

生成属性返回的是稳定 token，不是译文。应用通过静态门面解析和切换文化：

```csharp
using Arkheide.Essential.Culture;
using GeneratedKey = global::Arkheide.Essential.Culture.Key;

Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting, "Arkheide.Essential.Culture"));

Localizer.Current.SetCulture("zh-CN");
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting, "Arkheide.Essential.Culture"));
```

`Localizer.Parse(token)` 解析无参数文本；`Localizer.Parse(token, args...)` 在当前文化下执行复合格式化。`Localizer.Current` 提供 `Culture`、`AvailableCultures`、`SetCulture(...)` 和 `Changed`。第一次访问时会从 `AppContext.BaseDirectory` 懒加载 `Culture.json`，默认文化与 fallback 均为 `en-US`。

如需覆盖生成类型所在命名空间，可在项目中设置：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureNamespace>MyApplication.Localization</ArkheideEssentialCultureNamespace>
</PropertyGroup>
```

这只改变生成类型的 CLR 地址，属性值仍是稳定的 `Key.*` token。

## XAML 强类型键

WPF 使用 `x:Static`：

```xml
<Window xmlns:culture="clr-namespace:Arkheide.Essential.Culture"
        Title="{x:Static culture:Key.App_Title}">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
</Window>
```

Avalonia 同样使用 `x:Static`，但命名空间映射使用 `using:` 语法：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
</Window>
```

WinUI 3 使用静态属性的 `x:Bind`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Bind culture:Key.Greeting, Mode=OneTime}" />
</Window>
```

这些表达式在初始化时把 token 写入属性；对应 Applicator 随后写入译文，并在 `Localizer.Current.Changed` 后刷新。WinUI 的 `Mode=OneTime` 可避免绑定再次覆盖 Applicator 写入的值。

强类型引用会在编译期检查成员。编辑器通常也能提供成员补全；首次生成或修改键后，如果 IntelliSense 尚未更新，请先构建一次项目，再重新打开补全列表。

## UI Applicator 生命周期

三个框架包都提供无参 Applicator，由应用显式管理：

- WPF：`new WpfLocalizationApplicator()`，调用 `Start(Dispatcher)`；窗口显示前调用 `Apply(window)`；退出时调用 `Stop()` 或 `Dispose()`。
- Avalonia：`new AvaloniaLocalizationApplicator()`，调用 `Start(Application)`；根视图呈现前调用 `Apply(root)`；退出时调用 `Stop()` 或 `Dispose()`。
- WinUI 3：`new WinUILocalizationApplicator()`；窗口激活前调用 `Attach(window)`，独立对话框显示前调用 `Apply(dialog)`；关闭时调用 `Detach(window)` 或最终 `Dispose()`。

这里需要清理的是 Applicator 的框架事件订阅；`Localizer` 核心自身不需要显式释放。

## 文档与示例

- [完整使用指南](docs/usage-guide.md)
- [Demo 总览](demo/README.md)
- [Console Demo](demo/Arkheide.Essential.Culture.Demo.Console/README.md)
- [WPF Demo](demo/Arkheide.Essential.Culture.Demo.Wpf/README.md)
- [Avalonia Demo](demo/Arkheide.Essential.Culture.Demo.Avalonia/README.md)
- [WinUI 3 Demo](demo/Arkheide.Essential.Culture.Demo.WinUI3/README.md)
- [测试项目说明](tests/README.md)

## 许可证

Arkheide.Essential.Culture 使用 [MIT License](LICENSE.txt)。
