# currentproject-architecture

This document explains the maintained directory and file tree from the repository root. Every listed node has a short responsibility description. Essential is the root solution for independent modules. Essential.Culture owns its six libraries and module version 1.3.0 under src/Essential.Culture. Demos and tests remain in their root directories. Essential projects do not depend on Flourish.

The tree omits all docs/ and docs-ai/ directories, version-control internals, IDE state, generated builds and caches (bin/, obj/, artifacts/, TestResults/, .packages/, packages/, node_modules/, x64/, x86/ and ARM64/), local agent state and archived directories. Empty directories without maintained files are omitted.

Coverage: 112 maintained files and 33 directories, plus the repository root.

```text
Essential/ — Repository root for independent modules, demos and tests.
├── .github/ — Contains repository automation configuration.
│   └── workflows/ — Defines GitHub Actions workflows.
│       └── build.yml — Builds, tests and verifies all release packages; matching version tags publish through the nuget environment.
├── demo/ — Contains runnable examples for the supported application frameworks.
│   ├── Gallery.Essential.Culture.Avalonia/ — Demonstrates localization in an Avalonia application.
│   │   ├── App.axaml — Defines application resources for the desktop demo.
│   │   ├── App.axaml.cs — Starts the desktop demo and connects its localization runtime.
│   │   ├── Gallery.Essential.Culture.Avalonia.csproj — Defines the runnable Avalonia localization demo.
│   │   ├── GreetingDialog.axaml — Defines a localized greeting dialog.
│   │   ├── GreetingDialog.axaml.cs — Initializes and handles the localized greeting dialog.
│   │   ├── MainWindow.axaml — Defines the demo window with localized controls.
│   │   ├── MainWindow.axaml.cs — Handles demo window actions and culture changes.
│   │   ├── Program.cs — Starts the demo and configures localization for its application framework.
│   │   └── README.md — Contains the existing startup and usage guide for this demo.
│   ├── Gallery.Essential.Culture.Blazor/ — Demonstrates scoped localization and culture switching in Blazor.
│   │   ├── Components/ — Contains the demo application root, routing and localized Razor UI.
│   │   │   ├── Layout/ — Contains the demo page layout.
│   │   │   │   └── MainLayout.razor — Provides the common container for demo pages.
│   │   │   ├── Pages/ — Contains routable demo pages.
│   │   │   │   └── Home.razor — Demonstrates translated text, generated keys and per-session culture selection.
│   │   │   ├── _Imports.razor — Imports Razor component namespaces and the generated localization keys.
│   │   │   ├── App.razor — Defines the HTML document, application assets and interactive routes.
│   │   │   └── Routes.razor — Routes requests to Razor pages in the demo assembly.
│   │   ├── Properties/ — Contains launch configuration for the demo application.
│   │   │   └── launchSettings.json — Configures local launch profiles and development addresses.
│   │   ├── wwwroot/ — Contains the Blazor demo static assets.
│   │   │   └── app.css — Styles the demo page and language selector.
│   │   ├── appsettings.json — Configures the Blazor demo host logging and allowed hosts.
│   │   ├── Culture.json — Contains the embedded demo translations and generator input.
│   │   ├── Gallery.Essential.Culture.Blazor.csproj — Defines the runnable Blazor localization demo.
│   │   └── Program.cs — Registers demo catalogs, scoped localization and the Blazor host.
│   ├── Gallery.Essential.Culture/ — Demonstrates localization in a console application.
│   │   ├── Gallery.Essential.Culture.csproj — Defines the runnable Console localization demo.
│   │   ├── Program.cs — Starts the demo and configures localization for its application framework.
│   │   └── README.md — Contains the existing startup and usage guide for this demo.
│   ├── Gallery.Essential.Culture.WinUI/ — Demonstrates localization in a WinUI3 application.
│   │   ├── Properties/ — Contains launch configuration for the demo application.
│   │   │   └── launchSettings.json — Configures local launch profiles and development addresses.
│   │   ├── app.manifest — Configures Windows application identity and runtime compatibility.
│   │   ├── App.xaml — Defines application resources for the desktop demo.
│   │   ├── App.xaml.cs — Starts the desktop demo and connects its localization runtime.
│   │   ├── Gallery.Essential.Culture.WinUI.csproj — Defines the runnable WinUI3 localization demo.
│   │   ├── MainWindow.xaml — Defines the demo window with localized controls.
│   │   ├── MainWindow.xaml.cs — Handles demo window actions and culture changes.
│   │   └── README.md — Contains the existing startup and usage guide for this demo.
│   ├── Gallery.Essential.Culture.Wpf/ — Demonstrates localization in a WPF application.
│   │   ├── App.xaml — Defines application resources for the desktop demo.
│   │   ├── App.xaml.cs — Starts the desktop demo and connects its localization runtime.
│   │   ├── Gallery.Essential.Culture.Wpf.csproj — Defines the runnable Wpf localization demo.
│   │   ├── GreetingDialog.xaml — Defines a localized greeting dialog.
│   │   ├── GreetingDialog.xaml.cs — Initializes and handles the localized greeting dialog.
│   │   ├── MainWindow.xaml — Defines the demo window with localized controls.
│   │   ├── MainWindow.xaml.cs — Handles demo window actions and culture changes.
│   │   └── README.md — Contains the existing startup and usage guide for this demo.
│   ├── Culture.json — Contains translations shared by the existing desktop and console demos.
│   ├── Directory.Build.props — Imports Culture module build settings for all demo projects.
│   ├── Gallery.Essential.Culture.slnx — Opens the Avalonia, WPF, WinUI3, console and Blazor demos.
│   └── README.md — Contains the existing demo overview and startup guidance.
├── scripts/ — Contains local release preparation, package inspection and user-confirmed tag publishing commands.
│   ├── Publish-Helper.ps1 — Prepares packages and optionally validates clean master, confirms and pushes the matching release tag.
│   ├── Release-Common.ps1 — Loads release settings, reads versions, runs checked commands and validates release tag ancestry.
│   ├── ReleaseSettings.psd1 — Lists the release solution, version source, ordered package IDs, dependencies, required assets and checks.
│   ├── Test-Release.ps1 — Builds and tests Release, packs every configured library and verifies the resulting package set.
│   ├── Verify-BlazorPackageConsumer.ps1 — Builds an isolated Blazor-only NuGet consumer and checks transitive Core and Generator integration.
│   └── Verify-PackageSet.ps1 — Checks exact package identities, versions, dependencies and required packaged assemblies and assets.
├── src/ — Contains independently versioned Essential modules.
│   └── Essential.Culture/ — Groups the Culture libraries, module build settings and existing module documentation.
│       ├── Essential.Culture/ — Provides framework-independent catalogs, contexts and the desktop localization facade.
│       │   ├── Properties/ — Contains core assembly metadata.
│       │   │   └── AssemblyInfo.cs — Allows regression tests to inspect core internal types.
│       │   ├── CultureName.cs — Normalizes and validates culture names.
│       │   ├── Essential.Culture.csproj — Defines the core localization package.
│       │   ├── KeyToken.cs — Creates and recognizes stable localization key tokens.
│       │   ├── KeyValidation.cs — Validates dictionary keys and culture names.
│       │   ├── LocalizationCatalog.cs — Loads and validates immutable translation catalogs.
│       │   ├── LocalizationContext.cs — Maintains an isolated language and formatting selection over a catalog.
│       │   ├── LocalizationRuntime.cs — Keeps the existing process-wide desktop facade over the catalog and context implementation.
│       │   └── Localizer.cs — Exposes the established static desktop localization API.
│       ├── Essential.Culture.Avalonia/ — Provides Avalonia localization markup support.
│       │   ├── AvaloniaLocalizeExtensionBase.cs — Updates Avalonia localized bindings when the desktop culture changes.
│       │   └── Essential.Culture.Avalonia.csproj — Defines the Avalonia localization adapter package.
│       ├── Essential.Culture.Blazor/ — Provides scoped Blazor localization services and rendering helpers.
│       │   ├── Essential.Culture.Blazor.csproj — Defines the Blazor localization package.
│       │   ├── ILocalizationService.cs — Declares scoped cultures, formatting, catalog-aware lookup, formatted TryParse and refresh notifications.
│       │   ├── LocalizationBuilder.cs — Configures immutable catalogs, supported cultures and scope initialization before service registration.
│       │   ├── LocalizationServiceCollectionExtensions.cs — Registers immutable startup options and the scoped Blazor localization service.
│       │   ├── LocalizationSession.cs — Resolves translations and formatting within one request or circuit without changing desktop state.
│       │   ├── LocalizedComponentBase.cs — Refreshes a component on localization changes and removes its subscription on disposal.
│       │   └── LocalizedText.cs — Renders a catalog key with optional arguments and fallback text.
│       ├── Essential.Culture.Generator/ — Provides the localization source generator and consumer build integration.
│       │   ├── buildTransitive/ — Contains build assets imported by projects consuming the generator package.
│       │   │   ├── Arkheide.Essential.Culture.Generator.props — Registers the analyzer and default localization build properties.
│       │   │   ├── Arkheide.Essential.Culture.Generator.targets — Prepares dictionary files and adds explicit translation inputs to compilation.
│       │   │   └── Culture.template.json — Supplies the initial dictionary template for opted-in consumers.
│       │   ├── AnalyzerReleases.Shipped.md — Records released source-generator diagnostics.
│       │   ├── AnalyzerReleases.Unshipped.md — Records pending source-generator diagnostics.
│       │   ├── CultureGenerator.cs — Generates typed dictionary keys and framework localization markup extensions.
│       │   └── Essential.Culture.Generator.csproj — Defines the source generator package and its analyzer and build assets.
│       ├── Essential.Culture.WinUI/ — Provides WinUI localization markup and window refresh support.
│       │   ├── Properties/ — Contains WinUI adapter assembly metadata.
│       │   │   └── AssemblyInfo.cs — Allows regression tests to inspect internal WinUI localization markers.
│       │   ├── Essential.Culture.WinUI.csproj — Defines the WinUI localization adapter package.
│       │   ├── IWinUILocalizationHost.cs — Declares the window registration and localization refresh contract.
│       │   ├── WinUILocalizationHost.cs — Refreshes registered WinUI windows and their localized display properties.
│       │   ├── WinUILocalizationMarker.cs — Encodes and reads localization markers emitted by WinUI markup extensions.
│       │   └── WinUILocalizeExtensionBase.cs — Provides the base for generated WinUI localization markup extensions.
│       ├── Essential.Culture.Wpf/ — Provides WPF localization markup support.
│       │   ├── Essential.Culture.Wpf.csproj — Defines the WPF localization adapter package.
│       │   └── WpfLocalizeExtensionBase.cs — Updates WPF localized bindings when the desktop culture changes.
│       ├── Directory.Build.props — Imports root metadata and owns Culture version 1.3.0 and existing packaged README settings.
│       ├── Essential.Culture.slnx — Opens all six Culture libraries and their regression test projects.
│       ├── README_zh-CN.md — Contains the existing Chinese Culture module and package guide.
│       └── README.md — Contains the existing English Culture module and package guide.
├── tests/ — Contains regression tests for the Culture module.
│   ├── Tests.Essential.Culture.Blazor/ — Checks scoped localization services and localized Razor rendering.
│   │   ├── Tests.Essential.Culture.Blazor.csproj — Defines the Blazor localization regression test project.
│   │   ├── LocalizationServiceTests.cs — Checks catalog lookup, scope isolation, fallback, formatting and language selection.
│   │   └── LocalizedTextTests.cs — Checks localized text rendering and component updates.
│   ├── Tests.Essential.Culture.Generator/ — Checks dictionary validation and generated localization code.
│   │   ├── CultureGeneratorTests.cs — Checks generated keys, framework adapters and diagnostic handling.
│   │   └── Tests.Essential.Culture.Generator.csproj — Defines the source generator regression test project.
│   ├── Tests.Essential.Culture/ — Checks the localization core and WPF and Avalonia bindings.
│   │   ├── AvaloniaLocalizeExtensionTests.cs — Checks translation and refresh behavior in Avalonia bindings.
│   │   ├── Culture.json — Contains translations used by the core and desktop binding tests.
│   │   ├── Tests.Essential.Culture.csproj — Defines the core and desktop binding regression test project.
│   │   ├── LocalizationContextTests.cs — Checks isolated contexts, catalog validation and formatting behavior.
│   │   ├── LocalizerTests.cs — Checks compatibility of the existing static desktop localization API.
│   │   ├── TestAssembly.cs — Runs tests serially to protect process-wide desktop localization state.
│   │   └── WpfLocalizeExtensionTests.cs — Checks translation and refresh behavior in WPF bindings.
│   ├── Tests.Essential.Culture.WinUI/ — Checks WinUI localization marker handling.
│   │   ├── Tests.Essential.Culture.WinUI.csproj — Defines the WinUI marker regression test project.
│   │   └── WinUILocalizationMarkerTests.cs — Checks stable and dynamic WinUI localization marker encoding.
│   ├── package-consumers/ — Contains checked-in isolated release-consumer fixtures rather than solution test projects.
│   │   └── Tests.Essential.Culture.Blazor.PackageConsumer/ — Validates a consumer whose only direct Culture package is Blazor.
│   │       ├── Culture.json — Supplies the independent consumer catalog and generated key input.
│   │       ├── Program.cs — Checks transitive Core/Generator, scoped localization and encoded rendering.
│   │       └── Tests.Essential.Culture.Blazor.PackageConsumer.csproj — Defines the isolated executable while preserving its BlazorOnly assembly identity.
│   └── Directory.Build.props — Imports Culture module build settings for all test projects.
├── .gitattributes — Sets repository text normalization rules.
├── .gitignore — Excludes build outputs, IDE state and local caches from version control.
├── AGENTS.md — Records portable AI ownership, collaboration and shared project naming rules.
├── Directory.Build.props — Defines shared package metadata, deterministic builds and the repository package output directory.
├── Essential.slnx — Opens the Culture module and tests and exposes the module and demo solution files.
├── LICENSE.txt — Contains the repository license terms.
└── publish-helper.bat — Forwards Prepare or Publish arguments to the PowerShell release helper and returns its exit code.
```
