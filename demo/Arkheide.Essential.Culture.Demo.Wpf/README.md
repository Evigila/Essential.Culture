# Arkheide.Essential.Culture WPF Demo

这个示例展示 WPF 应用如何统一使用生成的 `Localize` 标记扩展，在运行期间切换文化并刷新无参数与参数化译文。

## 依赖

真实应用只需安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.Wpf" Version="1.0.0" />
```

框架包会自动传递 Core 和 Generator。仓库内 Demo 为验证源码，使用项目引用并显式链接共享 [`../Culture.json`](../Culture.json)。

## 启动方式

`Localize` 本身会监听文化和参数变化，不需要创建或释放应用级本地化服务。Demo 直接通过 [`App.xaml`](App.xaml) 启动主窗口：

```xml
<Application ... StartupUri="MainWindow.xaml">
  <Application.Resources />
</Application>
```

## XAML 强类型本地化

Generator 默认在 `Arkheide.Essential.Culture` 中产生 `CultureKey`、`Key` 和 `Localize`。[`MainWindow.xaml`](MainWindow.xaml) 只映射这一个 CLR 命名空间：

```xml
<Window xmlns:culture="clr-namespace:Arkheide.Essential.Culture"
        Title="{culture:Localize App_Title}">
  <TextBlock Text="{culture:Localize Greeting,
                    Arg0={Binding ProductName}}" />
  <Button Content="{culture:Localize Action_SwitchLanguage}" />
</Window>
```

位置键由生成的 `CultureKey` 枚举约束。`Localize` 返回 WPF `MultiBinding`，因此 `ProductName` 变化和文化变化都会保留参数并重新解析。首次生成或修改键后，如果 XAML 补全没有更新，请先构建一次项目。

## 参数化文本

Demo 的 `Greeting` 和 `Current_Culture` 都包含 `{0}`，参数直接绑定到窗口属性：

```xml
<TextBlock Text="{culture:Localize Greeting,
                  Arg0={Binding ProductName}}" />
<TextBlock Text="{culture:Localize Current_Culture,
                  Arg0={Binding CurrentCulture}}" />
```

窗口只在文化变化后更新原始 `CurrentCulture` 属性并触发 `PropertyChanged`；格式化由 `Localize` 完成。语言按钮调用 `Localizer.Current.SetCulture(...)`。MessageBox 不是 XAML 属性，因此其正文继续使用参数化 `Localizer.Parse(...)`。

## 运行与验证

```powershell
dotnet run --project demo\Arkheide.Essential.Culture.Demo.Wpf\Arkheide.Essential.Culture.Demo.Wpf.csproj
```

请验证：

- 首帧直接显示译文。
- 标题、描述、按钮和窗口标题随语言切换。
- 问候文本始终显示 `Arkheide`，不会残留 `{0}`。
- 当前文化文本正确格式化，并在切换后保留参数。
- MessageBox 标题和参数化内容使用当前语言。
- 重复打开 MessageBox 后仍可继续动态切换语言。
