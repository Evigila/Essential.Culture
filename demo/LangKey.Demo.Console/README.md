# LangKey Console Demo

这个示例展示不使用依赖注入时，如何直接创建 `LangKeyParser`、使用生成键、格式化文本并在运行时切换文化。

## 环境与依赖

- .NET 10 SDK
- `ArkheideSystem.LangKey`（自动传递 Generator）
- 可在 Windows、Linux 或 macOS 的 .NET 10 环境运行

示例通过 [`LangKey.Demo.Console.csproj`](LangKey.Demo.Console.csproj) 引用仓库源码。集成到自己的项目时，可替换为：

```xml
<PackageReference Include="ArkheideSystem.LangKey" Version="1.0.0" />
```

## 资源与 Generator

项目链接共享的 [`../LangKey.json`](../LangKey.json)：

```xml
<LangKeyNamespace>ArkheideSystem.LangKey.Demo.ConsoleApp.Generated</LangKeyNamespace>

<AdditionalFiles Include="..\LangKey.json" Link="LangKey.json" />
<None Include="..\LangKey.json"
      Link="LangKey.json"
      CopyToOutputDirectory="PreserveNewest" />
```

Generator 产生 `Generated.LangKey` 类；资源文件同时被复制到应用输出目录，供运行时 Parser 读取。

上面的显式 `AdditionalFiles`、复制项、`CompilerVisibleProperty` 和 Generator 项目引用只用于仓库内源码测试。真实应用安装 NuGet 后，把 `LangKey.json` 放在项目根目录即可自动生成和复制；只有需要改写生成命名空间时才需配置 `LangKeyNamespace`。

## 核心用法

[`Program.cs`](Program.cs) 从输出目录加载文档：

```csharp
var documentPath = Path.Combine(AppContext.BaseDirectory, "LangKey.json");
using var parser = new LangKeyParser(documentPath, "en-US");
```

解析和格式化：

```csharp
parser.Get(GeneratedLangKey.App_Title);
parser.Format(GeneratedLangKey.Current_Culture, parser.Current);
```

切换文化：

```csharp
parser.Current = parser.Current == "en-US" ? "zh-CN" : "en-US";
```

## 运行

从仓库根目录执行：

```powershell
dotnet run --project demo\LangKey.Demo.Console\LangKey.Demo.Console.csproj
```

菜单行为：

- `1`：清空控制台，在英语和简体中文之间切换并重新绘制。
- `2`：按照当前文化输出 Hello World。
- `3`：退出。
- 其他输入：显示本地化错误提示。
- 输入流结束：正常退出。

这个项目适合作为 Console、Worker、服务进程或自行管理 Parser 生命周期的最小参考。

Console 项目没有 XAML，因此不涉及 `x:Static` 或 `x:Bind`。输入 `GeneratedLangKey.` 时由 C# IntelliSense 直接列出 Generator 产生的键；WPF、Avalonia 和 WinUI 3 中对应的强类型 XAML 写法请参阅各自示例。
