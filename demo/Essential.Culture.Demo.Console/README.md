# Essential.Culture Console Demo

这个示例展示如何通过 `Localizer` 静态门面使用生成键、解析带参文本。

## 依赖

安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture" Version="1.1.0" />
```

Core 包会自动传递 Generator；Demo 不需要单独引用 Generator。

## 核心调用

第一次调用时，`Localizer` 会从输出目录懒加载资源文件：

```csharp
using ArkheideSystem.Essential.Culture;
using GeneratedKey = global::ArkheideSystem.Essential.Culture.Key;

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

`Localizer.Parse(token, args...)` 使用当前文化执行复合格式化。

需要响应文化变化的组件可以订阅 `Localizer.Current.Changed`。
