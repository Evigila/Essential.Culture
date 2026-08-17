# Arkheide.Essential.Culture

`Arkheide.Essential.Culture` 是一个面向 .NET 的 JSON 文化管理组件。

它使用单一 `Culture.json` 管理文化键，通过 Source Generator 生成强类型键（源神启动！），并为 WPF、Avalonia 和 WinUI 3 提供运行时文化切换。

## 功能

- `Culture.json` 是约定俗成的 JSON 名称，将自动被识别。
- 源生成 `Arkheide.Essential.Culture.Key` 静态属性，为 C# 和 XAML 提供强类型键与编译期检查。
- 通过 `Localizer.Parse(...)` 解析文化键，支持参数化
- 通过 `Localizer.Current` 动态操作文化

## 快速开始

通过 Nuget 一键安装：

如果是WPF项目：
```
```

如果是AVALONIA项目：
```
```

如果是WINUI3项目：
```
```

其他：
```
```

项目会自动创建 `Culture.json` 并包含默认内容，你可以创建自己的键和译文：

```json
{
  "Greeting": {
    "en-US": "Hello, World!",
    "zh-CN": "你好，世界！"
  },
  "Custom_Key": {
    "en-US": "This is a custom message",
    "zh-CN": "这是一句自定义译文"
  }
}
```

## 立即使用

> [!NOTE]
如果Intellisence没有反应，你应该尝试编译一次项目

WPF 使用 `x:Static`：

```xml
<Window xmlns:ct="clr-namespace:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Static ct:Key.Greeting}" />
</Window>
```

Avalonia 同样使用 `x:Static`，但命名空间使用 `using:` 语法：

```xml
<Window xmlns:ct="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Static ct:Key.Greeting}" />
</Window>
```

WinUI 3 使用 `x:Bind`：

```xml
<Window xmlns:ct="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Bind ct:Key.Greeting, Mode=OneTime}" />
</Window>
```

## 源生成

Generator 会自动获取 `Culture.json`，并在构建和发布时复制到输出目录。默认文件会生成以下类型：

```csharp
namespace Arkheide.Essential.Culture;

public static class Key
{
    public static string Greeting => "Key.Greeting";
}
```

```csharp
using Arkheide.Essential.Culture;
using GeneratedKey = global::Arkheide.Essential.Culture.Key;

// 输出译文
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));

// 切换文化
Localizer.Current.SetCulture("zh-CN");

// 再次输出译文，无需管理文化切换
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));
```
> [!NOTE]
默认文化与 fallback 均为 `en-US`。

如需覆盖生成类型所在命名空间，可在项目中设置：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureNamespace>MyApplication.Localization</ArkheideEssentialCultureNamespace>
</PropertyGroup>
```

自动生成 `Culture.json` 时已有的文件不会被覆盖。创建后可以直接编辑该文件，添加自己的文化键和译文。如需禁止自动创建，可在项目文件中设置：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureAutoCreate>false</ArkheideEssentialCultureAutoCreate>
</PropertyGroup>
```

## 包

| 包 | 用途 |
| --- | --- |
| `Arkheide.Essential.Culture` | 解析文化状态与 `Localizer` 静态入口 |
| `Arkheide.Essential.Culture.Generator` | 从 `Culture.json` 生成强类型键；通常自传递，无需单独安装 |
| `Arkheide.Essential.Culture.Wpf` | WPF 视觉树本地化 Applicator |
| `Arkheide.Essential.Culture.Avalonia` | Avalonia 视觉树本地化 Applicator |
| `Arkheide.Essential.Culture.WinUI` | WinUI 3 窗口与视觉树本地化 Applicator |

```powershell
dotnet add package Arkheide.Essential.Culture
dotnet add package Arkheide.Essential.Culture.Wpf
dotnet add package Arkheide.Essential.Culture.Avalonia
dotnet add package Arkheide.Essential.Culture.WinUI
```

## AI 辅助

> [!IMPORTANT]
本库使用了 AI Agent (ChatGPT Codex) 技术来辅助编写。  
欢迎使用任何形式的 AI 辅助进行维护和创作，**但在提交前人工审核是必要的**。

## 文档与示例

- [完整使用指南](docs/usage-guide.md)
- [Demo 总览](demo/README.md)
- [Console Demo](demo/Arkheide.Essential.Culture.Demo.Console/README.md)
- [WPF Demo](demo/Arkheide.Essential.Culture.Demo.Wpf/README.md)
- [Avalonia Demo](demo/Arkheide.Essential.Culture.Demo.Avalonia/README.md)
- [WinUI 3 Demo](demo/Arkheide.Essential.Culture.Demo.WinUI3/README.md)
- [测试项目说明](tests/README.md)

## 许可证

使用 [MIT License](LICENSE.txt)。
