; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
LANGKEY001 | LangKey | Error | Requires exactly one LangKey.json AdditionalFile.
LANGKEY002 | LangKey | Error | Reports invalid LangKey documents or generator configuration.
LANGKEY003 | LangKey | Error | Rejects keys that cannot become C# members.
