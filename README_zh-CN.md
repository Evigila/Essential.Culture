# Essential.Culture

[英文](README.md) | [简体中文](README_zh-CN.md)

`Essential.Culture` 是一个面向 .NET 的 JSON 文化管理组件。

它使用单一 `Culture.json` 管理文化键，通过 Source Generator 生成强类型键（源神启动！），并为 WPF、Avalonia 和 WinUI 3 提供运行时文化切换。

## 功能

- `Culture.json` 是约定俗成的 JSON 名称，将自动被识别。
- 源生成 `CultureKey` 和统一的 `Localize` XAML API，提供强类型键与编译期检查。
- 通过 `KeyBinding` 让集合项或运行时状态动态选择翻译键。
- 通过 `Localizer.Parse(...)` 和 `Localizer.TryParse(...)` 解析文化键，支持参数化。
- 通过 `Localizer.Current` 动态操作文化。

## 快速开始

通过 NuGet 一键安装：

如果是 WPF 项目：

```powershell
dotnet add package Arkheide.Essential.Culture.Wpf
```

如果是 Avalonia 项目：

```powershell
dotnet add package Arkheide.Essential.Culture.Avalonia
```

如果是 WinUI 3 项目：

```powershell
dotnet add package Arkheide.Essential.Culture.WinUI
```

其他：

```powershell
dotnet add package Arkheide.Essential.Culture
```

项目会自动创建 `Culture.json` 并包含默认内容，你可以创建自己的键和译文：

```json
{
  "Greeting": {
    "en-US": "Hello, World!",
    "zh-CN": "你好，世界！"
  },
  "Welcome_User": {
    "en-US": "Hello, {0}!",
    "zh-CN": "你好，{0}！"
  }
}
```

## 立即使用

> [!NOTE]
> 如果 IntelliSense 尚未显示生成类型，请先构建一次项目。

WPF 使用 `Localize`，参数可以直接使用 Binding：

```xml
<Window xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
  <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
</Window>
```

Avalonia 使用相同 API，仅命名空间语法不同：

```xml
<Window xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
  <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
</Window>
```

使用强类型 `Key=` 属性，让编辑器能够根据生成的 `CultureKey` 枚举提供键候选。键来自 DataContext 或运行时状态时使用 `KeyBinding=`。

WinUI 3 的动态参数通过同一 `Localize` 类型的附加属性提供：

```xml
<Window xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User}"
             culture:Localize.Argument0="{x:Bind ViewModel.UserName, Mode=OneWay}" />
  <TextBlock Text="{culture:Localize}"
             culture:Localize.KeyBinding="{x:Bind ViewModel.CurrentTextKey, Mode=OneWay}" />
</Window>
```

`Key` 和 `KeyBinding` 不能同时使用。静态键、动态键、格式参数或当前文化发生变化时，`Localize` 都会重新解析目标文本；应用无需自行监听文化事件。

## 源生成

Generator 会自动获取 `Culture.json`，并在构建和发布时复制到输出目录。默认文件会生成 `CultureKey` 枚举、`Key` token，以及 UI 项目使用的 `Localize` 静态入口：

```csharp
namespace ArkheideSystem.Essential.Culture;

public enum CultureKey
{
    Greeting,
}

public static class Key
{
    public static string Greeting => "Key.Greeting";
}
```

```csharp
using ArkheideSystem.Essential.Culture;
using GeneratedKey = global::ArkheideSystem.Essential.Culture.Key;

// 输出译文
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));

// 切换文化
Localizer.Current.SetCulture("zh-CN");

// 再次输出译文，无需管理文化切换
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));
```

> [!NOTE]
> 默认文化与 fallback 均为 `en-US`。

如需覆盖生成类型所在命名空间，可在项目中设置：

```xml
<PropertyGroup>
  <EssentialCultureNamespace>MyApplication.Localization</EssentialCultureNamespace>
</PropertyGroup>
```

自动生成 `Culture.json` 时已有的文件不会被覆盖。创建后可以直接编辑该文件，添加自己的文化键和译文。如需禁止自动创建，可在项目文件中设置：

```xml
<PropertyGroup>
  <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
</PropertyGroup>
```

## 包

| 包 | 用途 |
| --- | --- |
| `Arkheide.Essential.Culture` | 解析文化状态与 `Localizer` 静态入口 |
| `Arkheide.Essential.Culture.Generator` | 从 `Culture.json` 生成强类型键；通常自传递，无需单独安装 |
| `Arkheide.Essential.Culture.Wpf` | WPF 强类型 `Localize` XAML Binding |
| `Arkheide.Essential.Culture.Avalonia` | Avalonia 强类型 `Localize` XAML Binding |
| `Arkheide.Essential.Culture.WinUI` | WinUI 3 强类型 `Localize` 与窗口刷新基础设施 |

```powershell
dotnet add package Arkheide.Essential.Culture
dotnet add package Arkheide.Essential.Culture.Wpf
dotnet add package Arkheide.Essential.Culture.Avalonia
dotnet add package Arkheide.Essential.Culture.WinUI
```

## AI 辅助

> [!IMPORTANT]
> 本库使用了 AI Agent (ChatGPT Codex) 技术来辅助编写。
> 欢迎使用任何形式的 AI 辅助进行维护和创作，**但在提交前人工审核是必要的**。

## 文档与示例

- [完整使用指南](docs/usage-guide.md)
- [Demo 总览](demo/README.md)
- [Console Demo](demo/Essential.Culture.Demo.Console/README.md)
- [WPF Demo](demo/Essential.Culture.Demo.Wpf/README.md)
- [Avalonia Demo](demo/Essential.Culture.Demo.Avalonia/README.md)
- [WinUI 3 Demo](demo/Essential.Culture.Demo.WinUI3/README.md)

## 许可证

使用 [MIT License](LICENSE.txt)。
