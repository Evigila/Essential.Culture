# Essential.Culture WPF Demo

这个示例展示 WPF 应用如何使用生成的 `Localize` 标记扩展，在运行期间切换文化并刷新译文。

## 依赖

安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Wpf" Version="1.2.0" />
```

框架包会自动传递 Core 和 Generator。

## XAML 强类型本地化

Generator 默认在 `ArkheideSystem.Essential.Culture` 中产生 `CultureKey`、`Key` 和 `Localize`。

```xml
<Window xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture"
        Title="{culture:Localize Key=App_Title}">
  <TextBlock Text="{culture:Localize KeyBinding={Binding GreetingKey}}" />
  <CheckBox Content="{culture:Localize Key=Identity_IsGirl}"
            IsChecked="{Binding IsGirl}" />
  <Button Content="{culture:Localize Key=Action_SwitchLanguage}" />
</Window>
```

`GreetingKey` 在 `Greeting_Boy` 与 `Greeting_Girl` 之间切换。`Localize` 返回 WPF `MultiBinding`，因此翻译键或文化变化时都会重新解析。

## 动态翻译键

复选框默认未选中，对应男孩问候：

```csharp
public string GreetingKey => IsGirl ? CKey.Greeting_Girl : CKey.Greeting_Boy;
```

主窗口和弹窗都直接绑定这个 Key：

```xml
<TextBlock Text="{culture:Localize KeyBinding={Binding GreetingKey}}" />
```

点击“问候”后，Demo 创建一个极简 WPF 弹窗并把 `GreetingKey` 作为数据传入；最终译文仍由 `KeyBinding` 从 `Culture.json` 解析。

## 参数化文本

Demo 的 `Current_Culture` 包含 `{0}`，参数直接绑定到窗口属性：

```xml
<TextBlock Text="{culture:Localize Key=Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

窗口只在文化变化后更新原始 `CurrentCulture` 属性并触发 `PropertyChanged`。
