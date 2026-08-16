using System.Globalization;
using System.Text.RegularExpressions;

namespace Arkheide.Essential.Culture;

internal static partial class KeyValidation
{
    private static readonly HashSet<string> CSharpKeywords =
    [
        "abstract",
        "as",
        "base",
        "bool",
        "break",
        "byte",
        "case",
        "catch",
        "char",
        "checked",
        "class",
        "const",
        "continue",
        "decimal",
        "default",
        "delegate",
        "do",
        "double",
        "else",
        "enum",
        "event",
        "explicit",
        "extern",
        "false",
        "finally",
        "fixed",
        "float",
        "for",
        "foreach",
        "goto",
        "if",
        "implicit",
        "in",
        "int",
        "interface",
        "internal",
        "is",
        "lock",
        "long",
        "namespace",
        "new",
        "null",
        "object",
        "operator",
        "out",
        "override",
        "params",
        "private",
        "protected",
        "public",
        "readonly",
        "ref",
        "return",
        "sbyte",
        "sealed",
        "short",
        "sizeof",
        "stackalloc",
        "static",
        "string",
        "struct",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "uint",
        "ulong",
        "unchecked",
        "unsafe",
        "ushort",
        "using",
        "virtual",
        "void",
        "volatile",
        "while",
    ];

    public static bool IsValidKey(string key) =>
        KeyPattern().IsMatch(key) && !CSharpKeywords.Contains(key);

    public static void ValidateKey(string key, string parameterName)
    {
        if (!IsValidKey(key))
        {
            throw new ArgumentException("Identifier not allowed.", parameterName);
        }
    }

    public static string NormalizeCulture(string culture, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("A culture cannot be empty.", parameterName);
        }

        var candidate = culture.Trim().Replace('_', '-');
        if (
            candidate.Any(character => !char.IsLetterOrDigit(character) && character != '-')
            || candidate.Split('-').Any(string.IsNullOrEmpty)
        )
        {
            throw new ArgumentException(
                "A culture must be separated by '-' or '_'.",
                parameterName
            );
        }

        try
        {
            var canonical = CultureInfo.GetCultureInfo(candidate).Name;
            return canonical.Length == 0 ? CanonicalizeCustomCulture(candidate) : canonical;
        }
        catch (CultureNotFoundException)
        {
            return CanonicalizeCustomCulture(candidate);
        }
    }

    private static string CanonicalizeCustomCulture(string culture)
    {
        var subtags = culture.Split('-');
        for (var index = 0; index < subtags.Length; index++)
        {
            var subtag = subtags[index];
            subtags[index] = index switch
            {
                0 => subtag.ToLowerInvariant(),
                _ when subtag.Length == 4 && subtag.All(char.IsLetter) => char.ToUpperInvariant(
                    subtag[0]
                ) + subtag[1..].ToLowerInvariant(),
                _ when (subtag.Length == 2 && subtag.All(char.IsLetter))
                        || (subtag.Length == 3 && subtag.All(char.IsDigit)) =>
                    subtag.ToUpperInvariant(),
                _ => subtag.ToLowerInvariant(),
            };
        }

        return string.Join('-', subtags);
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyPattern();
}
