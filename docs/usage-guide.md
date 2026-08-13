# LangKey 使用指南

本文介绍 LangKey 的资源格式、Source Generator、核心运行时、依赖注入，以及 WPF、Avalonia 和 WinUI 3 的框架适配方式。

## 1. 选择依赖

LangKey 将运行时、编译期生成、依赖注入和 UI 框架适配分开维护。面向最终应用的默认包已经组合好所需依赖：

| 场景 | 建议依赖 |
| --- | --- |
| Console、服务或手动管理 Parser | `ArkheideSystem.LangKey` |
| 仅需 Microsoft DI | `ArkheideSystem.LangKey.DependencyInjection` |
| WPF，默认包含 DI | `ArkheideSystem.LangKey.Wpf` |
| Avalonia，默认包含 DI | `ArkheideSystem.LangKey.Avalonia` |
| WinUI 3，默认包含 DI | `ArkheideSystem.LangKey.WinUI` |
| WPF，不使用 DI | `ArkheideSystem.LangKey.Wpf.Runtime` |
| Avalonia，不使用 DI | `ArkheideSystem.LangKey.Avalonia.Runtime` |
| WinUI 3，不使用 DI | `ArkheideSystem.LangKey.WinUI.Runtime` |

依赖关系如下：

```text
LangKey.Wpf / LangKey.Avalonia / LangKey.WinUI
├── 对应框架的 Runtime 适配器
└── LangKey.DependencyInjection
    └── LangKey
        └── LangKey.Generator（仅编译期）
```

因此安装任一默认 UI 包都会自动获得 Core、DI 和 Generator。Generator 是 Analyzer，不会进入应用运行时输出。`.Runtime` 包只携带对应框架 Applicator、Core 和 Generator，用于明确不采用 DI 的项目。

所有包均保持框架职责边界；LangKey 不要求应用或文化来源依赖任何特定控件库。

## 2. 编写 LangKey.json

文档根节点是资源键对象；每个键映射到完整的“文化 → 文本”集合：

```json
{
  "App_Title": {
    "en-US": "My application",
    "zh-CN": "我的应用"
  },
  "Welcome_User": {
    "en-US": "Welcome, {0}!",
    "zh-CN": "欢迎，{0}！"
  }
}
```

运行时会执行以下校验：

- 根节点必须是非空 JSON 对象。
- 键必须是合法、非关键字的 C# 标识符，只允许 ASCII 字母、数字和下划线。
- 键不能包含点；例如使用 `Toolbar_Save`，不要使用 `Toolbar.Save`。
- 每个键必须声明相同的文化集合。
- 每个键都必须包含 fallback 文化，默认是 `en-US`。
- 文化名称由非空的字母/数字子标签组成，使用 `-` 或 `_` 分隔；已知文化按 `CultureInfo` 规范化，自定义文化按 LangKey 规则规范化。
- 翻译值必须是非空字符串。
- 所有文化必须保留与 fallback 相同的复合格式占位符编号。

例如下面的文档无效，因为中文丢失了 `{1}`：

```json
{
  "Order_Summary": {
    "en-US": "{0}: {1}",
    "zh-CN": "订单：{0}"
  }
}
```

## 3. Source Generator 与资源文件约定

Core 会自动传递 Generator，DI 和三个默认 UI 包又会自动传递 Core，因此 NuGet 使用者不需要额外安装 `ArkheideSystem.LangKey.Generator`。

把 `LangKey.json` 放在项目根目录后，包内的 `buildTransitive` 配置会自动：

- 将它作为 `AdditionalFiles` 交给 Generator。
- 在构建后复制到 `$(TargetDir)\LangKey.json`。
- 在发布后复制到 `$(PublishDir)\LangKey.json`。
- 将 `LangKeyNamespace` 与 `LangKeyGeneratorEnabled` 暴露给 Generator。

通常只需配置生成代码的命名空间：

