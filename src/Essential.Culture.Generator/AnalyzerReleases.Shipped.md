; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
AEC001 | Arkheide.Essential.Culture | Error | Requires exactly one Culture.json AdditionalFile.
AEC002 | Arkheide.Essential.Culture | Error | Reports invalid culture documents or generator configuration.
AEC003 | Arkheide.Essential.Culture | Error | Rejects keys that cannot become C# members.
AEC004 | Arkheide.Essential.Culture | Error | Reports invalid ArkheideEssentialCultureGeneratorEnabled values.

## Release 1.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
AEC005 | Arkheide.Essential.Culture | Error | Reports invalid or ambiguous XAML framework selection.
