# Arkheide.Essential.Culture Console Demo

这个示例展示如何通过 `Localizer` 静态门面使用生成键、解析带参文本，并在进程运行期间即时重新本地化已有消息。

## 项目结构

项目引用：

- `Arkheide.Essential.Culture` 源码项目。
- `Arkheide.Essential.Culture.Generator` 源码项目，并将其作为 Analyzer 使用。

项目还链接共享的 [`../Culture.json`](../Culture.json)：

```xml
<AdditionalFiles Include="..\Culture.json" Link="Culture.json" />
<None Include="..\Culture.json"
      Link="Culture.json"
      CopyToOutputDirectory="PreserveNewest" />
```

这些显式项目引用和文件项只用于仓库源码验证。真实应用安装 `Arkheide.Essential.Culture` 后，把 `Culture.json` 放在应用项目根目录即可自动生成和复制。

## 核心调用

第一次调用时，`Localizer` 会从输出目录懒加载资源文件：

```csharp
using Arkheide.Essential.Culture;
using GeneratedKey = global::Arkheide.Essential.Culture.Key;

var title = Localizer.Parse(GeneratedKey.App_Title);
var cultureText = Localizer.Parse(
    GeneratedKey.Current_Culture,
    Localizer.Current.Culture
);
```

切换文化：

```csharp
var next = Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US";
Localizer.Current.SetCulture(next);
```

`Localizer.Parse(token)` 解析无参数文本；`Localizer.Parse(token, args...)` 使用当前文化执行复合格式化。需要响应文化变化的组件可以订阅 `Localizer.Current.Changed`。核心不需要显式初始化，也不需要在进程退出时清理。

示例的消息历史保存稳定 token 和格式参数，而不是保存已经翻译的字符串：

```csharp
var messageHistory = new List<LocalizedMessage>();
messageHistory.Add(new LocalizedMessage(GeneratedKey.Greeting, ["Arkheide"]));

Localizer.Current.SetCulture("zh-CN");
foreach (var message in messageHistory)
{
    Console.WriteLine(Localizer.Parse(message.Token, message.Arguments));
}
```

控制台无法改写已经输出的普通文本，因此切换文化时不会清屏，而是用新文化重新解析历史 token，并追加一份新的本地化视图。这既保留了旧输出，也能直接观察已有消息随文化变化得到的新译文。

## 运行

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Console\Arkheide.Essential.Culture.Demo.Console.csproj
```

菜单操作：

1. 在英文和中文之间切换，并追加重新本地化后的消息历史。
2. 输出当前文化下的 Hello World，同时把问候语的稳定 token 加入消息历史。
3. 退出程序。

Console 项目没有 XAML。在 C# 中输入 `Arkheide.Essential.Culture.Key.` 时，IntelliSense 会列出 Generator 产生的键属性；属性值只是稳定 token，实际文本始终由 `Localizer.Parse` 返回。
