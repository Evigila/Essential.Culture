# Essential.Culture

[English](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/README.md) | [简体中文](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/README_zh-CN.md)

`Essential.Culture` 是一个面向 .NET 的 JSON 文化管理组件。

它使用 `Culture.json` 管理多语言文化键，支持可选功能模块，通过 Source Generator 生成强类型键，并为 WPF、Avalonia、WinUI 3 和独立作用域的 Blazor 服务提供运行时文化切换。

## 功能

- `Culture.json` 是约定俗成的 JSON 名称，将自动被识别。
- 源生成 `CultureKey` 和统一的 `Localize` XAML API，提供强类型键与编译期检查。
- 通过 `KeyBinding` 让集合项或运行时状态动态选择翻译键。
- 通过 `Localizer.Parse(...)` 和 `Localizer.TryParse(...)` 解析文化键，支持参数化。
- 通过 `Localizer.Current` 动态操作文化。
- 启动语言策略在保留目录内存中排除不需要的翻译，同时严格验证全部原始资源。
- 可选 `CultureModule` 将多个多语言文件组合为一个目录，键在目录中全局唯一。

## 快速开始

核心、Avalonia 和 Blazor 包面向 **.NET 10**。WPF 要求 `net10.0-windows`；WinUI 3 要求 `net10.0-windows10.0.19041.0`，最低支持 Windows `10.0.17763.0`。Generator 是 `netstandard2.0` 分析器，不能单独提供翻译运行时。Blazor 包依赖 ASP.NET Core 共享框架，支持服务端渲染和 Interactive Server，不面向独立 WebAssembly。

安装宿主对应的包。各适配器都会传递引入核心包和 Generator，无需在一个应用中安装全部适配器。

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

服务端 Blazor 项目：

```powershell
dotnet add package Arkheide.Essential.Culture.Blazor
```

Console 或其他 .NET 10 宿主：

```powershell
dotnet add package Arkheide.Essential.Culture
```

首次构建时，传递引入的 Generator 会在项目根目录创建缺失的 `Culture.json`，并复制到构建和发布目录。已有文件不会被覆盖。可以定义自己的键和译文：

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

每个文件必须是非空 JSON 对象。键区分大小写，必须是 ASCII C# 标识符：仅含字母、数字和下划线，不能以数字开头或使用 C# 关键字；生成保留名称 `Key`、`CultureKey`、`value__` 也会被拒绝。同一文件中所有键的规范化语言集合必须一致，默认包含 `en-US`。译文必须为非空字符串，使用合法复合格式；各语言必须保留回退译文的占位符索引及出现次数。不同模块文件可拥有不同的可选语言集合。

保持原有单文件配置时，`Localizer` 首次使用会加载 `AppContext.BaseDirectory/Culture.json`，当前语言和回退语言均为 `en-US`。它不会自动扫描文件夹或加载模块。

## 立即使用

> [!NOTE]
> 如果 IntelliSense 尚未显示生成类型，请先构建一次项目。

WPF 使用 `Localize`，参数可以直接使用 Binding：

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
    <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
  </StackPanel>
</Window>
```

Avalonia 使用相同 API，仅命名空间语法不同：

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
    <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
  </StackPanel>
</Window>
```

使用强类型 `Key=` 属性，让编辑器能够根据生成的 `CultureKey` 枚举提供键候选。键来自 DataContext 或运行时状态时使用 `KeyBinding=`。

WinUI 3 的动态参数通过同一 `Localize` 类型的附加属性提供：

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <StackPanel>
    <TextBlock Text="{culture:Localize Key=Greeting}" />
    <TextBlock Text="{culture:Localize Key=Welcome_User}"
               culture:Localize.Argument0="{x:Bind ViewModel.UserName, Mode=OneWay}" />
    <TextBlock Text="{culture:Localize}"
               culture:Localize.KeyBinding="{x:Bind ViewModel.CurrentTextKey, Mode=OneWay}" />
  </StackPanel>
</Window>
```

WinUI 3 必须创建并保留宿主，在各窗口完成 `InitializeComponent` 后附加窗口：

```csharp
using ArkheideSystem.Essential.Culture.WinUI;

