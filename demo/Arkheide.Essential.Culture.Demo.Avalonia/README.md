# Arkheide.Essential.Culture Avalonia Demo

这个示例展示 Avalonia 应用如何只通过强类型 `Localize` 标记扩展完成无参数与参数化翻译，并即时切换文化。

## 依赖

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Avalonia" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 为验证源码，使用项目引用并显式链接共享 [`../Culture.json`](../Culture.json)。

## 应用初始化

[`App.axaml.cs`](App.axaml.cs) 不需要本地化服务的启动、扫描或释放逻辑，按普通 Avalonia 应用创建窗口即可：

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow();
    }

    base.OnFrameworkInitializationCompleted();
}
```

## XAML 强类型本地化

[`MainWindow.axaml`](MainWindow.axaml) 使用 `using:Arkheide.Essential.Culture` 下生成的 `Localize`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture"
        Title="{culture:Localize App_Title}">
  <TextBlock Text="{culture:Localize Greeting,
                    Arg0={Binding ProductName}}" />
  <Button Content="{culture:Localize Action_SwitchLanguage}" />
</Window>
```

位置键由 Generator 产生的 `CultureKey` 枚举约束。`Localize` 组合文化信号与参数 Binding，因此任一输入变化都会重新执行参数化解析。无参数翻译也使用同一个 API，所有翻译都遵循同一套 XAML 标准。

参数较多时可以使用对象元素形式的 `Arguments` 内容集合：

```xml
<TextBlock.Text>
  <culture:Localize Key="Greeting">
    <Binding Path="ProductName" />
  </culture:Localize>
</TextBlock.Text>
```

## 参数化文本

共享 `Culture.json` 的 `Greeting` 与 `Current_Culture` 均包含 `{0}`。Demo 直接绑定 `ProductName` 和 `CurrentCulture`：

```xml
<TextBlock Text="{culture:Localize Greeting,
                  Arg0={Binding ProductName}}" />
<TextBlock Text="{culture:Localize Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

窗口在文化变化时更新 `CurrentCulture` 并触发 `PropertyChanged`，格式化由 `Localize` 负责。示例不引入额外 MVVM 基础设施；语言按钮直接调用 `Localizer.Current.SetCulture(...)`。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Avalonia\Arkheide.Essential.Culture.Demo.Avalonia.csproj
```

请验证：

- 主窗口标题、正文和按钮在首次显示时已经本地化。
- 问候文本始终显示 `Arkheide`，不会残留 `{0}`。
- 切换语言后 XAML 文本和参数化当前文化文本同时刷新。
- 问候对话框的标题、正文和关闭按钮使用当前语言。
- 新打开的问候对话框不需要额外的本地化初始化。
