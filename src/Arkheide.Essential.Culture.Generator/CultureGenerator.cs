using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Arkheide.Essential.Culture.Generator;

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
        "Arkheide Essential Culture key is invalid",
        "Key '{0}' must be a non-keyword C# identifier containing only ASCII letters, digits, and underscores; replace dots with underscores",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidConfiguration = new(
        "AEC004",
        "Arkheide Essential Culture generator configuration is invalid",
        "ArkheideEssentialCultureGeneratorEnabled must be 'auto', 'true', or 'false'; found '{0}'",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    private static readonly DiagnosticDescriptor InvalidXamlFramework = new(
        "AEC005",
        "Arkheide Essential Culture XAML framework configuration is invalid",
        "{0}",
        "Arkheide.Essential.Culture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var documents = context
            .AdditionalTextsProvider.Where(static file =>
                string.Equals(
                    Path.GetFileName(file.Path),
                    "Culture.json",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Select(
                static (file, cancellationToken) => new LocalizationDocument(
                    file.GetText(cancellationToken)?.ToString() ?? string.Empty
                )
            )
            .Collect()
            .WithTrackingName("CultureDocuments");
        var configuration = context.AnalyzerConfigOptionsProvider.Select(
            static (options, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return new GeneratorConfiguration(
                    GetGlobalOption(options, "build_property.ArkheideEssentialCultureNamespace"),
                    GetGlobalOption(
                        options,
                        "build_property.ArkheideEssentialCultureGeneratorEnabled"
                    ),
                    GetGlobalOption(
                        options,
                        "build_property.ArkheideEssentialCultureXamlFramework"
                    )
                );
            }
        ).WithTrackingName("CultureConfiguration");
        var availableFrameworks = context.CompilationProvider.Select(
            static (compilation, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var frameworks = XamlFramework.None;
                if (
                    compilation.GetTypeByMetadataName(
                        "Arkheide.Essential.Culture.Wpf.WpfLocalizeExtensionBase"
                    ) is not null
                )
                {
                    frameworks |= XamlFramework.Wpf;
                }

                if (
                    compilation.GetTypeByMetadataName(
                        "Arkheide.Essential.Culture.Avalonia.AvaloniaLocalizeExtensionBase"
                    ) is not null
                )
                {
                    frameworks |= XamlFramework.Avalonia;
                }

                if (
                    compilation.GetTypeByMetadataName(
                        "Arkheide.Essential.Culture.WinUI.WinUILocalizeExtensionBase"
                    ) is not null
                )
                {
                    frameworks |= XamlFramework.WinUI;
                }

                return frameworks;
            }
        ).WithTrackingName("CultureXamlFrameworkReferences");

        var inputs = documents
            .Combine(configuration)
            .Combine(availableFrameworks)
            .WithTrackingName("CultureGenerationInputs");

        context.RegisterSourceOutput(
            inputs,
            static (productionContext, input) =>
                Generate(
                    productionContext,
                    input.Left.Left,
                    input.Left.Right,
                    input.Right
                )
        );
    }

    private static string GetGlobalOption(
        AnalyzerConfigOptionsProvider options,
        string propertyName
    ) =>
        options.GlobalOptions.TryGetValue(propertyName, out var configured)
            ? configured
            : string.Empty;

    private static void Generate(
        SourceProductionContext context,
        IReadOnlyList<LocalizationDocument> documents,
        GeneratorConfiguration configuration,
        XamlFramework availableFrameworks
    )
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        if (!TryGetMode(configuration.Enabled, out var mode))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(InvalidConfiguration, Location.None, configuration.Enabled)
            );
            return;
        }

        if (mode == GeneratorMode.Disabled)
        {
            return;
        }

        if (mode == GeneratorMode.Auto && documents.Count == 0)
        {
            return;
        }

        if (documents.Count != 1)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(MissingDocument, Location.None, documents.Count)
            );
            return;
        }

        if (
            !string.IsNullOrWhiteSpace(configuration.TargetNamespace)
            && !IsValidNamespace(configuration.TargetNamespace)
        )
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    InvalidDocument,
                    Location.None,
                    $"uses invalid ArkheideEssentialCultureNamespace '{configuration.TargetNamespace}'"
                )
            );
            return;
        }

        string[] keys;
        if (
            !JsonKeyReader.TryRead(
                documents[0].Content,
                context.CancellationToken,
                out keys,
                out var parseError
            )
        )
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    InvalidDocument,
                    Location.None,
                    $"contains invalid JSON: {parseError}"
                )
            );
            return;
        }

        var unique = new HashSet<string>(StringComparer.Ordinal);
        var collected = new List<string>();
        foreach (var key in keys)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (!IsValidKey(key))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidKey, Location.None, key));
                continue;
            }

            if (!unique.Add(key))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InvalidDocument,
                        Location.None,
                        $"contains duplicate key '{key}'"
                    )
                );
                continue;
            }

            collected.Add(key);
        }

        keys = [.. collected.OrderBy(key => key, StringComparer.Ordinal)];

        if (keys.Length == 0)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(InvalidDocument, Location.None, "does not contain any valid keys")
            );
            return;
        }

        if (
            !TrySelectXamlFramework(
                configuration.XamlFramework,
                availableFrameworks,
                out var xamlFramework,
                out var frameworkError
            )
        )
        {
            context.ReportDiagnostic(
                Diagnostic.Create(InvalidXamlFramework, Location.None, frameworkError)
            );
            return;
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.Append("namespace ").Append(configuration.TargetNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine(
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Arkheide.Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
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
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Arkheide.Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
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

    private static string GenerateLocalizeFacade(
        string targetNamespace,
        IReadOnlyList<string> keys,
        XamlFramework framework
    )
    {
        var baseType = framework switch
        {
            XamlFramework.Wpf =>
                "global::Arkheide.Essential.Culture.Wpf.WpfLocalizeExtensionBase",
            XamlFramework.Avalonia =>
                "global::Arkheide.Essential.Culture.Avalonia.AvaloniaLocalizeExtensionBase",
            XamlFramework.WinUI =>
                "global::Arkheide.Essential.Culture.WinUI.WinUILocalizeExtensionBase",
            _ => throw new ArgumentOutOfRangeException(nameof(framework)),
        };
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.Append("namespace ").Append(targetNamespace).AppendLine(";");
        source.AppendLine();
        source.AppendLine(
            $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Arkheide.Essential.Culture.Generator\", \"{GeneratorVersion}\")]"
        );
        source.Append("public sealed class Localize : ").AppendLine(baseType);
        source.AppendLine("{");
        source.AppendLine("    private global::" + targetNamespace + ".CultureKey key;");
        source.AppendLine();
        source.AppendLine("    public Localize()");
        source.AppendLine("    {");
        source.AppendLine("    }");
        if (framework != XamlFramework.WinUI)
        {
            source.AppendLine();
            source.AppendLine("    public Localize(global::" + targetNamespace + ".CultureKey key)");
            source.AppendLine("        : base(GetToken(key))");
            source.AppendLine("    {");
            source.AppendLine("        this.key = key;");
            source.AppendLine("    }");
        }

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
                    "Multiple XAML framework adapters are referenced; set ArkheideEssentialCultureXamlFramework to 'wpf', 'avalonia', 'winui', or 'none'";
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
                    $"ArkheideEssentialCultureXamlFramework must be 'auto', 'wpf', 'avalonia', 'winui', or 'none'; found '{configured}'";
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
            $"ArkheideEssentialCultureXamlFramework is '{configured}', but that adapter is not referenced";
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

    private static bool IsValidKey(string key) =>
        key.Length > 0
        && key[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_'
        && key.All(character =>
            character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_'
        )
        && SyntaxFacts.GetKeywordKind(key) == SyntaxKind.None;

    private static bool IsValidNamespace(string value) =>
        value
            .Split('.')
            .All(part =>
                SyntaxFacts.IsValidIdentifier(part)
                && SyntaxFacts.GetKeywordKind(part) == SyntaxKind.None
            );

    private sealed class LocalizationDocument : IEquatable<LocalizationDocument>
    {
        public LocalizationDocument(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public bool Equals(LocalizationDocument? other) =>
            other is not null && string.Equals(Content, other.Content, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as LocalizationDocument);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Content);
    }

    private sealed class GeneratorConfiguration : IEquatable<GeneratorConfiguration>
    {
        public GeneratorConfiguration(string targetNamespace, string enabled, string xamlFramework)
        {
            TargetNamespace = string.IsNullOrWhiteSpace(targetNamespace)
                ? "Arkheide.Essential.Culture"
                : targetNamespace.Trim();
            Enabled = enabled;
            XamlFramework = xamlFramework;
        }

        public string TargetNamespace { get; }

        public string Enabled { get; }

        public string XamlFramework { get; }

        public bool Equals(GeneratorConfiguration? other) =>
            other is not null
            && string.Equals(TargetNamespace, other.TargetNamespace, StringComparison.Ordinal)
            && string.Equals(Enabled, other.Enabled, StringComparison.Ordinal)
            && string.Equals(XamlFramework, other.XamlFramework, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as GeneratorConfiguration);

        public override int GetHashCode()
        {
            unchecked
            {
                return StringComparer.Ordinal.GetHashCode(TargetNamespace) * 397
                    ^ StringComparer.Ordinal.GetHashCode(Enabled) * 31
                    ^ StringComparer.Ordinal.GetHashCode(XamlFramework);
            }
        }
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

    private sealed class JsonKeyReader
    {
        private readonly string text;
        private readonly CancellationToken cancellationToken;
        private int position;

        private JsonKeyReader(string text, CancellationToken cancellationToken)
        {
            this.text = text;
            this.cancellationToken = cancellationToken;
        }

        public static bool TryRead(
            string text,
            CancellationToken cancellationToken,
            out string[] keys,
            out string error
        )
        {
            try
            {
                var reader = new JsonKeyReader(text, cancellationToken);
                var collected = new List<string>();
                reader.SkipWhitespace();
                reader.ReadObject(collected);
                reader.SkipWhitespace();
                if (!reader.IsEnd)
                {
                    reader.Fail("unexpected content after the root object");
                }

                keys = [.. collected];
                error = string.Empty;
                return true;
            }
            catch (FormatException exception)
            {
                keys = Array.Empty<string>();
                error = exception.Message;
                return false;
            }
        }

        private bool IsEnd => position >= text.Length;

        private char Current => !IsEnd ? text[position] : '\0';

        private void ReadObject(List<string>? rootKeys)
        {
            Expect('{');
            SkipWhitespace();
            if (TryConsume('}'))
            {
                return;
            }

            while (true)
            {
                CheckCancellation();
                SkipWhitespace();
                string? name = null;
                if (rootKeys is null)
                {
                    SkipString();
                }
                else
                {
                    name = ReadString();
                }
                SkipWhitespace();
                Expect(':');
                SkipWhitespace();
                if (rootKeys is not null && Current != '{')
                {
                    Fail($"key '{name}' must contain a culture-to-string object");
                }

                if (name is not null)
                {
                    rootKeys!.Add(name);
                }
                ReadValue();
                SkipWhitespace();
                if (TryConsume('}'))
                {
                    return;
                }

                Expect(',');
            }
        }

        private void ReadArray()
        {
            Expect('[');
            SkipWhitespace();
            if (TryConsume(']'))
            {
                return;
            }

            while (true)
            {
                CheckCancellation();
                ReadValue();
                SkipWhitespace();
                if (TryConsume(']'))
                {
                    return;
                }

                Expect(',');
                SkipWhitespace();
            }
        }

        private void ReadValue()
        {
            CheckCancellation();
            SkipWhitespace();
            switch (Current)
            {
                case '{':
                    ReadObject(null);
                    break;
                case '[':
                    ReadArray();
                    break;
                case '"':
                    SkipString();
                    break;
                case 't':
                    ConsumeLiteral("true");
                    break;
                case 'f':
                    ConsumeLiteral("false");
                    break;
                case 'n':
                    ConsumeLiteral("null");
                    break;
                default:
                    ReadNumber();
                    break;
            }
        }

        private string ReadString()
        {
            Expect('"');
            var result = new StringBuilder();
            while (!IsEnd)
            {
                CheckCancellation();
                var character = text[position++];
                if (character == '"')
                {
                    return result.ToString();
                }

                if (character < ' ')
                {
                    Fail("a string contains an unescaped control character");
                }

                if (character != '\\')
                {
                    result.Append(character);
                    continue;
                }

                if (IsEnd)
                {
                    Fail("a string ends after an escape character");
                }

                var escaped = text[position++];
                switch (escaped)
                {
                    case '"':
                    case '\\':
                    case '/':
                        result.Append(escaped);
                        break;
                    case 'b':
                        result.Append('\b');
                        break;
                    case 'f':
                        result.Append('\f');
                        break;
                    case 'n':
                        result.Append('\n');
                        break;
                    case 'r':
                        result.Append('\r');
                        break;
                    case 't':
                        result.Append('\t');
                        break;
                    case 'u':
                        result.Append(ReadUnicodeEscape());
                        break;
                    default:
                        Fail($"contains unsupported escape '\\{escaped}'");
                        break;
                }
            }

            Fail("contains an unterminated string");
            return string.Empty;
        }

        private void SkipString()
        {
            Expect('"');
            while (!IsEnd)
            {
                CheckCancellation();
                var character = text[position++];
                if (character == '"')
                {
                    return;
                }

                if (character < ' ')
                {
                    Fail("a string contains an unescaped control character");
                }

                if (character != '\\')
                {
                    continue;
                }

                if (IsEnd)
                {
                    Fail("a string ends after an escape character");
                }

                var escaped = text[position++];
                if (escaped == 'u')
                {
                    SkipUnicodeEscape();
                }
                else if (escaped is not ('"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't'))
                {
                    Fail($"contains unsupported escape '\\{escaped}'");
                }
            }

            Fail("contains an unterminated string");
        }

        private void SkipUnicodeEscape()
        {
            if (position + 4 > text.Length)
            {
                Fail("contains an incomplete unicode escape");
            }

            for (var index = 0; index < 4; index++)
            {
                var character = text[position++];
                if (character is not (>= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F'))
                {
                    Fail($"invalid unicode escape at position {position - 1}");
                }
            }
        }

        private char ReadUnicodeEscape()
        {
            if (position + 4 > text.Length)
            {
                Fail("contains an incomplete unicode escape");
            }

            var value = 0;
            for (var index = 0; index < 4; index++)
            {
                var character = text[position++];
                value =
                    value * 16
                    + character switch
                    {
                        >= '0' and <= '9' => character - '0',
                        >= 'a' and <= 'f' => character - 'a' + 10,
                        >= 'A' and <= 'F' => character - 'A' + 10,
                        _ => throw new FormatException(
                            $"invalid unicode escape at position {position - 1}"
                        ),
                    };
            }

            return (char)value;
        }

        private void ReadNumber()
        {
            var start = position;
            TryConsume('-');
            if (TryConsume('0'))
            {
                if (char.IsDigit(Current))
                {
                    Fail("a number contains a leading zero");
                }
            }
            else
            {
                ReadDigits(required: true);
            }

            if (TryConsume('.'))
            {
                ReadDigits(required: true);
            }

            if (TryConsume('e') || TryConsume('E'))
            {
                _ = TryConsume('+') || TryConsume('-');
                ReadDigits(required: true);
            }

            if (position == start)
            {
                Fail("expected a JSON value");
            }
        }

        private void ReadDigits(bool required)
        {
            var start = position;
            while (char.IsDigit(Current))
            {
                position++;
            }

            if (required && start == position)
            {
                Fail("expected a digit");
            }
        }

        private void ConsumeLiteral(string literal)
        {
            for (var index = 0; index < literal.Length; index++)
            {
                if (IsEnd || text[position++] != literal[index])
                {
                    Fail($"expected '{literal}'");
                }
            }
        }

        private void SkipWhitespace()
        {
            while (Current is ' ' or '\t' or '\r' or '\n')
            {
                position++;
            }
        }

        private void Expect(char expected)
        {
            if (!TryConsume(expected))
            {
                Fail($"expected '{expected}'");
            }
        }

        private bool TryConsume(char expected)
        {
            if (Current != expected)
            {
                return false;
            }

            position++;
            return true;
        }

        private void Fail(string message) =>
            throw new FormatException($"{message} at position {position}");

        private void CheckCancellation()
        {
            if ((position & 1023) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
