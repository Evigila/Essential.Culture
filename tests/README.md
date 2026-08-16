# Arkheide.Essential.Culture 测试

本目录验证 Arkheide.Essential.Culture 的核心解析、Generator，以及 WPF/Avalonia Applicator 契约。

| 项目 | 范围 |
| --- | --- |
| [Arkheide.Essential.Culture.Generator.Test](Arkheide.Essential.Culture.Generator.Test/README.md) | `Culture.json` 发现、默认 `Arkheide.Essential.Culture.Key` 输出、配置覆盖与 `AEC001`–`AEC004` 诊断 |
| [Arkheide.Essential.Culture.Test](Arkheide.Essential.Culture.Test/README.md) | `Localizer` 懒加载与解析、token、文化切换及 WPF/Avalonia 适配行为 |

从仓库根目录运行：

```powershell
dotnet test Arkheide.Essential.Culture.slnx -c Release
```

WinUI 3 的窗口生命周期、`DispatcherQueue` 和 XAML UI 线程行为不能由当前普通 xUnit 项目可靠模拟，因此保留在 [WinUI 3 Demo](../demo/Arkheide.Essential.Culture.Demo.WinUI3/README.md) 的实机测试清单中。
