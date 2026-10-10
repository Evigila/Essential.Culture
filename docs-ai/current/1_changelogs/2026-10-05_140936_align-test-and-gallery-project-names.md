# Align test and Gallery project names

## Scope and mappings

User-authorized naming migration only; no platform API, localization behavior or NuGet identity changes.

| Previous project | Current project |
| --- | --- |
| Essential.Culture.Test | Tests.Essential.Culture |
| Essential.Culture.Generator.Test | Tests.Essential.Culture.Generator |
| Essential.Culture.WinUI.Test | Tests.Essential.Culture.WinUI |
| Essential.Culture.Blazor.Test | Tests.Essential.Culture.Blazor |
| Essential.Culture.Demo.Console | Gallery.Essential.Culture |
| Essential.Culture.Demo.Wpf | Gallery.Essential.Culture.Wpf |
| Essential.Culture.Demo.Avalonia | Gallery.Essential.Culture.Avalonia |
| Essential.Culture.Demo.WinUI3 | Gallery.Essential.Culture.WinUI |
| Essential.Culture.Demo.Blazor | Gallery.Essential.Culture.Blazor |
| BlazorOnly | Tests.Essential.Culture.Blazor.PackageConsumer |

Each project directory and csproj basename moves together. The demo solution becomes demo/Gallery.Essential.Culture.slnx; root and focused module solutions retain Essential.slnx and Essential.Culture.slnx. Active solution/release/consumer paths, launch profile and AI guide links were updated. Ten explicit AssemblyName settings preserve the original default assembly names; existing RootNamespace, XAML/public namespaces, generator namespaces, InternalsVisibleTo and six library PackageId/version settings remain unchanged.

## Verification

- Static validation: 16 maintained projects, 22 ProjectReference paths and three solutions resolve.
- Full scripts/Test-Release.ps1: exit 0; 89 tests passed; five Galleries compiled; six Culture 1.3.0 packages and the isolated only-Blazor consumer passed.
- Consumer evidence: artifacts/package-consumers/blazor-206e03a49b9c41e680bbbb21f1de30a4.
- Current directory map covers 112 maintained files and 33 directories, excluding its documented generated/human/AI-doc categories.
- Dirty changes were preserved. Native PowerShell moves verified exact within-repository source/target paths and unoccupied destinations. No overwrite, recursive delete, commit, tag, push or publication occurred.

## Boundaries and manual acceptance

Existing human README content moved intact with its project directory; its old executable/project links may still require a separately authorized documentation correction. Missing Essential ensure/common/ownership bootstrap remains unresolved under the prior user question and is not silently repaired by this naming task.

Open Essential.slnx, src/Essential.Culture/Essential.Culture.slnx and demo/Gallery.Essential.Culture.slnx; verify project display names and Gallery startup selection, then run the existing desktop and Blazor language-switching acceptance checks. No Computer Use testing was performed.
