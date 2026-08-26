# 示例项目

| 示例 | 引用的包 | 说明 |
|---|---|---|
| [Console](Essential.Culture.Demo.Console/README.md) | `Arkheide.Essential.Culture` | 静态 `Localizer`、文化切换与历史消息刷新 |
| [WPF](Essential.Culture.Demo.Wpf/README.md) | `Arkheide.Essential.Culture.Wpf` | 强类型 `Localize Key=...` 与动态参数 |
| [Avalonia](Essential.Culture.Demo.Avalonia/README.md) | `Arkheide.Essential.Culture.Avalonia` | `MultiBinding`、文化切换与对话框 |
| [WinUI 3](Essential.Culture.Demo.WinUI3/README.md) | `Arkheide.Essential.Culture.WinUI` | Host 生命周期、附加参数与 ContentDialog |

Core 和三个框架包都会自动传递 Generator，项目链接同一份 [`Culture.json`](Culture.json)，保持示例文案一致。

构建全部示例：

```powershell
dotnet restore demo/Essential.Culture.Demo.slnx
dotnet build demo/Essential.Culture.Demo.slnx -c Release --no-restore
```