```xml
<PropertyGroup>
  <LangKeyNamespace>MyApplication.Localization</LangKeyNamespace>
</PropertyGroup>
```

Generator 支持三种模式：

```xml
<PropertyGroup>
  <LangKeyGeneratorEnabled>auto</LangKeyGeneratorEnabled>
</PropertyGroup>
```

| 值 | 行为 |
| --- | --- |
| `auto` | 默认值；没有 `LangKey.json` 时不生成也不报错，存在一份时生成，多于一份时报错 |
| `true` | 严格启用；必须恰好存在一份 `LangKey.json` |
| `false` | 完全关闭 Generator |

如果不希望自动识别项目根目录的文件，可关闭约定并自行声明文件及复制规则：

```xml
<PropertyGroup>
  <LangKeyAutoInclude>false</LangKeyAutoInclude>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="Localization\LangKey.json" />
  <None Update="Localization\LangKey.json"
        TargetPath="LangKey.json"
        CopyToOutputDirectory="PreserveNewest"
        CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

Generator 会产生：

```csharp
namespace MyApplication.Localization;

public static class LangKey
{
    public static string App_Title => "LangKey.App_Title";
    public static string Welcome_User => "LangKey.Welcome_User";
}
```

这些属性是稳定 token，不是已经翻译的字符串。把 token 交给 `ILangKeyResolver.Get` 或 `Format` 后才会得到当前文化的文本。

### 在 XAML 中使用生成属性

普通的 `Text="LangKey.Greeting"` 只是 `string` 字面量，XAML 编辑器无法把句点后的内容关联到 Generator 生成类。UI 项目应配置 `LangKeyNamespace`，再通过框架原生的强类型语法引用静态属性：

```xml
<!-- WPF -->
<Window xmlns:keys="clr-namespace:MyApplication.Localization">
  <TextBlock Text="{x:Static keys:LangKey.Greeting}" />
</Window>

<!-- Avalonia -->
<Window xmlns:keys="using:MyApplication.Localization">
  <TextBlock Text="{x:Static keys:LangKey.Greeting}" />
</Window>

<!-- WinUI 3 -->
<Window xmlns:keys="using:MyApplication.Localization">
  <TextBlock Text="{x:Bind keys:LangKey.Greeting, Mode=OneTime}" />
</Window>
```

这样 XAML 编译器会解析实际 CLR 类型和静态成员，能够检查键是否存在，并可由编辑器提供成员补全。WinUI 明确使用 `Mode=OneTime`：静态属性只负责在初始化时提供 token，之后由 Applicator 写入译文并处理文化切换，不让绑定再次覆盖译文。

Generator 的属性仍返回 `LangKey.Greeting`，而不是某一种文化的译文，因此强类型写法不会改变运行时刷新机制。新增或修改 JSON 键后，XAML 设计时构建可能尚未加载最新的 Analyzer 输出；此时先构建项目，再重新触发 IntelliSense。

如果生成类名与 `ArkheideSystem.LangKey` 命名空间产生歧义，可以使用别名：

```csharp
using GeneratedLangKey = MyApplication.Localization.LangKey;
```

仓库内的 Demo 使用源码 `ProjectReference`，不会执行已打包 NuGet 的 `buildTransitive` 导入。因此这些示例仍显式引用 Generator、链接共享 JSON，并传递 MSBuild 属性：

```xml
<CompilerVisibleProperty Include="LangKeyNamespace" />
```

真实应用使用 NuGet 包时不需要这些仓库内部配置。

## 4. 直接使用核心运行时

建议始终从应用输出目录构造绝对路径：

```csharp
var path = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
using var parser = new LangKeyParser(path, current: "en-US", fallback: "en-US");
```

常用操作：

```csharp
var title = parser.Get(GeneratedLangKey.App_Title);
var welcome = parser.Format(GeneratedLangKey.Welcome_User, "Evigila");

