# Essential.Culture WinUI 3 Demo

这个示例展示WinUI 3 应用如何用 `WinUILocalizationHost` 管理窗口生命周期、用 `Localize` 编写 XAML。

## 依赖

安装：

```xml
<PackageReference Include="Arkheide.Essential.Culture.WinUI" Version="1.2.0" />
```

框架包会自动传递 Core 和 Generator。

## Host 生命周期

[`App.xaml.cs`](App.xaml.cs) 创建一个应用级 Host：

```csharp
private readonly WinUILocalizationHost localizationHost;
private Window? window;

public App()
{
    InitializeComponent();
    localizationHost = new WinUILocalizationHost();
}

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    window = new MainWindow();
    localizationHost.Attach(window);
    window.Activate();
}
```

> [!NOTE]
>
> `Attach(window)` 必须在窗口所属 UI 线程、`Activate()` 之前调用。

## XAML 强类型本地化

WinUI XAML 使用 Generator 产生的 `Localize`。 WinUI XAML 编译器要求使用公开默认构造器，键必须写成 `Key=...`：

```xml
<Window xmlns:culture="using:ArkheideSystem.Essential.Culture"
        Title="{culture:Localize Key=App_Title}">
  <TextBlock Text="{culture:Localize}"
             culture:Localize.KeyBinding="{x:Bind GreetingKey, Mode=OneWay}" />
  <CheckBox Content="{culture:Localize Key=Identity_IsGirl}"
            IsChecked="{x:Bind IsGirl, Mode=TwoWay}" />
  <Button Content="{culture:Localize Key=Action_SwitchLanguage}" />
</Window>
```

无静态 `Key` 的 `Localize` 会产生动态 marker。Host 在首次发现、`KeyBinding` 变化、参数变化和文化变化时重新解析该标记。

## 动态翻译键

WinUI 通过附加属性绑定 Token：

```xml
<TextBlock
    Text="{culture:Localize}"
    culture:Localize.KeyBinding="{x:Bind GreetingKey, Mode=OneWay}" />
```

```csharp
public string GreetingKey => IsGirl ? CKey.Greeting_Girl : CKey.Greeting_Boy;
```

复选框默认未选中。主页面和 XAML 中声明的 `ContentDialog` 都绑定 `GreetingKey`，点击“问候”时显示已经由 Host 解析的动态问候，不需要在事件处理器中调用 `Localizer.Parse`。

## 文化变化与对话框

窗口通过静态入口切换文化，并更新绑定到 `Localize.Argument0` 的原始文化名称：

```csharp
Localizer.Current.SetCulture(
    Localizer.Current.Culture == "en-US" ? "zh-CN" : "en-US"
);

CurrentCulture = Localizer.Current.Culture;
PropertyChanged?.Invoke(
    this,
    new PropertyChangedEventArgs(nameof(CurrentCulture))
);
```

`ContentDialog` 在 XAML 中使用 `KeyBinding`。按钮事件只负责显示已经声明的对话框：

```csharp
GreetingDialog.XamlRoot = Root.XamlRoot;
await GreetingDialog.ShowAsync();
```

窗口关闭时会退订自己的 `Localizer.Current.Changed` 处理器。
