# Culture release audit and pending publication

## Verified analysis and decisions

The user authorized Essential.Culture's six synchronized packages (Generator/runtime/Wpf/Avalonia/WinUI/Blazor) at 1.3.0, then Flourish's six Core/Blazor packages at 1.1.0. NuGet.org still exposes five Essential families through 1.2.0; Essential.Blazor and the six Flourish IDs return 404. The user confirmed that the Flourish.Blazor umbrella must also depend on Culture.Blazor integration. Framework-only installation remains independent of Design/Culture. Runtime services still require explicit registration.

Required publication order is Essential six, Flourish Core, Abstract, Culture.Blazor extension, Framework, Design and umbrella. The bridge cannot be consumed before its current Core/Abstract dependencies exist. Flourish WPF and Culture.WPF are excluded from packing/publishing.

Essential generator/runtime key validation now rejects names conflicting with generated Key/CultureKey types or CLR fields, four internal C# keywords and trailing line breaks. Generator tests now compile generated consumers. Essential full Release preparation passed: 106 tests, all five Galleries, six packages, isolated transitive Blazor-only consumer. Generator template is included in required-asset validation. Missing Essential AI documentation scaffolding was restored after explicit user confirmation; no human README was modified.

Release preparation scripts now isolate bin/obj through --artifacts-path. Flourish automatic sibling Essential feeds are removed; candidate-package tests accept an explicit EssentialPackageDirectory. The umbrella source/reference and exact release manifest now include the bridge. Package-consumer fixtures test umbrella-only transitive runtime/generator, keyed three-language rendering and four design/registration configurations. Flourish's revised pipeline has not yet been run after these edits. Gallery still uses its bridge ProjectReference; migration to PackageReference and removal of its redundant direct Generator remain pending after a fresh bridge package exists.

## Current boundary

The user changed the publication plan again: configure NuGet CLI credentials later, then have the agent execute pushing/publication. The current request asks only which environment prerequisites are missing. The machine has .NET SDK 10.0.400 and dotnet nuget; an additional nuget.exe or GitHub CLI is unnecessary for API-key push. No NuGet.org API key is configured in process/user environment or the user NuGet configuration. A valid account/key scoped for new packages and versions in Arkheide.Essential.Culture* and Arkheide.Flourish.* is required. Never send the key through chat or record it in source/logs.

No Git commit/tag/push or public NuGet publication occurred. Final Flourish package/consumer/build checks, public-source verification, package-reference migration and publication remain work for the continuation after credentials are configured. Follow the project commit-consent rule before a release commit. Existing Trusted Publishing helpers also enforce clean master/origin equality/tag confirmation. Do not claim local artifacts are a public release.
