# Arkheide.Essential.Culture 使用指南

## 1. 选择包

Arkheide.Essential.Culture 公开五个包。普通 .NET 应用安装 Core；UI 应用安装对应框架包即可：

| 场景 | 安装包 |
| --- | --- |
| Console、服务或普通 .NET 应用 | `Arkheide.Essential.Culture` |
| WPF | `Arkheide.Essential.Culture.Wpf` |
| Avalonia | `Arkheide.Essential.Culture.Avalonia` |
| WinUI 3 | `Arkheide.Essential.Culture.WinUI` |
| 仅单独引用 Source Generator | `Arkheide.Essential.Culture.Generator` |

依赖关系保持扁平：

```text
Arkheide.Essential.Culture.Wpf ───────┐
Arkheide.Essential.Culture.Avalonia ──┼─> Arkheide.Essential.Culture ─> Arkheide.Essential.Culture.Generator
Arkheide.Essential.Culture.WinUI ─────┘
```

Generator 是编译期 Analyzer，不会成为应用的运行时程序集依赖。

## 2. 编写 Culture.json

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

加载时会执行以下校验：

- 根节点必须是非空 JSON 对象。
- 键必须是合法、非关键字的 C# 标识符，只允许 ASCII 字母、数字和下划线。
- 键不能包含点；例如使用 `Toolbar_Save`，不要使用 `Toolbar.Save`。
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

## 3. Generator 与资源文件约定

Core 和三个框架包都会传递 Generator。把 `Culture.json` 放在应用项目根目录后，包内的 `buildTransitive` 配置会自动：

- 将文件作为 `AdditionalFiles` 交给 Generator。
- 在构建后复制到 `$(TargetDir)\Culture.json`。
- 在发布后复制到 `$(PublishDir)\Culture.json`。
- 将 Generator 配置属性暴露给编译器。

Generator 默认使用 `Arkheide.Essential.Culture` 命名空间和 `Key` 类型：

```csharp
namespace Arkheide.Essential.Culture;

public static class Key
{
    public static string App_Title => "Key.App_Title";
    public static string Welcome_User => "Key.Welcome_User";
}
```

`Arkheide.Essential.Culture.Key` 是生成的 CLR 键容器；每个属性返回的 `Key.*` 字符串才是稳定 token，不是已经翻译的文本。

如需覆盖默认命名空间：

```xml
<PropertyGroup>
  <ArkheideEssentialCultureNamespace>MyApplication.Localization</ArkheideEssentialCultureNamespace>
</PropertyGroup>
```

命名空间覆盖只改变生成类型的 CLR 地址，不改变属性返回的 token。

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

仓库内 Demo 使用源码 `ProjectReference`，不会执行已打包 NuGet 的全部传递构建资产，因此 Demo 显式引用 Generator 并链接共享 JSON。真实应用安装 NuGet 包后只需遵循项目根目录约定。

## 4. C# 中解析文本

核心无需显式构造、初始化或释放。第一次访问 `Localizer` 时，它会从 `AppContext.BaseDirectory` 懒加载 `Culture.json`。

```csharp
using Arkheide.Essential.Culture;
using GeneratedKey = global::Arkheide.Essential.Culture.Key;

var title = Localizer.Parse(GeneratedKey.App_Title);
var welcome = Localizer.Parse(GeneratedKey.Welcome_User, "Evigila");
```

推荐为 `Arkheide.Essential.Culture.Key` 声明 `GeneratedKey` 别名，使“静态门面”和“生成键容器”在同一行代码中更容易区分。

当前文化状态集中在 `Localizer.Current`：

```csharp
var culture = Localizer.Current.Culture;
var cultures = Localizer.Current.AvailableCultures;

Localizer.Current.Changed += OnLocalizationChanged;
Localizer.Current.SetCulture("zh-CN");
```

需要自行刷新属性的对象应订阅 `Changed`，并在自身生命周期结束时退订。核心对象本身不需要清理。

解析某个键时依次尝试：

