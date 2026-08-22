# NuGet 示例项目

本目录中的四个 Demo 都直接消费 NuGet.org 已发布的 `1.0.0` 包，不引用仓库内的 `src` 项目。这样可以验证真实用户获得的包依赖、Generator 传递、`buildTransitive` 配置与 XAML 编译体验。

| 示例 | 直接引用的包 | 说明 |
|---|---|---|
| [Console](Arkheide.Essential.Culture.Demo.Console/README.md) | `Arkheide.Essential.Culture` | 静态 `Localizer`、文化切换与历史消息刷新 |
| [WPF](Arkheide.Essential.Culture.Demo.Wpf/README.md) | `Arkheide.Essential.Culture.Wpf` | 强类型 `Localize Key=...` 与动态参数 |
| [Avalonia](Arkheide.Essential.Culture.Demo.Avalonia/README.md) | `Arkheide.Essential.Culture.Avalonia` | `MultiBinding`、文化切换与对话框 |
| [WinUI 3](Arkheide.Essential.Culture.Demo.WinUI3/README.md) | `Arkheide.Essential.Culture.WinUI` | Host 生命周期、附加参数与 ContentDialog |

Core 和三个框架包都会自动传递 Generator，因此 Demo 不单独安装 `Arkheide.Essential.Culture.Generator`。四个项目显式链接同一份 [`Culture.json`](Culture.json)，用于保持示例文案一致。

构建全部示例：

```powershell
dotnet restore demo/Arkheide.Essential.Culture.Demo.slnx
dotnet build demo/Arkheide.Essential.Culture.Demo.slnx -c Release --no-restore
```

Demo 固定引用最新的已发布版本。未来发布新版本时，应先由产品测试验证源码并完成 NuGet 发布，再通过普通 `master` 提交更新这里的 PackageReference；仅更新 Demo 不会触发 NuGet 发布。
