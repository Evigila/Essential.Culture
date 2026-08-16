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

WinUI 3 的首次本地化依赖真实 `Window.Activated`、生成的 `x:Bind` 回调顺序、`DispatcherQueue` 和依赖属性存储。普通非 UI xUnit 测试即使伪造一个状态机，也无法验证这些关键条件，因此没有添加只能覆盖布尔标记的假测试。WinUI 项目由 Debug/Release 构建门禁覆盖；窗口首次激活、失活重入、重复弹窗与 GC 行为保留在 [WinUI 3 Demo](../demo/Arkheide.Essential.Culture.Demo.WinUI3/README.md) 的实机测试清单中。