1. 当前文化的精确匹配，例如 `zh-Hans-CN`。
2. 逐级父文化，例如 `zh-Hans`、`zh`。
3. fallback 文化，默认是 `en-US`。
4. 若键不存在，返回传入的原始键或 token，便于发现资源遗漏。

带参数的 `Parse` 使用当前文化的 `CultureInfo` 执行复合格式化：

```csharp
Localizer.Parse(Arkheide.Essential.Culture.Key.Price, 19.95m);
```

## 5. XAML 中引用生成键

普通的 `Text="Key.Greeting"` 只是字符串字面量，XAML 编辑器无法把句点后的内容关联到 Generator 输出。应通过框架原生的强类型语法引用静态属性。

WPF：

```xml
<Window xmlns:culture="clr-namespace:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
</Window>
```

Avalonia：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Static culture:Key.Greeting}" />
</Window>
```

WinUI 3：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture">
  <TextBlock Text="{x:Bind culture:Key.Greeting, Mode=OneTime}" />
</Window>
```

这些表达式解析真实 CLR 类型和公开静态成员，因此编译期可以检查键是否存在。编辑器通常也能提供成员补全；Source Generator 首次生成或键集合变化后，如果 IntelliSense 尚未更新，请先构建一次项目。

生成属性在初始化时提供 token，Applicator 保存 token 后写入当前译文，并在文化变化后重新应用。WinUI 应显式使用 `Mode=OneTime`，避免绑定再次覆盖 Applicator 写入的译文。

## 6. WPF

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.Wpf
```

应用启动时创建一个 `WpfLocalizationApplicator`，在窗口显示前主动应用一次：

```csharp
private WpfLocalizationApplicator? applicator;

protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    Localizer.Current.SetCulture("en-US");
    applicator = new WpfLocalizationApplicator();
    applicator.Start(Dispatcher);

    var window = new MainWindow();
    applicator.Apply(window);
    MainWindow = window;
    window.Show();
}

protected override void OnExit(ExitEventArgs e)
{
    applicator?.Dispose();
    base.OnExit(e);
}
```

`Start` 监听后续 Loaded 事件和文化变化；`Apply` 必须在目标对象所属的 Dispatcher 线程调用，并立即发现已经创建的对象树，避免首帧显示 `Key.*`。文化变化只刷新已发现的本地化属性，不会重新扫描整棵视觉树。

WPF Applicator 识别常见显示属性，包括：

- `Window.Title`
- `TextBlock.Text`，但不会修改 `TextBoxBase.Text`
- `Content`、`Header`、`ToolTip`
- `AutomationProperties.Name`
- `DataGridColumn.Header`

退出时调用 `Stop()` 或 `Dispose()` 解除框架事件订阅。

## 7. Avalonia

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.Avalonia
```

在桌面生命周期初始化时创建并启动 Applicator：

```csharp
private AvaloniaLocalizationApplicator? applicator;

public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        Localizer.Current.SetCulture("en-US");
        applicator = new AvaloniaLocalizationApplicator();
        applicator.Start(this);

        var window = new MainWindow();
        applicator.Apply(window);
        desktop.MainWindow = window;
    }

    base.OnFrameworkInitializationCompleted();
}
```

在应用退出时调用 `Stop()` 或 `Dispose()`。新建窗口、对话框或其他独立根时，在首次显示前从 UI 线程调用 `Apply(root)`。文化变化只刷新已经发现的本地化属性。

Applicator 识别 `Window.Title`、`TextBlock.Text`、Content、Header、`TextBox.PlaceholderText`、ToolTip 和 Automation Name 等显示属性。

由 ViewModel 自行格式化的属性不包含可供 Applicator 保存的原始 token，因此应监听文化变化并触发 `PropertyChanged`：

```csharp
public string CurrentCulture =>
    Localizer.Parse(Arkheide.Essential.Culture.Key.Current_Culture, Localizer.Current.Culture);

private void Localizer_Changed(object? sender, EventArgs e) =>
    OnPropertyChanged(nameof(CurrentCulture));
```

