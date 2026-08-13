# LangKey

LangKey 是一个面向 .NET 的 JSON 本地化组件。

使用单一的JSON文件统一管理文本键，并提供运行时解析、文化切换、格式化等能力。

支持Console、WPF、Avalonia、WinUI 3。

## 功能

- 使用单个 `LangKey.json` 管理全部文化。
- 通过 Source Generator 生成强类型资源键。
- WPF/Avalonia 使用 `x:Static`、WinUI 3 使用 `x:Bind`，让 XAML 获得键名补全和编译期检查。
- 支持运行时文化切换、父文化查找、fallback 和复合格式化。
- 提供只读的 `ILangKeyResolver` 与可变的 `ILangKeyParser`。
- Core 自动携带 Source Generator，并按约定识别项目根目录的 `LangKey.json`。
- 提供 Microsoft.Extensions.DependencyInjection 注册入口。
- 为 WPF、Avalonia 和 WinUI 3 分别提供自动应用与文化切换刷新。
- 通过 `ILangKeyCultureSource` 接入自定义文化来源。

## 包

| 包 | 用途 | 目标框架 |
| --- | --- | --- |
| `ArkheideSystem.LangKey` | Parser、Resolver；自动传递 Generator | `net10.0` |
| `ArkheideSystem.LangKey.Generator` | 从 `LangKey.json` 生成强类型 `LangKey` 类；通常无需单独安装 | `netstandard2.0` Analyzer |
| `ArkheideSystem.LangKey.DependencyInjection` | `AddLangKey` 扩展方法与 DI 生命周期；自动传递 Core 和 Generator | `net10.0` |
| `ArkheideSystem.LangKey.Wpf` | WPF 自动应用、HostedService 与 DI；默认入口 | `net10.0-windows` |
| `ArkheideSystem.LangKey.Wpf.Runtime` | 不使用 DI 时的 WPF Applicator | `net10.0-windows` |
| `ArkheideSystem.LangKey.Avalonia` | Avalonia 自动应用、HostedService 与 DI；默认入口 | `net10.0` |
| `ArkheideSystem.LangKey.Avalonia.Runtime` | 不使用 DI 时的 Avalonia Applicator | `net10.0` |
| `ArkheideSystem.LangKey.WinUI` | WinUI 3 自动应用与 DI；默认入口 | `net10.0-windows10.0.19041.0` |
| `ArkheideSystem.LangKey.WinUI.Runtime` | 不使用 DI 时的 WinUI 3 Applicator | `net10.0-windows10.0.19041.0` |

通常选择：

- Console、服务或手动管理生命周期：`ArkheideSystem.LangKey`。
- 通用 DI：`ArkheideSystem.LangKey.DependencyInjection`。
- WPF：`ArkheideSystem.LangKey.Wpf`。
- Avalonia：`ArkheideSystem.LangKey.Avalonia`。
- WinUI 3：`ArkheideSystem.LangKey.WinUI`。
- UI 应用明确不使用 DI：安装对应的 `.Runtime` 包。

三个默认 UI 包都会自动获得 Core、DI 和 Generator。Generator 是编译期 Analyzer，不会成为应用的运行时程序集依赖。

```powershell
dotnet add package ArkheideSystem.LangKey.Wpf
dotnet add package ArkheideSystem.LangKey.Avalonia
dotnet add package ArkheideSystem.LangKey.WinUI
```

## 快速开始

普通 .NET 应用只需安装 Core：

```powershell
dotnet add package ArkheideSystem.LangKey
```

在项目根目录创建 `LangKey.json`：

```json
{
  "Greeting": {
    "en-US": "Hello, {0}!",
    "zh-CN": "你好，{0}！"
  }
}
```

NuGet 包会自动把项目根目录的 `LangKey.json` 交给 Generator，并在构建和发布时复制到输出目录。通常只需指定生成类的命名空间：

```xml
<PropertyGroup>
  <LangKeyNamespace>MyApplication.Localization</LangKeyNamespace>
</PropertyGroup>
```

Generator 默认使用 `auto` 模式：没有 `LangKey.json` 时静默跳过，存在一份时生成，多于一份时报错。可使用 `<LangKeyGeneratorEnabled>true</LangKeyGeneratorEnabled>` 开启严格模式，或设为 `false` 完全关闭；使用 `<LangKeyAutoInclude>false</LangKeyAutoInclude>` 可关闭根目录文件的自动包含与复制。

然后创建 Parser 并使用生成的 token：

```csharp
using ArkheideSystem.LangKey;
using GeneratedLangKey = MyApplication.Localization.LangKey;

var path = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
using var parser = new LangKeyParser(path, "en-US");

Console.WriteLine(parser.Format(GeneratedLangKey.Greeting, "LangKey"));

parser.Current = "zh-CN";
Console.WriteLine(parser.Format(GeneratedLangKey.Greeting, "LangKey"));
```

在 XAML 中不要再把键写成不透明的字符串。映射生成命名空间后，WPF 与 Avalonia 使用静态成员引用：

```xml
<TextBlock Text="{x:Static keys:LangKey.Greeting}" />
```

WinUI 3 使用一次性强类型绑定：

```xml
<TextBlock Text="{x:Bind keys:LangKey.Greeting, Mode=OneTime}" />
```

生成属性返回的仍是稳定 token，框架 Applicator 会解析当前文化并在语言变化后刷新。首次添加或修改键后，如果 XAML IntelliSense 尚未更新，请先构建一次项目。

## 文档与示例

- [完整使用指南](docs/usage-guide.md)
- [Demo 总览](demo/README.md)
- [Console](demo/LangKey.Demo.Console/README.md)
- [普通 WPF](demo/LangKey.Demo.Wpf/README.md)
- [WPF + DI](demo/LangKey.Demo.Wpf.DependencyInjection/README.md)
- [Avalonia + DI](demo/LangKey.Demo.Avalonia.DependencyInjection/README.md)
- [WinUI 3 + DI](demo/LangKey.Demo.WinUI3.DependencyInjection/README.md)

## 许可证

LangKey 使用 [MIT License](LICENSE.txt)。
