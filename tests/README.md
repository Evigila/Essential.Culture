# Arkheide.Essential.Culture 测试

本目录验证 Arkheide.Essential.Culture 的核心解析、Generator，以及 WPF/Avalonia `Localize` Binding 契约。

| 项目 | 范围 |
| --- | --- |
| [Arkheide.Essential.Culture.Generator.Test](Arkheide.Essential.Culture.Generator.Test/Arkheide.Essential.Culture.Generator.Test.csproj) | `Culture.json` 发现、`CultureKey`/`Key`/`Localize` 输出、XAML 框架选择与 `AEC001`–`AEC005` 诊断 |
| [Arkheide.Essential.Culture.Test](Arkheide.Essential.Culture.Test/Arkheide.Essential.Culture.Test.csproj) | `Localizer` 参数化解析、文化切换，以及 WPF/Avalonia `Localize` 的参数与刷新行为 |

从仓库根目录运行：

```powershell
dotnet test Arkheide.Essential.Culture.slnx -c Release
```

WinUI 3 的首次本地化依赖真实 `Window.Activated`、MarkupExtension、参数附加属性、`DispatcherQueue` 和依赖属性存储。普通非 UI xUnit 测试即使伪造一个状态机，也无法验证这些关键条件，因此没有添加只能覆盖布尔标记的假测试。WinUI 项目由 Debug/Release 构建门禁覆盖；参数变化、窗口首次激活、失活重入、重复弹窗与 GC 行为保留在 [WinUI 3 Demo](../demo/Arkheide.Essential.Culture.Demo.WinUI3/README.md) 的实机测试清单中。