## 8. WinUI 3

安装：

```powershell
dotnet add package Arkheide.Essential.Culture.WinUI
```

创建一个应用级 Applicator，并在窗口激活前调用 `Attach`：

```csharp
private readonly WinUILocalizationApplicator applicator = new();
private Window? window;

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    window = new MainWindow(applicator);
    applicator.Attach(window);
    window.Activate();
}
```

`Attach(window)` 必须在窗口所属 UI 线程、`Activate()` 之前调用。Applicator 会先发现当前已有的 token；窗口首次进入非 `Deactivated` 状态时，它会在 `x:Bind` 写入 token 后再发现一次，然后立即退订 `Activated`。后续文化变化会通过窗口自己的 `DispatcherQueue` 合并调度，并且只刷新已经登记的属性，不会重新扫描整棵视觉树。窗口关闭时会自动解除跟踪；应用最终退出时调用 `Dispose()`。

对于 `ContentDialog`、Popup 内容或其他独立根，必须在目标所属的 UI 线程、首次显示前主动调用：

```csharp
var dialog = new ContentDialog
{
    Title = Arkheide.Essential.Culture.Key.App_Title,
    Content = Arkheide.Essential.Culture.Key.Greeting,
    CloseButtonText = Arkheide.Essential.Culture.Key.Action_Close,
    XamlRoot = Root.XamlRoot,
};

applicator.Apply(dialog);
await dialog.ShowAsync();
```

Applicator 识别 Text、Content、ContentDialog 按钮文本、Placeholder、ToolTip 和 Automation Name 等依赖属性。

## 9. 发布检查

发布前确认：

- 输出目录和发布目录中存在 `Culture.json`。
- Generator 只作为 Analyzer 参与编译，没有成为运行时程序集依赖。
- JSON 中每个键具有相同文化集合和相同格式占位符编号。
- fallback 文化存在于全部键中。
- WPF 窗口在 `Show()` 前执行过 `Apply(window)`。
- Avalonia 根视图在首次呈现前执行过 `Apply(root)`。
- WinUI 窗口在 `Activate()` 前执行过 `Attach(window)`，独立对话框在目标 UI 线程、显示前执行过 `Apply(dialog)`。
- Applicator 在应用退出时执行过 `Stop()`、`Detach(...)` 或 `Dispose()`。

## 10. 常见问题

### Generator 报告找不到 Culture.json

默认 `auto` 模式在没有文件时静默跳过；`true` 模式要求恰好存在一份文件。检查文件名、位置和 `AdditionalFiles`，并确保项目中只有一个名为 `Culture.json` 的生成输入。

### 编译成功但启动时找不到文件

默认约定会复制项目根目录的 `Culture.json`。如果关闭了 `ArkheideEssentialCultureAutoInclude` 或把文件放在其他目录，需要自行配置输出复制规则。

### 首次显示 Key.*

必须在根对象首次显示前主动调用对应的 `Apply` 或 `Attach`。仅调用 `Start` 负责后续监听，不替代首次扫描。

### 切换文化后自定义 MVVM 属性不刷新

Applicator 只能重放它管理的 XAML token。由 ViewModel 调用 `Localizer.Parse(...)` 产生的属性应订阅 `Localizer.Current.Changed`，再触发相应的 `PropertyChanged`。

### 为什么未知键直接显示 token

这是刻意的诊断行为，能让开发和测试阶段立即发现资源遗漏。

### XAML 中输入 Key. 没有补全

确认使用的是 `{x:Static culture:Key.}`（WPF/Avalonia）或 `{x:Bind culture:Key., Mode=OneTime}`（WinUI 3），而不是普通字符串 `Text="Key."`。WPF 使用 `xmlns:culture="clr-namespace:Arkheide.Essential.Culture"`，Avalonia/WinUI 使用 `xmlns:culture="using:Arkheide.Essential.Culture"`。首次生成或修改键后先构建项目，让设计时构建读取最新的 Generator 输出。
