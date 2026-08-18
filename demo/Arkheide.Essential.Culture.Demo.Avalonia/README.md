# Arkheide.Essential.Culture Avalonia Demo

这个示例展示 Avalonia 应用如何只通过强类型 `Localize` 标记扩展完成无参数与参数化翻译，并即时切换文化。

## 依赖

安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Avalonia" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。

## 应用初始化

按普通 Avalonia 应用创建窗口即可：

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

使用 `using:Arkheide.Essential.Culture` 下生成的 `Localize`：

```xml
<Window xmlns:culture="using:Arkheide.Essential.Culture"
        Title="{culture:Localize Key=App_Title}">
  <TextBlock Text="{culture:Localize Key=Greeting,
                    Arg0={Binding ProductName}}" />
  <Button Content="{culture:Localize Key=Action_SwitchLanguage}" />
</Window>
```

`Localize` 自动处理文化与参数 Binding，因此任一输入变化都会重新执行参数化解析。无参数翻译也使用同一个 API。

参数较多时可以使用对象元素形式的 `Arguments` 内容集合：

```xml
<TextBlock.Text>
  <culture:Localize Key="Greeting">
    <Binding Path="ProductName" />
  </culture:Localize>
</TextBlock.Text>
```

## 参数化文本

`Culture.json` 的 `Greeting` 与 `Current_Culture` 均包含 `{0}`。Demo 直接绑定 `ProductName` 和 `CurrentCulture`：

```xml
<TextBlock Text="{culture:Localize Key=Greeting,
                  Arg0={Binding ProductName}}" />
<TextBlock Text="{culture:Localize Key=Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

窗口在文化变化时更新 `CurrentCulture` 并触发 `PropertyChanged`。