var exists = parser.Contains(GeneratedLangKey.App_Title);
var cultures = parser.AvailableCultures;
var keys = parser.Keys;

parser.Current = "zh-CN";
parser.Reload();
```

`ILangKeyParser` 允许切换文化和重新加载文档；`ILangKeyResolver` 只公开解析能力，适合注入普通业务服务和 ViewModel。

Parser 的 `Changed` 事件包含两种变化：

- `LangKeyChangeKind.CultureChanged`
- `LangKeyChangeKind.DocumentReloaded`

订阅者应在不再使用时退订；直接创建的 Parser 应调用 `Dispose()`。

## 5. 文化选择与 fallback

解析某个键时，LangKey 按以下顺序查找：

1. 当前文化的精确匹配，例如 `zh-Hans-CN`。
2. 逐级父文化，例如 `zh-Hans`、`zh`。
3. 配置的 fallback 文化。
4. 如果键不存在，返回传入的原始键或 token，便于发现遗漏。

`Format` 使用当前文化的 `CultureInfo` 执行 `string.Format`：

```csharp
parser.Format(GeneratedLangKey.Price, 19.95m);
```

## 6. 使用依赖注入

安装 `ArkheideSystem.LangKey.DependencyInjection` 后即可获得 DI、Core 和 Generator：

```csharp
var services = new ServiceCollection();

services.AddLangKey(
    "LangKey.json",
    initialCulture: _ => "en-US",
    fallback: "en-US"
);

using var provider = services.BuildServiceProvider();

var resolver = provider.GetRequiredService<ILangKeyResolver>();
var parser = provider.GetRequiredService<ILangKeyParser>();
```

DI 注册中的相对路径会基于 `AppContext.BaseDirectory` 解析。Parser 与 Resolver 指向同一个 Singleton 运行时。

不要在同一 `IServiceCollection` 中重复调用 `AddLangKey`，也不要预先注册 LangKey 自己管理的 `ILangKeyParser` 或 `ILangKeyResolver`。

## 7. 跟随外部文化来源

如果语言由设置服务、系统服务或其他模块控制，实现 `ILangKeyCultureSource`：

```csharp
public sealed class ApplicationCultureSource : ILangKeyCultureSource
{
    public string CurrentCulture { get; private set; } = "en-US";

    public event EventHandler<LangKeyCultureChangedEventArgs>? Changed;

    public void SetCulture(string culture)
    {
        if (CurrentCulture == culture)
        {
            return;
        }

        CurrentCulture = culture;
        Changed?.Invoke(this, new LangKeyCultureChangedEventArgs(culture));
    }
}
```

然后注册文化源和 LangKey：

```csharp
services.AddSingleton<ApplicationCultureSource>();
services.AddLangKey<ApplicationCultureSource>("LangKey.json");
```

Parser 创建时会读取 `CurrentCulture`，并在文化源触发 `Changed` 时同步更新。文化源接口属于 LangKey Core，不要求实现方依赖某个 UI 框架。

## 8. WPF Runtime：不使用 DI

不使用 DI 时安装 `ArkheideSystem.LangKey.Wpf.Runtime`。它只提供 WPF Applicator，并自动获得 Core 和 Generator。

WPF Applicator 会识别常见显示属性中的 `LangKey.*` token：

- `Window.Title`
- `TextBlock.Text`，但不会修改 `TextBoxBase.Text`
- `Content`、`Header`、`Placeholder`、`ToolTip`
- `AutomationProperties.Name`
- `DataGridColumn.Header`
- `ItemsControl.ItemsSource` 中实现 `ILangKeyLocalizable` 的数据项

XAML 使用 `x:Static` 引用 Generator 产生的稳定 token：

```xml
<Window xmlns:keys="clr-namespace:MyApplication.Localization"
        Title="{x:Static keys:LangKey.App_Title}">
  <StackPanel>
    <TextBlock Text="{x:Static keys:LangKey.Welcome_User}" />
    <Button Content="{x:Static keys:LangKey.Action_Save}" />
  </StackPanel>
