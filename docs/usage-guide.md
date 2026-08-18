# 使用指南

## 包

使用框架包可自动传递核心以及源生成器项目。

| 场景 | 安装包 |
| --- | --- |
| WPF | `Arkheide.Essential.Culture.Wpf` |
| Avalonia | `Arkheide.Essential.Culture.Avalonia` |
| WinUI 3 | `Arkheide.Essential.Culture.WinUI` |
| Console、.NET 应用 | `Arkheide.Essential.Culture` |


## 编写 Culture.json

安装 Nuget 后，第一次构建会在应用项目根目录自动创建以下 `Culture.json`：

```json
{
  "Greeting": {
    "en-US": "Hello, World!",
    "zh-CN": "你好，世界！"
  }
}
```

已有文件不会被覆盖。可以直接编辑该文件，将默认键替换或扩展为应用自己的资源。

文档根节点是资源键对象，每个键映射到完整的“文化 → 译文”：

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

加载时会执行以下校验：

- 根节点必须是非空 JSON 对象。
- 键必须是合法、非关键字的 C# 标识符，只允许 ASCII 字母、数字和下划线。
- 键不能包含点：应该使用 `Toolbar_Save`，不要使用 `Toolbar.Save`。
- 每个键必须声明相同的文化集合。
- 每个键都必须包含 fallback 文化，默认是 `en-US`。
- 文化名称由非空的字母或数字子标签组成，使用 `-` 或 `_` 分隔。
- 翻译值必须是非空字符串。
- 所有文化必须保留与 fallback 相同的复合格式占位符编号。

下面的文档无效，因为中文丢失了 `{1}`：

```json
{
  "Order_Summary": {
    "en-US": "{0}: {1}",
    "zh-CN": "订单：{0}"
  }
}
```

## Generator 与 Culture.json

框架包都会传递 Generator。包内的 `buildTransitive` 配置会自动：

- 在采用默认约定、没有显式 `Culture.json` 生成输入且项目根目录缺少该文件时，于第一次构建创建默认文件。
- 将文件作为 `AdditionalFiles` 交给 Generator。
- 在构建后复制到 `$(TargetDir)\Culture.json`。
- 在发布后复制到 `$(PublishDir)\Culture.json`。
- 将 Generator 配置属性暴露给编译器。

自动创建默认开启；若希望手动提供文件，可以单独关闭创建行为：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureAutoCreate>false</ArkheideEssentialCultureAutoCreate>
</PropertyGroup>
```

关闭自动创建不会影响已有根目录文件的自动包含和复制。

Generator 默认使用 `Arkheide.Essential.Culture` 命名空间，并生成供 XAML 使用的 `CultureKey` 与供 C# 使用的 `Key`：

```csharp
namespace Arkheide.Essential.Culture;

public enum CultureKey
{
    App_Title,
    Welcome_User,
}

public static class Key
{
    public static string App_Title => "Key.App_Title";
    public static string Welcome_User => "Key.Welcome_User";
}
```

如需覆盖默认命名空间：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureNamespace>MyApplication.Localization</ArkheideEssentialCultureNamespace>
</PropertyGroup>
```

Generator 默认自动识别项目引用的 XAML 适配器。单个项目同时引用多个适配器时，需要选择要生成的入口；不需要 XAML 门面时可设为 `none`：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureXamlFramework>wpf</ArkheideEssentialCultureXamlFramework>
</PropertyGroup>
```

| 值 | 行为 |
| --- | --- |
| `auto` | 默认值；自动使用唯一的 WPF、Avalonia 或 WinUI 适配器 |
| `wpf` | 生成 WPF `Localize` 入口 |
| `avalonia` | 生成 Avalonia `Localize` 入口 |
| `winui` | 生成 WinUI 3 `Localize` 入口 |
| `none` | 仅生成 `CultureKey` 和 `Key`，不生成 XAML 入口 |

Generator 支持三种启用模式：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureGeneratorEnabled>auto</ArkheideEssentialCultureGeneratorEnabled>
</PropertyGroup>
```

| 值 | 行为 |
| --- | --- |
| `auto` | 默认值；没有 `Culture.json` 时静默跳过，存在一份时生成，多于一份时报错 |
| `true` | 严格启用；必须恰好存在一份 `Culture.json` |
| `false` | 完全关闭 Generator |

若资源文件不在项目根目录，可关闭自动约定并自行声明：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureAutoInclude>false</ArkheideEssentialCultureAutoInclude>
</PropertyGroup>

<ItemGroup>
  <AdditionalFiles Include="Localization\Culture.json" />
  <None Update="Localization\Culture.json"
        TargetPath="Culture.json"
        CopyToOutputDirectory="PreserveNewest"
        CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

`ArkheideEssentialCultureAutoInclude=false` 会同时关闭默认文件创建、Generator 自动输入以及构建和发布复制；此时这些行为均由应用项目自行配置。

## C# 解析文本

第一次访问 `Localizer` 时，它会从 `AppContext.BaseDirectory` 懒加载 `Culture.json`。

