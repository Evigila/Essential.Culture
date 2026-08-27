# Essential.Culture

[英文](README.md) | [简体中文](README_zh-CN.md)

`Essential.Culture` is a JSON-based localization component for .NET.

It manages localization keys in a single `Culture.json` file, generates strongly typed keys through a Source Generator, and provides runtime culture switching for WPF, Avalonia, and WinUI 3.

## Features

- `Culture.json` is the conventional JSON file name and is detected automatically.
- Source generation provides the `CultureKey` enum and a unified `Localize` XAML API for strongly typed keys and compile-time validation.
- `KeyBinding` allows collection items or runtime state to select localization keys dynamically.
- `Localizer.Parse(...)` and `Localizer.TryParse(...)` resolve localization keys and support format arguments.
- `Localizer.Current` provides runtime culture management.

## Quick start

Install the appropriate package from NuGet.

For WPF:

```powershell
dotnet add package Arkheide.Essential.Culture.Wpf
```

For Avalonia:

```powershell
dotnet add package Arkheide.Essential.Culture.Avalonia
```

For WinUI 3:

```powershell
dotnet add package Arkheide.Essential.Culture.WinUI
```

For other project types:

```powershell
dotnet add package Arkheide.Essential.Culture
```

The package automatically creates `Culture.json` with default content. You can then define your own keys and translations:

```json
{
  "Greeting": {
    "en-US": "Hello, World!",
    "zh-CN": "你好，世界！"
  },
  "Welcome_User": {
    "en-US": "Hello, {0}!",
    "zh-CN": "你好，{0}！"
  }
}
```

## Start using it

> [!NOTE]
> If IntelliSense does not show the generated types yet, build the project once.

WPF uses `Localize`, and format arguments can use bindings directly:

```xml
<Window xmlns:culture="clr-namespace:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
  <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
</Window>
```

Avalonia uses the same API with different namespace syntax:

```xml
<Window xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User, Arg0={Binding UserName}}" />
  <TextBlock Text="{culture:Localize KeyBinding={Binding CurrentTextKey}}" />
</Window>
```

Use the strongly typed `Key=` property so the editor can suggest keys from the generated `CultureKey` enum. When the key comes from the DataContext or runtime state, use `KeyBinding=`.

WinUI 3 exposes dynamic arguments through attached properties on the same `Localize` type:

```xml
<Window xmlns:culture="using:ArkheideSystem.Essential.Culture">
  <TextBlock Text="{culture:Localize Key=Greeting}" />
  <TextBlock Text="{culture:Localize Key=Welcome_User}"
             culture:Localize.Argument0="{x:Bind ViewModel.UserName, Mode=OneWay}" />
  <TextBlock Text="{culture:Localize}"
             culture:Localize.KeyBinding="{x:Bind ViewModel.CurrentTextKey, Mode=OneWay}" />
</Window>
```

`Key` and `KeyBinding` cannot be used together. `Localize` resolves the target text again whenever the static key, dynamic key, format arguments, or current culture changes. Applications do not need to subscribe to culture events themselves.

## Source generation

The Generator automatically discovers `Culture.json` and copies it to the output directory during build and publish. By default, it generates the `CultureKey` enum, `Key` tokens, and the `Localize` static entry point used by UI projects:

```csharp
namespace ArkheideSystem.Essential.Culture;

public enum CultureKey
{
    Greeting,
}

public static class Key
{
    public static string Greeting => "Key.Greeting";
}
```

```csharp
using ArkheideSystem.Essential.Culture;
using GeneratedKey = global::ArkheideSystem.Essential.Culture.Key;

// Print the translated text
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));

// Change the current culture
Localizer.Current.SetCulture("zh-CN");

// Print the translated text again without managing culture-change events
Console.WriteLine(Localizer.Parse(GeneratedKey.Greeting));
```

> [!NOTE]
> The default culture and fallback culture are both `en-US`.

To override the namespace of generated types, add the following project setting:

```xml
<PropertyGroup>
  <EssentialCultureNamespace>MyApplication.Localization</EssentialCultureNamespace>
</PropertyGroup>
```

Automatic creation never overwrites an existing `Culture.json`. After it is created, edit the file directly to add your own localization keys and translations. To disable automatic creation, add this project setting:

```xml
<PropertyGroup>
  <EssentialCultureAutoCreate>false</EssentialCultureAutoCreate>
</PropertyGroup>
```

## Packages

| Package | Purpose |
| --- | --- |
| `Arkheide.Essential.Culture` | Culture resolution and the static `Localizer` entry point |
| `Arkheide.Essential.Culture.Generator` | Generates strongly typed keys from `Culture.json`; normally included transitively and does not need to be installed separately |
| `Arkheide.Essential.Culture.Wpf` | Strongly typed WPF `Localize` XAML binding |
| `Arkheide.Essential.Culture.Avalonia` | Strongly typed Avalonia `Localize` XAML binding |
| `Arkheide.Essential.Culture.WinUI` | Strongly typed WinUI 3 `Localize` support and window refresh infrastructure |

```powershell
dotnet add package Arkheide.Essential.Culture
dotnet add package Arkheide.Essential.Culture.Wpf
dotnet add package Arkheide.Essential.Culture.Avalonia
dotnet add package Arkheide.Essential.Culture.WinUI
```

## AI assistance

> [!IMPORTANT]
> This library was developed with assistance from AI Agent (ChatGPT Codex).
> All forms of AI assistance are welcome for maintenance and development, **but human review is required before submission**.

## Documentation and examples

- [Complete usage guide](docs/usage-guide.md)
- [Demo overview](demo/README.md)
- [Console Demo](demo/Essential.Culture.Demo.Console/README.md)
- [WPF Demo](demo/Essential.Culture.Demo.Wpf/README.md)
- [Avalonia Demo](demo/Essential.Culture.Demo.Avalonia/README.md)
- [WinUI 3 Demo](demo/Essential.Culture.Demo.WinUI3/README.md)

## License

Licensed under the [MIT License](LICENSE.txt).