</Window>
```

应用启动时：

```csharp
var path = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
var parser = new LangKeyParser(path, "en-US");
var applicator = new LangKeyWpfApplicator(parser);

applicator.Start(Dispatcher);

var window = new MainWindow(parser);
applicator.Apply(window); // 必须在 Show 前执行，避免首帧显示 token。
window.Show();
```

`Start` 负责 Loaded 事件和后续文化变化；`Apply` 负责立即扫描已创建的对象树。退出时释放 Applicator 和 Parser。

## 9. WPF 默认包：DI + Generic Host

安装 `ArkheideSystem.LangKey.Wpf`。该默认入口自动获得 WPF Runtime、DI、Core 和 Generator：

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(this);
builder.Services.AddLangKeyWpf<App>(
    "LangKey.json",
    initialCulture: _ => "en-US"
);
builder.Services.AddSingleton<MainWindow>();

var host = builder.Build();
await host.StartAsync();

var window = host.Services.GetRequiredService<MainWindow>();
host.Services.GetRequiredService<ILangKeyWpfApplicator>().Apply(window);
window.Show();
```

使用外部文化源时：

```csharp
builder.Services.AddSingleton<ApplicationCultureSource>();
builder.Services.AddLangKeyWpf<App, ApplicationCultureSource>("LangKey.json");
```

`AddLangKeyWpf` 会注册 Core、Applicator 和一个 `IHostedService`。Host 启动后 Applicator 开始监听；Host 停止或释放时自动退订。

## 10. Avalonia

默认安装 `ArkheideSystem.LangKey.Avalonia`。它自动获得 Avalonia Runtime、DI、Core 和 Generator。使用 Generic Host 时注册应用和框架适配器：

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton(this);
builder.Services.AddSingleton<ApplicationCultureSource>();
builder.Services.AddLangKeyAvalonia<App, ApplicationCultureSource>("LangKey.json");
builder.Services.AddTransient<MainWindow>();

using var host = builder.Build();
host.Start();

var window = host.Services.GetRequiredService<MainWindow>();
host.Services.GetRequiredService<ILangKeyAvaloniaApplicator>().Apply(window);
desktop.MainWindow = window;
```

Applicator 识别 `Window.Title`、`TextBlock.Text`、Content、Header、`TextBox.PlaceholderText`、ToolTip 和 Automation Name 中的 `LangKey.*` token。HostedService 负责启动 Loaded 监听及文化变化刷新；首次显示窗口或对话框前调用 `Apply(root)`，可避免首帧出现 token。

```xml
<Window xmlns:keys="using:MyApplication.Localization"
        Title="{x:Static keys:LangKey.App_Title}">
  <StackPanel>
    <TextBlock Text="{x:Static keys:LangKey.Greeting}" />
    <Button Content="{x:Static keys:LangKey.Action_SwitchLanguage}" />
  </StackPanel>
</Window>
```

明确不使用 DI 时安装 `ArkheideSystem.LangKey.Avalonia.Runtime`，自行创建 `LangKeyParser` 和 `LangKeyAvaloniaApplicator`，调用 `Start(Application)`、`Apply(Visual)`，并在退出时释放二者。

## 11. WinUI 3

默认安装 `ArkheideSystem.LangKey.WinUI`。它自动获得 WinUI Runtime、DI、Core 和 Generator：

```csharp
var services = new ServiceCollection();
services.AddSingleton<ApplicationCultureSource>();
services.AddLangKeyWinUI<ApplicationCultureSource>("LangKey.json");
services.AddSingleton<MainWindow>();