var localizationHost = new WinUILocalizationHost();
var window = new MainWindow();
localizationHost.Attach(window);
window.Closed += (_, _) => localizationHost.Dispose(); // 单窗口应用
window.Activate();
```

附加、解除附加和释放应在所属 UI 线程执行。多窗口应用应附加每个窗口，在最后一个窗口关闭后释放共享宿主。宿主解析标记并刷新所支持的 WinUI 显示依赖属性；对话框和生命周期集成可参考 WinUI Gallery。

这些 XAML 片段用于现有窗口定义，请补齐其常规 `x:Class`、DataContext 或 `ViewModel`。`Key` 和 `KeyBinding` 不能同时使用。动态键必须为原始键字符串或生成的 `Key.*` token；生成的 `CultureKey` 枚举用于静态 `Key` 属性。WPF/Avalonia 绑定和已附加的 WinUI 窗口会在语言或参数改变时刷新本地化目标值。`Parse` 返回的普通字符串是查询时的快照；自行维护的 UI 状态需要重新查询或监听 `Changed` 才能更新。

## Blazor

在服务端 Blazor 宿主中，显式加载一次目录，并在 `builder.Build()` 前注册作用域服务：

```csharp
using ArkheideSystem.Essential.Culture;
using Microsoft.Extensions.DependencyInjection;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"));
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("App", catalog)
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US", "zh-CN"));
```

宿主已配置 Interactive Server 时，组件可使用：

```razor
@using ArkheideSystem.Essential.Culture.Blazor
@inherits LocalizedComponentBase
@rendermode Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer

<LocalizedText Key="Welcome_User" Arguments="@(new object?[] { "Blazor" })" />
<button @onclick="UseChinese">中文</button>

@code {
    private void UseChinese() => Localization.SetCulture("zh-CN");
}
```

`LocalizedComponentBase` 订阅作用域服务并刷新组件；`LocalizedText` 对文本进行编码输出。其他服务也可注入 `ILocalizationService`。每个 HTTP 请求或 Interactive Server circuit 保持独立语言选择，共享不可变目录数据。Blazor 用户语言状态应使用作用域服务，避免存入进程共享的桌面 `Localizer`。

默认初始化使用受支持的 `CurrentUICulture` 和 `CurrentCulture`，不可用时使用配置的默认值；也可用 `InitializeWith` 提供宿主状态。请求本地化中间件、语言 Cookie、持久化和格式化语言限制由宿主负责。`SetCulture` 只改变当前作用域。SSR/交互初始化和 Cookie 持久化可参考 Blazor Gallery。

## 源生成

Generator 自动识别项目根目录的 `Culture.json`，并复制到构建和发布目录。在消费程序集内生成公开的 `CultureKey` 枚举和 `Key` 类型；引用 XAML 适配器时，还会生成公开的 `Localize` 标记扩展，它是实例类型，并非静态入口。以下为简化的生成示例：

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

// 新查询使用新选择的语言
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));
```

> [!NOTE]
> 默认文化与 fallback 均为 `en-US`。

如需覆盖生成类型所在命名空间，可在项目中设置：

```xml
<PropertyGroup>
  <EssentialCultureNamespace>ArkheideSystem.MyApplication.Localization</EssentialCultureNamespace>
</PropertyGroup>
```

自动创建不会覆盖已有 `Culture.json`。如仅需关闭模板创建，可设置：

```xml
<PropertyGroup>
  <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
</PropertyGroup>
```

`EssentialCultureAutoInclude=false` 关闭自动资源准备、创建、复制和模块输出清理，由宿主管理这些操作。`EssentialCultureGeneratorEnabled=false` 单独关闭 C# 生成和源诊断，不影响复制或 MSBuild 模块元数据验证。`EssentialCultureXamlFramework` 默认为 `auto`；引用多个 XAML 适配器时，应显式选择 `wpf`、`avalonia`、`winui` 或 `none`。

## 独立上下文

实例上下文拥有独立语言选择，不改变环境 .NET 文化或桌面静态入口：