```csharp
using Arkheide.Essential.Culture;
using GeneratedKey = global::Arkheide.Essential.Culture.Key;

var title = Localizer.Parse(GeneratedKey.App_Title);
var welcome = Localizer.Parse(GeneratedKey.Welcome_User, "User");
```

获取当前文化状态，使用 `Localizer.Current`：

```csharp
var culture = Localizer.Current.Culture;
var cultures = Localizer.Current.AvailableCultures;

Localizer.Current.Changed += OnLocalizationChanged;
Localizer.Current.SetCulture("zh-CN");
```

解析某个键时依次尝试：

1. 当前文化的精确匹配，例如 `zh-Hans-CN`。
2. 逐级父文化，例如 `zh-Hans`、`zh`。
3. fallback 文化，默认是 `en-US`。
4. 若键不存在，返回传入的原始键或 token。

带参数的 `Parse` 使用当前文化的 `CultureInfo` 执行复合格式化：

```csharp
Localizer.Parse(GeneratedKey.Price, 19.95m);
```

需要区分“键不存在”和“解析成功”时，可以使用参数化 `TryParse`：

```csharp
if (Localizer.TryParse(GeneratedKey.Welcome_User, "User", out var welcome))
{
    Console.WriteLine(welcome);
}
```

## XAML 中引用生成键

推荐统一使用 Generator 生成的 `Localize` 入口。键名由 `CultureKey` 强类型约束；无参文本和参数化文本使用同一个 API，参数 Binding 或当前文化变化时都会重新解析。

WPF：

```xml
<Window xmlns:culture="clr-namespace:Arkheide.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=App_Title}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
</Window>
```

Avalonia：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=App_Title}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
</Window>
```

WinUI 3：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=App_Title}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User}"
             culture:Localize.Argument0="{x:Bind ViewModel.UserName, Mode=OneWay}" />
</Window>
```

WPF 与 Avalonia 可使用 `Arg0`、`Arg1`、`Arg2`。参数必须是 Binding；常量可写成 `Arg0={Binding Source=Arkheide}`。需要更多参数时使用对象元素形式的 `Arguments` 集合：

```xml
<TextBlock.Text>
  <culture:Localize Key="Order_Summary">
    <Binding Path="OrderNumber" />
    <Binding Path="Total" />
    <Binding Path="CreatedAt" />
    <Binding Path="Status" />
  </culture:Localize>
</TextBlock.Text>
```

`Arguments` 不能与 `Arg0`–`Arg2` 混用。WPF/Avalonia 的 `Localize` 自身持有本地化 Binding，不需要额外初始化或视觉树扫描。

`Key` 是 Generator 生成的强类型 `CultureKey` 枚举属性，编辑器通常可以列出所有可用键，并由 XAML 编译器校验键名。

`Arguments` 可传入 `IList<object?>` 作为任意数量参数；设置后优先于三个独立参数。

> [!NOTE]
> 如果 IntelliSense 尚未更新，请先构建一次项目。

## WPF

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.Wpf
```

正常创建窗口，并在需要时设置初始文化：

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    Localizer.Current.SetCulture("en-US");
    var window = new MainWindow();
    MainWindow = window;
    window.Show();
}
```

`Localize` 直接返回 WPF `MultiBinding`，可用于 `Window.Title`、`TextBlock.Text`、Content、Header、ToolTip 等支持 Binding 的属性。参数源变化和 `Localizer.Current.Changed` 都会触发重新解析。


## Avalonia

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.Avalonia
```

在桌面生命周期中直接创建窗口：

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        Localizer.Current.SetCulture("en-US");
        desktop.MainWindow = new MainWindow();
    }

    base.OnFrameworkInitializationCompleted();
}
```

使用 `Localize` 后，ViewModel 只需要公开原始参数；文化变化与参数属性通知会由生成的本地化 Binding 组合处理：

```xml
<TextBlock Text="{culture:Localize Key=Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

`Localize` 可用于 `Window.Title`、`TextBlock.Text`、Content、Header、`TextBox.PlaceholderText`、ToolTip 等支持 Binding 的属性。

## WinUI 3

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.WinUI
```

WinUI 3 需要一个窗口刷新协调器，这是 `Localize` 的框架运行基础设施。在窗口激活前调用 `Attach`：

```csharp
private readonly WinUILocalizationHost localizationHost = new();
private Window? window;

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    window = new MainWindow();
    localizationHost.Attach(window);
    window.Activate();
}
```

如果在 C# 中创建 `ContentDialog`，遵循 C# 标准并通过 `Localizer` 生成最终文本：

```csharp
var dialog = new ContentDialog
{
    Title = Localizer.Parse(GeneratedKey.App_Title),
    Content = Localizer.Parse(GeneratedKey.Welcome_User, "User"),
    CloseButtonText = Localizer.Parse(GeneratedKey.Action_Close),
    XamlRoot = Root.XamlRoot,
};

await dialog.ShowAsync();
```

由 XAML 创建的独立根仍应在显示前通过协调器登记，以便其中的 `Localize` 标记得到发现和刷新：

```csharp
localizationHost.Apply(dialogRoot);
```
