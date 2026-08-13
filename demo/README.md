# LangKey Demo

本目录包含五个彼此独立的示例项目，共用一份 [`LangKey.json`](LangKey.json) 和少量框架无关辅助代码。

| 示例 | 重点 |
| --- | --- |
| [Console](LangKey.Demo.Console/README.md) | 直接创建 Parser、格式化、交互式文化切换 |
| [普通 WPF](LangKey.Demo.Wpf/README.md) | `.Wpf.Runtime`：不使用 DI，手动管理 Parser 与 Applicator |
| [WPF + DI](LangKey.Demo.Wpf.DependencyInjection/README.md) | 默认 `.Wpf` 包：Generic Host、文化源与 `AddLangKeyWpf` |
| [Avalonia + DI](LangKey.Demo.Avalonia.DependencyInjection/README.md) | 默认 `.Avalonia` 包：Host、Applicator 与 `x:Static` 强类型键 |
| [WinUI 3 + DI](LangKey.Demo.WinUI3.DependencyInjection/README.md) | 默认 `.WinUI` 包：DI、窗口 Attach 与 `x:Bind` 强类型键 |

统一构建：

```powershell
dotnet build LangKey.Demo.slnx
```

Demo 使用项目引用以便同时验证仓库源码。由于 `ProjectReference` 不会导入已打包 NuGet 的 `buildTransitive` 资产，示例仍显式引用 Generator、链接共享 JSON，并声明 `CompilerVisibleProperty`。这属于仓库内部测试配置。

真实应用只需引用一个面向场景的 NuGet 包：Core、DI 或 WPF/Avalonia/WinUI 默认入口；Generator 会自动传递，项目根目录的 `LangKey.json` 也会自动参与生成并复制到构建和发布目录。

四个 UI 示例均使用 Generator 生成的 CLR 属性：WPF/Avalonia 采用 `x:Static`，WinUI 3 采用 `x:Bind Mode=OneTime`。这使 XAML 编辑器能够按成员补全键名，并让不存在的键在编译阶段失败；Console 没有 XAML，直接在 C# 中使用同一生成类。
