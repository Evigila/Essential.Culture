# LangKey WPF + DI Demo

这个示例展示 WPF、Microsoft Generic Host、依赖注入和外部文化源的组合方式。

## 环境与依赖

- Windows
- .NET 10 SDK
- WPF
- `ArkheideSystem.LangKey.Wpf`（自动传递 WPF Runtime、DI、Core 和 Generator）
- `Microsoft.Extensions.Hosting` 10.0.8

项目文件：[`LangKey.Demo.Wpf.DependencyInjection.csproj`](LangKey.Demo.Wpf.DependencyInjection.csproj)。

真实应用只需安装一个 LangKey 包：

```xml
<PackageReference Include="ArkheideSystem.LangKey.Wpf" Version="1.0.0" />
```

Demo 中额外的 Generator `ProjectReference` 只用于直接验证仓库源码，并不代表 NuGet 使用步骤。

## 服务注册

[`App.xaml.cs`](App.xaml.cs) 创建 Host 并注册：

```csharp
var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton(this);
builder.Services.AddSingleton<DemoCultureSource>();
builder.Services.AddLangKeyWpf<App, DemoCultureSource>("LangKey.json");
builder.Services.AddSingleton<MainWindow>();
```

注册顺序很重要：具体文化源必须先注册，再调用泛型 `AddLangKeyWpf`。

`AddLangKeyWpf` 会统一注册：

- Singleton `ILangKeyParser`
- 指向同一实例的 `ILangKeyResolver`
- `LangKeyWpfApplicator`
- 管理 Applicator Start/Stop 的 `IHostedService`

相对资源路径基于 `AppContext.BaseDirectory` 解析。

## 启动与首帧

```csharp
host = builder.Build();
await host.StartAsync();

var window = host.Services.GetRequiredService<MainWindow>();
host.Services.GetRequiredService<ILangKeyWpfApplicator>().Apply(window);
window.Show();
```

Host 必须先启动，Applicator 才开始监听。窗口仍要在 `Show()` 前主动 `Apply`，确保首次呈现已经完成翻译。

退出时调用 `StopAsync` 并释放 Host，容器会同时释放 Parser 及其文化源事件订阅。

## 文化同步

共享的 [`DemoCultureSource`](../Shared/DemoCultureSource.cs) 实现 `ILangKeyCultureSource`。窗口注入该类型并调用：

```csharp
cultureSource.Toggle();
```

文化源先更新 `CurrentCulture`，再触发 `Changed`；Parser 跟随事件更新，WPF Applicator 随后刷新窗口。

XAML token 和格式化方式分别见 [`MainWindow.xaml`](MainWindow.xaml) 与 [`MainWindow.xaml.cs`](MainWindow.xaml.cs)。

## XAML 强类型键

项目通过 `LangKeyNamespace` 指定 Generator 输出命名空间：

```xml
<LangKeyNamespace>ArkheideSystem.LangKey.Demo.WpfDi.Generated</LangKeyNamespace>
```

在 XAML 中映射该 CLR 命名空间，并用 `x:Static` 引用生成属性：

```xml
<Window
  xmlns:langKey="clr-namespace:ArkheideSystem.LangKey.Demo.WpfDi.Generated"
  Title="{x:Static langKey:LangKey.App_Title}">
  <TextBlock Text="{x:Static langKey:LangKey.Greeting}" />
  <Button Content="{x:Static langKey:LangKey.Action_SwitchLanguage}" />
</Window>
```

输入 `langKey:LangKey.` 时，XAML 编辑器可以按生成类的静态成员提供键名补全和编译期检查。新增或修改翻译键后，如补全未及时刷新，请先构建一次项目以更新 XAML 设计时生成信息。

`x:Static` 得到的值仍是 `LangKey.*` token；WPF Applicator 继续负责保存 token、解析当前文化，并在语言变化时重新应用译文。DI 只改变 Parser 和 Applicator 的创建及生命周期，不改变 XAML 的写法。

## 运行

```powershell
dotnet run --project demo\LangKey.Demo.Wpf.DependencyInjection\LangKey.Demo.Wpf.DependencyInjection.csproj
```

应验证首帧、语言切换、MessageBox 与 Host 关闭均正常。共享资源位于 [`../LangKey.json`](../LangKey.json)。