```csharp
using ArkheideSystem.Essential.Culture;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"));
var english = new LocalizationContext(catalog, "en-US");
var chinese = new LocalizationContext(catalog, "zh-CN", formatCulture: "de-DE");
Console.WriteLine(chinese.Parse("Welcome_User", "Ada"));
chinese.SetCulture("en-US", "de-DE"); // english 不受影响
```

也可用 `FromJson` 或 `Load(Stream)` 创建目录；流仍由调用者负责释放。查询接受原始键和生成的 `Key.*` token。`Contains` 检查键是否声明；键缺失时，`TryParse` 返回 `false` 和空文本，`Parse` 原样返回合法的缺失 token。`Parse` 的非法 token 和格式化错误会抛异常；`TryParse` 也不会吞掉格式化错误。一、二、三个参数的泛型重载避免分配参数数组，但格式化仍会分配结果字符串。

## 语言策略与可选模块（1.4.0）

现有单文件应用保持默认行为。可在创建上下文或桌面视图之前限制所加载的翻译语言：

```csharp
using ArkheideSystem.Essential.Culture;

var catalog = LocalizationCatalog.FromFile(
    Path.Combine(AppContext.BaseDirectory, "Culture.json"), "en-US",
    new CatalogLoadOptions(enabledCultures: ["en-US", "zh-CN"]));
Localizer.Configure(catalog, "en-US");
```

`Configure` 只能调用一次，必须早于任何静态查询、状态访问或事件订阅，包括创建桌面视图之前；后续配置会被拒绝。它接受独立格式化语言，但之后的 `Localizer.Current.SetCulture` 会使用所选 UI 语言进行格式化。实例上下文和 Blazor 可在每次切换时分别设置格式化语言。

- 没有策略，或使用空的默认选项时，任意合法语言请求仍按原有“精确标签 → 父标签 → 配置回退语言”解析；此时 `AvailableCultures` 列出原文件语言，而非所有合法请求。
- 显式 `enabledCultures` 仅允许列出的规范化请求，可包含合法自定义标签。空列表不允许任何语言。允许请求所需的原文件父语言及配置回退语言即使不在允许列表中，也会保留。
- 未指定允许列表且 `disabledCultures` 非空时，仅允许原文件中未被精确禁用的语言；此模式也拒绝原文件未声明的请求。
- 标签大小写和下划线会规范化。禁用 `fr` 不会自动禁用已允许或已声明的 `fr-CA`，但会从其回退链删除 `fr` 翻译。允许 `fr-CA` 可保留原文件中的 `fr` 父语言，除非父语言已显式禁用。
- 允许和禁用列表中存在相同规范化标签时，配置失败；禁用配置回退语言时，加载失败。`DeclaredCultures` 列出原文件语言，`AvailableCultures` 列出策略允许的请求，`IsCultureEnabled` 检查选择资格。上下文构造和 `SetCulture` 执行同一策略。

格式化语言不受翻译策略限制。包括被排除语言在内的全部译文仍会严格验证，启动期间仍存在临时分配；完整目录只保留必要翻译。这不会删除部署 JSON 文件中的文本。语言选择控件应使用 `AvailableCultures`，硬编码切换命令也应遵循策略。

模块文件可显式声明：

```xml
<ItemGroup>
  <CultureModule Include="Culture.MainPage.json" ModuleId="MainPage"
                 DeploymentPath="Pages/Culture.MainPage.json" />
</ItemGroup>
```

显式声明无需启用自动发现。已有根文件 `Culture.json` 仍会包含；存在模块声明时，不会创建缺失的根模板。也可设置：

```xml
<PropertyGroup>
  <EssentialCultureModulesEnabled>true</EssentialCultureModulesEnabled>
  <EssentialCultureFallbackCulture>en-US</EssentialCultureFallbackCulture>
</PropertyGroup>
```

这仅发现项目根目录的 `Culture.*.json`，排除 `Culture.options.json`，不会递归扫描文件夹。每个文件保持 `键 -> 语言 -> 文本` 结构；键在整个目录中必须唯一，同一文件的各键语言集合必须一致并包含回退语言。不同文件可包含不同的可选语言。`EssentialCultureFallbackCulture` 默认为 `en-US`，影响构建验证和清单，修改它不会自动重新配置默认静态入口。

