# Essential.Culture WPF Demo

这个示例展示 WPF 应用如何使用生成的 `Localize` 标记扩展，在运行期间切换文化并刷新译文。

## 依赖

安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Wpf" Version="1.1.0" />
```

框架包会自动传递 Core 和 Generator。

## XAML 强类型本地化

Generator 默认在 `ArkheideSystem.Essential.Culture` 中产生 `CultureKey`、`Key` 和 `Localize`。

```xml
<Window xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture"
        Title="{culture:Localize Key=App_Title}">
  <TextBlock Text="{culture:Localize Key=Greeting,
                    Arg0={Binding ProductName}}" />
  <Button Content="{culture:Localize Key=Action_SwitchLanguage}" />
</Window>
```

`Localize` 返回 WPF `MultiBinding`，因此 `ProductName` 变化和文化变化都会保留参数并重新解析。

## 参数化文本

Demo 的 `Greeting` 和 `Current_Culture` 都包含 `{0}`，参数直接绑定到窗口属性：

```xml
<TextBlock Text="{culture:Localize Key=Greeting,
                  Arg0={Binding ProductName}}" />
<TextBlock Text="{culture:Localize Key=Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

窗口只在文化变化后更新原始 `CurrentCulture` 属性并触发 `PropertyChanged`。
