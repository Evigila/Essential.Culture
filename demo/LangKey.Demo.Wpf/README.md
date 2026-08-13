# LangKey WPF Demo

这个示例展示普通 WPF 应用如何在不使用 DI/Host 的情况下，手动管理 `LangKeyParser` 和 `LangKeyWpfApplicator`。

## 环境与依赖

- Windows
- .NET 10 SDK
- WPF
- `ArkheideSystem.LangKey.Wpf.Runtime`（自动传递 Core 和 Generator，不包含 DI）

项目文件：[`LangKey.Demo.Wpf.csproj`](LangKey.Demo.Wpf.csproj)。

集成到自己的无 DI WPF 项目时只需：

```xml
<PackageReference Include="ArkheideSystem.LangKey.Wpf.Runtime" Version="1.0.0" />
```

Demo 为验证仓库源码而使用 Runtime 与 Generator 的显式 `ProjectReference`；真实 NuGet 使用者无需单独安装 Generator。

## 启动生命周期

[`App.xaml.cs`](App.xaml.cs) 按以下顺序初始化：

```csharp
var path = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
parser = new LangKeyParser(path, "en-US");
applicator = new LangKeyWpfApplicator(parser);

applicator.Start(Dispatcher);

var window = new MainWindow(parser);
applicator.Apply(window);
window.Show();
```

这里有两个不同职责：

- `Start` 订阅文化变化，并自动处理之后加载的 WPF 控件。
- `Apply(window)` 立即本地化已经创建的窗口树，必须在 `Show()` 前执行，避免首帧出现 `LangKey.*`。

应用退出时先释放 Applicator，再释放 Parser。

## XAML token

[`MainWindow.xaml`](MainWindow.xaml) 可以直接在常见 WPF 显示属性中写 token：

```xml
<Window Title="LangKey.App_Title">
  <TextBlock Text="LangKey.Greeting" />
  <Button Content="LangKey.Action_SwitchLanguage" />
</Window>
```

Applicator 会保存原 token，解析当前文化的文本，并在文化变化后重新应用。

需要参数的文本仍在 [`MainWindow.xaml.cs`](MainWindow.xaml.cs) 中显式格式化：

```csharp
CultureText.Text = parser.Format(GeneratedLangKey.Current_Culture, parser.Current);
```

语言按钮直接设置 `parser.Current`；问候按钮使用生成键解析 MessageBox 的标题和正文。

## 运行

```powershell
dotnet run --project demo\LangKey.Demo.Wpf\LangKey.Demo.Wpf.csproj
```

启动后应看到 1280×720 居中窗口。验证：

1. 首帧直接显示英文，不出现 `LangKey.*`。
2. 切换中文后，标题、正文和按钮同步刷新。
3. 问候 MessageBox 使用当前文化。

共享资源位于 [`../LangKey.json`](../LangKey.json)。
