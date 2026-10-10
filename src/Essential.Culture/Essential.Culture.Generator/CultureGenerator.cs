using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ArkheideSystem.Essential.Culture.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class CultureGenerator : IIncrementalGenerator
{
    private static readonly string GeneratorVersion =
        typeof(CultureGenerator).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
    private static readonly DiagnosticDescriptor MissingDocument = new(
        "AEC001",
        "Culture.json was not found",
        "Exactly one AdditionalFile named Culture.json is required; found {0}",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidDocument = new(
        "AEC002",
        "Culture.json is invalid",
        "Culture.json {0}",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidKey = new(
        "AEC003",
        "Culture key is invalid",
        "Key '{0}' must be a non-keyword C# identifier containing only ASCII letters, digits, and underscores; replace dots with underscores and avoid generated names Key, CultureKey, and value__",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidConfiguration = new(
        "AEC004",
        "Culture generator configuration is invalid",
        "EssentialCultureGeneratorEnabled must be 'auto', 'true', or 'false'; found '{0}'",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidXamlFramework = new(
        "AEC005",
        "Culture XAML framework configuration is invalid",
        "{0}",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor InvalidModule = new(
        "AEC006", "Culture module configuration is invalid", "{0}",
        "Arkheide.Essential.Culture", DiagnosticSeverity.Error, isEnabledByDefault: true);
    private static readonly DiagnosticDescriptor DuplicateModuleKey = new(
        "AEC007", "Culture key is defined by multiple resources",
        "Key '{0}' is declared by both '{1}' and '{2}'",
        "Arkheide.Essential.Culture", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configuration = context.AnalyzerConfigOptionsProvider.Select(
            static (options, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return new GeneratorConfiguration(
                    GetGlobalOption(options, "build_property.EssentialCultureNamespace", "build_property.ArkheideEssentialCultureNamespace"),
                    GetGlobalOption(options, "build_property.EssentialCultureGeneratorEnabled", "build_property.ArkheideEssentialCultureGeneratorEnabled"),
                    GetGlobalOption(options, "build_property.EssentialCultureXamlFramework", "build_property.ArkheideEssentialCultureXamlFramework"),
                    GetGlobalOption(options, "build_property.EssentialCultureModulesEnabled"),
                    GetGlobalOption(options, "build_property.EssentialCultureFallbackCulture"));
            }).WithTrackingName("CultureConfiguration");

        var resources = context.AdditionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (pair, token) => ReadInput(pair.Left, pair.Right, token))
            .Where(static input => input is not null)
            .Select(static (input, _) => input!)
            .WithTrackingName("CultureResourceInputs");
        var fallback = configuration.Select(static (settings, _) => settings.FallbackCulture);
        var parsed = resources.Combine(fallback)
            .Select(static (pair, token) => ResourceParser.Parse(pair.Left, pair.Right, token))
            .WithTrackingName("CultureParsedResources");
        var documents = parsed.Select(static (resource, _) => resource.Shape)
            .WithTrackingName("CultureResourceShapes")
            .Collect().WithTrackingName("CultureDocuments");
        var availableFrameworks = context.CompilationProvider.Select(
            static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var frameworks = XamlFramework.None;
                if (compilation.GetTypeByMetadataName("ArkheideSystem.Essential.Culture.Wpf.WpfLocalizeExtensionBase") is not null)
                    frameworks |= XamlFramework.Wpf;
                if (compilation.GetTypeByMetadataName("ArkheideSystem.Essential.Culture.Avalonia.AvaloniaLocalizeExtensionBase") is not null)
                    frameworks |= XamlFramework.Avalonia;
                if (compilation.GetTypeByMetadataName("ArkheideSystem.Essential.Culture.WinUI.WinUILocalizeExtensionBase") is not null)
                    frameworks |= XamlFramework.WinUI;
                return frameworks;
            }).WithTrackingName("CultureXamlFrameworkReferences");
        var inputs = documents.Combine(configuration).Combine(availableFrameworks)
            .WithTrackingName("CultureGenerationInputs");
        context.RegisterSourceOutput(inputs, static (production, input) =>
            Generate(production, input.Left.Left, input.Left.Right, input.Right));
        // Diagnostics retain precise source locations independently of the translation-free output shape.
        context.RegisterSourceOutput(parsed.Collect().Combine(configuration).Combine(availableFrameworks),
            static (production, input) => ReportDiagnostics(production, input.Left.Left, input.Left.Right, input.Right));
    }

    private static ResourceInput? ReadInput(AdditionalText file, AnalyzerConfigOptionsProvider options, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (string.Equals(GetGlobalOption(options, "build_property.EssentialCultureGeneratorEnabled",
            "build_property.ArkheideEssentialCultureGeneratorEnabled").Trim(), "false", StringComparison.OrdinalIgnoreCase)) return null;
        var metadata = options.GetOptions(file);
        metadata.TryGetValue("build_metadata.AdditionalFiles.CultureModule", out var marked);
        var module = string.Equals(marked, "true", StringComparison.OrdinalIgnoreCase);
        var name = Path.GetFileName(file.Path);
        var discovery = string.Equals(GetGlobalOption(options, "build_property.EssentialCultureModulesEnabled").Trim(), "true", StringComparison.OrdinalIgnoreCase);
        var traditional = string.Equals(name, "Culture.json", StringComparison.OrdinalIgnoreCase);
        var discovered = discovery && name.StartsWith("Culture.", StringComparison.OrdinalIgnoreCase)
            && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(name, "Culture.options.json", StringComparison.OrdinalIgnoreCase);
        if (!module && !traditional && !discovered) return null;
        metadata.TryGetValue("build_metadata.AdditionalFiles.CultureModuleId", out var id);
        metadata.TryGetValue("build_metadata.AdditionalFiles.CultureDeploymentPath", out var path);
        return new ResourceInput(file.Path, file.GetText(token)?.ToString() ?? string.Empty,
            module || discovered && !traditional, id ?? string.Empty, path ?? string.Empty);
    }

    private static string GetGlobalOption(
        AnalyzerConfigOptionsProvider options,
        string propertyName,
        string? legacyPropertyName = null
    )
    {
        if (options.GlobalOptions.TryGetValue(propertyName, out var configured))
        {
            return configured;
        }

        return legacyPropertyName is not null
            && options.GlobalOptions.TryGetValue(legacyPropertyName, out configured)
                ? configured
                : string.Empty;
    }

    private static void Generate(
        SourceProductionContext context,
        IReadOnlyList<ResourceShape> documents,
        GeneratorConfiguration configuration,
        XamlFramework availableFrameworks)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        if (!TryGetMode(configuration.Enabled, out var mode) || mode == GeneratorMode.Disabled
            || !TryGetModules(configuration.Modules, out var discover) || !configuration.ValidFallback
            || !IsValidNamespace(configuration.TargetNamespace)) return;
        var modular = discover || documents.Any(document => document.IsModule);
        if (documents.Count == 0 || !modular && documents.Count != 1
            || documents.Any(document => !document.IsValid)) return;
        if (modular && !ValidModuleIdentities(documents)) return;
        var allKeys = documents.SelectMany(document => document.Keys).ToArray();
        if (allKeys.Distinct(StringComparer.Ordinal).Count() != allKeys.Length) return;
        var keys = allKeys.OrderBy(key => key, StringComparer.Ordinal).ToArray();
        if (keys.Length == 0 || !TrySelectXamlFramework(configuration.XamlFramework, availableFrameworks,
                out var xamlFramework, out _)) return;

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.Append("namespace ").Append(configuration.TargetNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine(
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
        );
        source.AppendLine("public enum CultureKey");
        source.AppendLine("{");
        foreach (var key in keys)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            source.Append("    ").Append(key).AppendLine(",");
        }

        source.AppendLine("}");
        source.AppendLine();
        source.AppendLine(
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
        );
        source.AppendLine("public static class Key");
        source.AppendLine("{");
        foreach (var key in keys)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            source
                .Append("    public static string ")
                .Append(key)
                .Append(" => \"Key.")
                .Append(key)
                .AppendLine("\";");
        }

        source.AppendLine("}");
        context.AddSource("Key.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        if (modular)
            context.AddSource("CultureResources.g.cs", SourceText.From(
                GenerateResources(configuration.TargetNamespace, documents, configuration.FallbackCulture), Encoding.UTF8));

        if (xamlFramework != XamlFramework.None)
        {
            context.AddSource(
                "Localize.g.cs",
                SourceText.From(
                    GenerateLocalizeFacade(configuration.TargetNamespace, keys, xamlFramework),
                    Encoding.UTF8
                )
            );
        }
    }

    private static void ReportDiagnostics(SourceProductionContext context, IReadOnlyList<ParsedResource> resources,
        GeneratorConfiguration configuration, XamlFramework frameworks)
    {
        if (!TryGetMode(configuration.Enabled, out var mode))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidConfiguration, Location.None, configuration.Enabled));
            return;
        }
        if (mode == GeneratorMode.Disabled) return;
        if (!TryGetModules(configuration.Modules, out var discovery))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidModule, Location.None, "EssentialCultureModulesEnabled must be 'true' or 'false'."));
            return;
        }
        if (mode == GeneratorMode.Auto && resources.Count == 0) return;
        var modular = discovery || resources.Any(resource => resource.Shape.IsModule);
        if (resources.Count == 0 || !modular && resources.Count != 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingDocument, Location.None, resources.Count));
            return;
        }
        if (!configuration.ValidFallback)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidModule, Location.None, "EssentialCultureFallbackCulture is invalid."));
            return;
        }
        if (!IsValidNamespace(configuration.TargetNamespace))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidDocument, Location.None,
                $"uses invalid EssentialCultureNamespace '{configuration.TargetNamespace}'"));
            return;
        }
        foreach (var resource in resources)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            foreach (var issue in resource.Issues)
                context.ReportDiagnostic(Diagnostic.Create(issue.InvalidKey ? InvalidKey : InvalidDocument,
                    resource.Location(issue.Offset, issue.Length), issue.InvalidKey ? issue.Message : $"'{resource.Input.Path}' {issue.Message}"));
        }
        if (modular)
        {
            var ids = new Dictionary<string, ParsedResource>(StringComparer.OrdinalIgnoreCase);
            var paths = new Dictionary<string, ParsedResource>(StringComparer.OrdinalIgnoreCase);
            foreach (var resource in resources.OrderBy(item => item.Input.Path, StringComparer.Ordinal))
            {
                var shape = resource.Shape;
                if (!IsValidModuleId(shape.ModuleId))
                    context.ReportDiagnostic(Diagnostic.Create(InvalidModule, resource.Location(0), $"'{shape.Path}' has invalid ModuleId '{shape.ModuleId}'."));
                else if (ids.TryGetValue(shape.ModuleId, out var firstId))
                    context.ReportDiagnostic(Diagnostic.Create(InvalidModule, resource.Location(0), [firstId.Location(0)], null,
                        $"ModuleId '{shape.ModuleId}' is used by both '{firstId.Input.Path}' and '{shape.Path}'."));
                else ids.Add(shape.ModuleId, resource);
                if (!IsSafeDeploymentPath(shape.DeploymentPath))
                    context.ReportDiagnostic(Diagnostic.Create(InvalidModule, resource.Location(0), $"'{shape.Path}' has unsafe DeploymentPath '{shape.DeploymentPath}'."));
                else if (paths.TryGetValue(shape.DeploymentPath, out var firstPath))
                    context.ReportDiagnostic(Diagnostic.Create(InvalidModule, resource.Location(0), [firstPath.Location(0)], null,
                        $"DeploymentPath '{shape.DeploymentPath}' is used by both '{firstPath.Input.Path}' and '{shape.Path}'."));
                else paths.Add(shape.DeploymentPath, resource);
            }
        }
        var seen = new Dictionary<string, (ParsedResource Resource, KeyOccurrence Key)>(StringComparer.Ordinal);
        foreach (var resource in resources.OrderBy(item => item.Input.Path, StringComparer.Ordinal))
        {
            foreach (var key in resource.Keys)
            {
                if (seen.TryGetValue(key.Key, out var first))
                    context.ReportDiagnostic(Diagnostic.Create(DuplicateModuleKey, resource.Location(key.Offset, key.Length),
                        [first.Resource.Location(first.Key.Offset, first.Key.Length)], null,
                        key.Key, first.Resource.Input.Path, resource.Input.Path));
                else seen.Add(key.Key, (resource, key));
            }
        }
        if (resources.All(resource => resource.Shape.IsValid) && !TrySelectXamlFramework(configuration.XamlFramework, frameworks, out _, out var error))
            context.ReportDiagnostic(Diagnostic.Create(InvalidXamlFramework, Location.None, error));
    }

    private static bool TryGetModules(string configured, out bool enabled)
    {
        enabled = string.Equals(configured.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        return string.IsNullOrWhiteSpace(configured) || enabled || string.Equals(configured.Trim(), "false", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidModuleId(string id) => id.Length > 0
        && id.All(character => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-' or '.');

    private static bool IsSafeDeploymentPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path != path.Trim() || path.StartsWith("/", StringComparison.Ordinal)
            || !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            || path.Any(character => character < ' ' || "<>:\"|?*;".IndexOf(character) >= 0)) return false;
        return path.Split('/').All(part => part.Length > 0 && part is not ("." or "..")
            && !part.EndsWith(".", StringComparison.Ordinal) && !part.EndsWith(" ", StringComparison.Ordinal)
            && !IsDeviceName(part));
    }

    private static bool IsDeviceName(string segment)
    {
        var name = segment.Split('.')[0].ToUpperInvariant();
        return name is "CON" or "PRN" or "AUX" or "NUL" || name.Length == 4
            && (name.StartsWith("COM", StringComparison.Ordinal) || name.StartsWith("LPT", StringComparison.Ordinal))
            && name[3] is >= '1' and <= '9';
    }

    private static bool ValidModuleIdentities(IReadOnlyList<ResourceShape> resources) =>
        resources.All(resource => IsValidModuleId(resource.ModuleId) && IsSafeDeploymentPath(resource.DeploymentPath))
        && resources.Select(resource => resource.ModuleId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == resources.Count
        && resources.Select(resource => resource.DeploymentPath).Distinct(StringComparer.OrdinalIgnoreCase).Count() == resources.Count;

    private static string GenerateResources(string targetNamespace, IReadOnlyList<ResourceShape> resources, string fallback)
    {
        var source = new StringBuilder("// <auto-generated />\n#nullable enable\nnamespace ");
        source.Append(targetNamespace).AppendLine(";");
        source.AppendLine("internal static class CultureResources");
        source.AppendLine("{");
        source.Append("    internal const string FallbackCulture = ").Append(SymbolDisplay.FormatLiteral(fallback, true)).AppendLine(";");
        source.AppendLine("    internal static global::System.Collections.Generic.IReadOnlyList<string> Files { get; } =");
        source.AppendLine("        global::System.Array.AsReadOnly(new string[]");
        source.AppendLine("        {");
        foreach (var resource in resources.OrderBy(resource => resource.DeploymentPath, StringComparer.Ordinal))
            source.Append("            ").Append(SymbolDisplay.FormatLiteral(resource.DeploymentPath, true)).AppendLine(",");
        source.AppendLine("        });");
        source.AppendLine("}");
        return source.ToString();
    }

    private static string GenerateLocalizeFacade(
        string targetNamespace,
        IReadOnlyList<string> keys,
        XamlFramework framework
    )
    {
        var baseType = framework switch
        {
            XamlFramework.Wpf =>
                "global::ArkheideSystem.Essential.Culture.Wpf.WpfLocalizeExtensionBase",
            XamlFramework.Avalonia =>
                "global::ArkheideSystem.Essential.Culture.Avalonia.AvaloniaLocalizeExtensionBase",
            XamlFramework.WinUI =>
                "global::ArkheideSystem.Essential.Culture.WinUI.WinUILocalizeExtensionBase",
            _ => throw new ArgumentOutOfRangeException(nameof(framework)),
        };
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.Append("namespace ").Append(targetNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine(
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
        );
        source.Append("public sealed class Localize : ").AppendLine(baseType);
        source.AppendLine("{");
        source.AppendLine("    private global::" + targetNamespace + ".CultureKey key;");
        source.AppendLine();
        source.AppendLine("    public Localize()");
        source.AppendLine("    {");
        source.AppendLine("    }");

        source.AppendLine();
        source.AppendLine("    public global::" + targetNamespace + ".CultureKey Key");
        source.AppendLine("    {");
        source.AppendLine("        get => key;");
        source.AppendLine("        set");
        source.AppendLine("        {");
        source.AppendLine("            key = value;");
        source.AppendLine("            Token = GetToken(value);");
        source.AppendLine("        }");
        source.AppendLine("    }");
        source.AppendLine();
        source
            .Append("    private static string GetToken(global::")
            .Append(targetNamespace)
            .AppendLine(".CultureKey key) =>");
        source.AppendLine("        key switch");
        source.AppendLine("        {");
        foreach (var key in keys)
        {
            source
                .Append("            global::")
                .Append(targetNamespace)
                .Append(".CultureKey.")
                .Append(key)
                .Append(" => global::")
                .Append(targetNamespace)
                .Append(".Key.")
                .Append(key)
                .AppendLine(",");
        }

        source.AppendLine(
            "            _ => throw new global::System.ArgumentOutOfRangeException(nameof(key), key, \"Unknown localization key.\"),"
        );
        source.AppendLine("        };");

        if (framework == XamlFramework.WinUI)
        {
            AppendWinUIAttachedPropertyForwarders(source, baseType);
        }

        source.AppendLine("}");
        return source.ToString();
    }

    private static void AppendWinUIAttachedPropertyForwarders(
        StringBuilder source,
        string baseType
    )
    {
        source.AppendLine();
        source
            .Append("    public new static readonly global::Microsoft.UI.Xaml.DependencyProperty KeyBindingProperty = ")
            .Append(baseType)
            .AppendLine(".KeyBindingProperty;");
        source
            .Append("    public new static string? GetKeyBinding(global::Microsoft.UI.Xaml.DependencyObject target) => ")
            .Append(baseType)
            .AppendLine(".GetKeyBinding(target);");
        source
            .Append("    public new static void SetKeyBinding(global::Microsoft.UI.Xaml.DependencyObject target, string? value) => ")
            .Append(baseType)
            .AppendLine(".SetKeyBinding(target, value);");

        foreach (var index in new[] { "0", "1", "2" })
        {
            source.AppendLine();
            source
                .Append("    public new static readonly global::Microsoft.UI.Xaml.DependencyProperty Argument")
                .Append(index)
                .Append("Property = ")
                .Append(baseType)
                .Append(".Argument")
                .Append(index)
                .AppendLine("Property;");
            source
                .Append("    public new static object? GetArgument")
                .Append(index)
                .Append("(global::Microsoft.UI.Xaml.DependencyObject target) => ")
                .Append(baseType)
                .Append(".GetArgument")
                .Append(index)
                .AppendLine("(target);");
            source
                .Append("    public new static void SetArgument")
                .Append(index)
                .Append("(global::Microsoft.UI.Xaml.DependencyObject target, object? value) => ")
                .Append(baseType)
                .Append(".SetArgument")
                .Append(index)
                .AppendLine("(target, value);");
        }

        source.AppendLine();
        source
            .Append("    public new static readonly global::Microsoft.UI.Xaml.DependencyProperty ArgumentsProperty = ")
            .Append(baseType)
            .AppendLine(".ArgumentsProperty;");
        source
            .Append("    public new static global::System.Collections.Generic.IList<object?>? GetArguments(global::Microsoft.UI.Xaml.DependencyObject target) => ")
            .Append(baseType)
            .AppendLine(".GetArguments(target);");
        source
            .Append("    public new static void SetArguments(global::Microsoft.UI.Xaml.DependencyObject target, global::System.Collections.Generic.IList<object?>? value) => ")
            .Append(baseType)
            .AppendLine(".SetArguments(target, value);");
    }

    private static bool TrySelectXamlFramework(
        string configured,
        XamlFramework available,
        out XamlFramework selected,
        out string error
    )
    {
        switch (configured.Trim().ToLowerInvariant())
        {
            case "":
            case "auto":
                if (available == XamlFramework.None || HasSingleFlag(available))
                {
                    selected = available;
                    error = string.Empty;
                    return true;
                }

                selected = XamlFramework.None;
                error =
                    "Multiple XAML framework adapters are referenced; set EssentialCultureXamlFramework to 'wpf', 'avalonia', 'winui', or 'none'";
                return false;
            case "none":
            case "false":
                selected = XamlFramework.None;
                error = string.Empty;
                return true;
            case "wpf":
                return SelectConfiguredFramework(
                    XamlFramework.Wpf,
                    "wpf",
                    available,
                    out selected,
                    out error
                );
            case "avalonia":
                return SelectConfiguredFramework(
                    XamlFramework.Avalonia,
                    "avalonia",
                    available,
                    out selected,
                    out error
                );
            case "winui":
                return SelectConfiguredFramework(
                    XamlFramework.WinUI,
                    "winui",
                    available,
                    out selected,
                    out error
                );
            default:
                selected = XamlFramework.None;
                error =
                    $"EssentialCultureXamlFramework must be 'auto', 'wpf', 'avalonia', 'winui', or 'none'; found '{configured}'";
                return false;
        }
    }

    private static bool SelectConfiguredFramework(
        XamlFramework requested,
        string configured,
        XamlFramework available,
        out XamlFramework selected,
        out string error
    )
    {
        if ((available & requested) != 0)
        {
            selected = requested;
            error = string.Empty;
            return true;
        }

        selected = XamlFramework.None;
        error =
            $"EssentialCultureXamlFramework is '{configured}', but that adapter is not referenced";
        return false;
    }

    private static bool HasSingleFlag(XamlFramework value) =>
        value is XamlFramework.Wpf or XamlFramework.Avalonia or XamlFramework.WinUI;

    private static bool TryGetMode(string configured, out GeneratorMode mode)
    {
        switch (configured.Trim().ToLowerInvariant())
        {
            case "":
            case "auto":
                mode = GeneratorMode.Auto;
                return true;
            case "true":
                mode = GeneratorMode.Enabled;
                return true;
            case "false":
                mode = GeneratorMode.Disabled;
                return true;
            default:
                mode = default;
                return false;
        }
    }

    internal static bool IsValidKey(string key) =>
        key.Length > 0
        && key[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_'
        && key.All(character =>
            character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_'
        )
        && SyntaxFacts.GetKeywordKind(key) == SyntaxKind.None
        && key is not ("Key" or "CultureKey" or "value__");

    private static bool IsValidNamespace(string value) =>
        value
            .Split('.')
            .All(part =>
                SyntaxFacts.IsValidIdentifier(part)
                && SyntaxFacts.GetKeywordKind(part) == SyntaxKind.None
            );

    private sealed class GeneratorConfiguration : IEquatable<GeneratorConfiguration>
    {
        internal GeneratorConfiguration(string targetNamespace, string enabled, string xamlFramework, string modules, string fallback)
        {
            TargetNamespace = string.IsNullOrWhiteSpace(targetNamespace) ? "ArkheideSystem.Essential.Culture" : targetNamespace.Trim();
            Enabled = enabled;
            XamlFramework = xamlFramework;
            Modules = modules;
            try
            {
                FallbackCulture = ResourceParser.NormalizeCulture(string.IsNullOrWhiteSpace(fallback) ? "en-US" : fallback);
                ValidFallback = true;
            }
            catch (FormatException) { FallbackCulture = fallback; }
        }
        internal string TargetNamespace { get; }
        internal string Enabled { get; }
        internal string XamlFramework { get; }
        internal string Modules { get; }
        internal string FallbackCulture { get; }
        internal bool ValidFallback { get; }
        public bool Equals(GeneratorConfiguration? other) => other is not null
            && TargetNamespace == other.TargetNamespace && Enabled == other.Enabled && XamlFramework == other.XamlFramework
            && Modules == other.Modules && FallbackCulture == other.FallbackCulture && ValidFallback == other.ValidFallback;
        public override bool Equals(object? obj) => Equals(obj as GeneratorConfiguration);
        public override int GetHashCode() => TargetNamespace.GetHashCode() ^ Enabled.GetHashCode()
            ^ XamlFramework.GetHashCode() ^ Modules.GetHashCode() ^ FallbackCulture.GetHashCode() ^ ValidFallback.GetHashCode();
    }

    private enum GeneratorMode
    {
        Auto,
        Enabled,
        Disabled,
    }

    [Flags]
    private enum XamlFramework
    {
        None = 0,
        Wpf = 1,
        Avalonia = 2,
        WinUI = 4,
    }

}