using var provider = services.BuildServiceProvider();
var window = provider.GetRequiredService<MainWindow>();
provider.GetRequiredService<ILangKeyWinUIApplicator>().Attach(window);
window.Activate();
```

`Attach(window)` 必须在窗口所属 UI 线程调用，它会立即应用 Window 标题及视觉树，并在文化变化后刷新；窗口关闭时自动解除跟踪。对于 `ContentDialog`、Popup 内容或其他独立根，在显示前调用：

```csharp
applicator.Apply(dialog);
await dialog.ShowAsync();
```

Applicator 识别 Text、Content、ContentDialog 按钮文本、Placeholder、ToolTip 和 Automation Name 等 WinUI 依赖属性。明确不使用 DI 时安装 `ArkheideSystem.LangKey.WinUI.Runtime`，自行创建 Parser 和 Applicator；Applicator 释放时会解除全部窗口和文化变化订阅。

WinUI XAML 使用静态属性的 `x:Bind`，不要使用不透明的 token 字符串：

```xml
<Window xmlns:keys="using:MyApplication.Localization"
        Title="{x:Bind keys:LangKey.App_Title, Mode=OneTime}">
  <TextBlock Text="{x:Bind keys:LangKey.Greeting, Mode=OneTime}" />
  <Button Content="{x:Bind keys:LangKey.Action_SwitchLanguage, Mode=OneTime}" />
</Window>
```

`Mode=OneTime` 只在初始化时把 token 写入依赖属性；`Attach` 随后将其翻译，并在文化变化后持续刷新。

## 12. 部署与打包检查

发布前确认：

- 输出目录中存在 `LangKey.json`。
- Generator 只作为传递 Analyzer 使用，不应成为运行时程序集依赖。
- JSON 中每个键具有相同文化集合。
- fallback 与构造或注册时配置一致。
- 直接创建的 Parser 和 Applicator 已释放。
- WPF 在窗口 `Show()` 前执行过一次 `Apply(window)`。
- Avalonia 在首次呈现窗口或对话框前执行过一次 `Apply(root)`。
- WinUI 窗口在 `Activate()` 前已经 `Attach(window)`，独立对话框在显示前已经 `Apply(dialog)`。
- 使用 `.Runtime` 包时，Parser 和 Applicator 均已释放。

## 13. 常见问题

### Generator 报告找不到 LangKey.json

默认 `auto` 模式在没有文件时会静默跳过；`true` 模式要求恰好一份文件。检查文件名与位置，并确认项目中只有一个名为 `LangKey.json` 的 `AdditionalFiles`。

### 编译成功但运行时找不到文件

项目根目录的 `LangKey.json` 会由 NuGet 包自动复制。如果关闭了 `LangKeyAutoInclude` 或把文件放在其他目录，需要自行配置：

```xml
<None Update="LangKey.json" CopyToOutputDirectory="PreserveNewest" />
```

### WPF 首次显示 LangKey.*

窗口构造完成后、`Show()` 之前调用：

```csharp
applicator.Apply(window);
```

### Avalonia 或 WinUI 首次显示 LangKey.*

Avalonia 在呈现根之前调用 `Apply(root)`；WinUI 在 `Activate()` 前调用 `Attach(window)`，并在显示 ContentDialog 前调用 `Apply(dialog)`。

### 切换文化后自定义 MVVM 属性不刷新

三个 UI Applicator 只会刷新它们管理的 XAML token。由 ViewModel 自行调用 `resolver.Get(...)` 产生的属性仍应订阅 `ILangKeyResolver.Changed`，并触发相应的 `PropertyChanged`。

### 为什么未知键直接显示 token

这是刻意的诊断行为，能够让开发和测试阶段立即发现资源遗漏。

### XAML 中输入 LangKey. 没有补全

确认使用的是 `{x:Static keys:LangKey.}`（WPF/Avalonia）或 `{x:Bind keys:LangKey., Mode=OneTime}`（WinUI 3），而不是 `Text="LangKey."` 普通字符串；同时检查 `xmlns:keys` 是否指向 `LangKeyNamespace` 配置的生成命名空间。首次生成或修改键后先构建一次项目，让 XAML 设计时构建读取最新成员。