`ModuleId` 默认使用不含扩展名的文件名；`DeploymentPath` 默认使用 `Link`，否则使用文件名。ID 和部署路径均须在忽略大小写时唯一。ID 仅使用 ASCII 字母、数字、`_`、`-`、`.`；部署路径必须是安全相对 JSON 路径，不能含根路径、遍历段或保留文件名。同名链接资源应指定不同的部署路径。

生成器在配置的命名空间中提供内部清单 `CultureResources.Files` 和 `CultureResources.FallbackCulture`：

```csharp
using ArkheideSystem.Essential.Culture; // 默认生成命名空间

var paths = CultureResources.Files.Select(path => Path.Combine(AppContext.BaseDirectory, path));
var catalog = LocalizationCatalog.FromFiles(paths, CultureResources.FallbackCulture,
    new CatalogLoadOptions(disabledCultures: ["zh-CN"]));
Localizer.Configure(catalog, culture: "en-US");
```

`CultureResources` 是消费程序集内的内部类型；若修改了生成命名空间，请导入该命名空间。清单不会执行初始化。使用不同回退语言或策略时，请显式选择一个允许的初始语言。

模块会复制到声明的构建和发布路径。声明改变时，构建目标会在同一输出目录内清理此前记录的陈旧模块路径，并保护其他当前内容。`FromFiles` 预先读取并验证全部指定文件，拒绝重复的规范化路径，键冲突报告包含两个来源文件。它不执行延迟加载、模块卸载或查询时文件发现。

Blazor 通过 `AddCultureBlazor`/`AddCatalog` 注册组合目录；宿主支持的 UI 语言必须被所有已注册目录允许，否则注册失败。独立类库目录保留各自键空间，可通过 `ParseFrom` 或 `CatalogId` 选择。模块源验证诊断为 `AEC001`–`AEC007`；即使关闭生成，MSBuild 仍会拒绝非法 ID 或路径。

成功的原始键/token 查询没有文件 I/O，实测每次调用零分配。选择表按实际保留的有效语言共享，缓存规模受保留语言数约束。启动验证和首次选择仍有成本，不构成通用延迟保证。[性能测量](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-performance.md)记录了工作负载和适用限制。

## 包

| 包 | 用途 |
| --- | --- |
| `Arkheide.Essential.Culture` | 解析文化状态与 `Localizer` 静态入口 |
| `Arkheide.Essential.Culture.Generator` | 从 `Culture.json` 生成强类型键；通常自传递，无需单独安装 |
| `Arkheide.Essential.Culture.Wpf` | WPF 强类型 `Localize` XAML Binding |
| `Arkheide.Essential.Culture.Avalonia` | Avalonia 强类型 `Localize` XAML Binding |
| `Arkheide.Essential.Culture.WinUI` | WinUI 3 强类型 `Localize` 与窗口刷新基础设施 |
| `Arkheide.Essential.Culture.Blazor` | 请求或 circuit 独立的服务与编码文本组件 |

```powershell
# 仅需要源生成的项目可选直接安装：
dotnet add package Arkheide.Essential.Culture.Generator
```

## AI 辅助

> [!IMPORTANT]
> 本库使用了 AI Agent (ChatGPT Codex) 技术来辅助编写。

## 文档与示例

- [单文件/XAML 使用指南](https://github.com/Evigila/Essential/blob/master/src/Essential.Culture/docs/usage-guide.md)：既有指南，不覆盖上述新增的 1.4.0 策略、模块或 Blazor 配置。
- [1.4.0 资源配置与手动测试清单](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-resource-usage.md)
- [完整公开 API 清单](https://github.com/Evigila/Essential/blob/master/docs-ai/current/culture-public-api.md)
- [Console Gallery 源码](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture)
- [WPF Gallery 源码](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Wpf)
- [Avalonia Gallery 源码](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Avalonia)
- [WinUI 3 Gallery 源码](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.WinUI)
- [Blazor Gallery 源码](https://github.com/Evigila/Essential/tree/master/demo/Gallery.Essential.Culture.Blazor)

## 许可证

使用 [MIT License](https://github.com/Evigila/Essential/blob/master/LICENSE.txt)。
