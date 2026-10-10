using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ArkheideSystem.Essential.Culture.Generator;

internal sealed class ResourceInput : IEquatable<ResourceInput>
{
    internal ResourceInput(string path, string content, bool module, string moduleId, string deploymentPath)
    {
        Path = path;
        Content = content;
        IsModule = module;
        ModuleId = string.IsNullOrEmpty(moduleId) ? System.IO.Path.GetFileNameWithoutExtension(path) : moduleId;
        DeploymentPath = (string.IsNullOrEmpty(deploymentPath) ? System.IO.Path.GetFileName(path) : deploymentPath).Replace('\\', '/');
    }

    internal string Path { get; }
    internal string Content { get; }
    internal bool IsModule { get; }
    internal string ModuleId { get; }
    internal string DeploymentPath { get; }

    public bool Equals(ResourceInput? other) => other is not null
        && Path == other.Path && Content == other.Content && IsModule == other.IsModule
        && ModuleId == other.ModuleId && DeploymentPath == other.DeploymentPath;
    public override bool Equals(object? obj) => Equals(obj as ResourceInput);
    public override int GetHashCode() => Path.GetHashCode() ^ Content.GetHashCode() ^ ModuleId.GetHashCode() ^ DeploymentPath.GetHashCode();
}

internal sealed class ResourceShape : IEquatable<ResourceShape>
{
    internal ResourceShape(ResourceInput input, string[] keys, bool valid)
    {
        Path = input.Path;
        IsModule = input.IsModule;
        ModuleId = input.ModuleId;
        DeploymentPath = input.DeploymentPath;
        Keys = keys;
        IsValid = valid;
    }

    internal string Path { get; }
    internal bool IsModule { get; }
    internal string ModuleId { get; }
    internal string DeploymentPath { get; }
    internal string[] Keys { get; }
    internal bool IsValid { get; }

    public bool Equals(ResourceShape? other) => other is not null && Path == other.Path
        && IsModule == other.IsModule && ModuleId == other.ModuleId && DeploymentPath == other.DeploymentPath
        && IsValid == other.IsValid && Keys.SequenceEqual(other.Keys, StringComparer.Ordinal);
    public override bool Equals(object? obj) => Equals(obj as ResourceShape);
    public override int GetHashCode()
    {
        var hash = Path.GetHashCode() ^ ModuleId.GetHashCode() ^ DeploymentPath.GetHashCode();
        foreach (var key in Keys) hash = unchecked(hash * 31 + key.GetHashCode());
        return hash ^ IsValid.GetHashCode() ^ IsModule.GetHashCode();
    }
}

internal sealed class ParsedResource
{
    internal ParsedResource(ResourceInput input, string[] keys, KeyOccurrence[] occurrences, ResourceIssue[] issues, bool valid)
    {
        Input = input;
        Text = SourceText.From(input.Content);
        Shape = new ResourceShape(input, keys, valid);
        Keys = occurrences;
        Issues = issues;
    }

    internal ResourceInput Input { get; }
    internal SourceText Text { get; }
    internal ResourceShape Shape { get; }
    internal KeyOccurrence[] Keys { get; }
    internal ResourceIssue[] Issues { get; }
    internal Location Location(int offset, int length = 0)
    {
        var span = new TextSpan(Math.Min(offset, Text.Length), Math.Min(length, Text.Length - Math.Min(offset, Text.Length)));
        return Microsoft.CodeAnalysis.Location.Create(Input.Path, span, Text.Lines.GetLinePositionSpan(span));
    }
}

internal sealed class KeyOccurrence(string key, int offset, int length)
{
    internal string Key { get; } = key;
    internal int Offset { get; } = offset;
    internal int Length { get; } = length;
}

internal sealed class ResourceIssue(bool invalidKey, string message, int offset, int length = 0)
{
    internal bool InvalidKey { get; } = invalidKey;
    internal string Message { get; } = message;
    internal int Offset { get; } = offset;
    internal int Length { get; } = length;
}

// The analyzer targets netstandard2.0, so validation must not depend on the consumer's JSON runtime.
internal sealed class ResourceParser
{
    private readonly ResourceInput input;
    private readonly string fallback;
    private readonly CancellationToken cancellation;
    private readonly Dictionary<string, string> cultureNames = new(StringComparer.Ordinal);
    private readonly List<KeyOccurrence> keys = [];
    private readonly List<ResourceIssue> issues = [];
    private int position;

    private ResourceParser(ResourceInput input, string fallback, CancellationToken cancellation)
    {
        this.input = input;
        this.fallback = fallback;
        this.cancellation = cancellation;
    }

    internal static ParsedResource Parse(ResourceInput input, string fallback, CancellationToken cancellation)
    {
        var parser = new ResourceParser(input, fallback, cancellation);
        var valid = true;
        try { parser.ReadCatalog(); }
        catch (FormatException error)
        {
            valid = false;
            parser.issues.Add(new ResourceIssue(false, error.Message, parser.position));
        }
        return new ParsedResource(input,
            parser.keys.Select(key => key.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray(),
            parser.keys.ToArray(), parser.issues.ToArray(), valid);
    }

    internal static string NormalizeCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture)) throw new FormatException("a culture cannot be empty");
        var value = culture.Trim().Replace('_', '-');
        if (value.Any(character => !char.IsLetterOrDigit(character) && character != '-') || value.Split('-').Any(string.IsNullOrEmpty))
            throw new FormatException($"contains invalid culture '{culture}'");
        try
        {
            var canonical = CultureInfo.GetCultureInfo(value).Name;
            if (canonical.Length != 0) return canonical;
        }
        catch (CultureNotFoundException) { }
        var parts = value.Split('-');
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            parts[index] = index == 0 ? part.ToLowerInvariant()
                : part.Length == 4 && part.All(char.IsLetter) ? char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()
                : (part.Length == 2 && part.All(char.IsLetter)) || (part.Length == 3 && part.All(char.IsDigit)) ? part.ToUpperInvariant()
                : part.ToLowerInvariant();
        }
        return string.Join("-", parts);
    }

    private char Current => position < input.Content.Length ? input.Content[position] : '\0';

    private void ReadCatalog()
    {
        cancellation.ThrowIfCancellationRequested();
        if (Current == '\uFEFF') position++;
        Whitespace();
        Expect('{');
        Whitespace();
        if (Consume('}')) Fail("does not contain any translation keys");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string>? expectedCultures = null;
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            Whitespace();
            var keyOffset = position;
            var key = ReadString();
            var keyLength = position - keyOffset;
            if (!seen.Add(key)) Fail($"contains duplicate key '{key}'");
            Whitespace();
            Expect(':');
            Whitespace();
            var translations = ReadTranslations(key);
            if (!translations.TryGetValue(fallback, out var expectedFormat))
                Fail($"key '{key}' does not define fallback culture '{fallback}'");
            var cultures = new HashSet<string>(translations.Keys, StringComparer.OrdinalIgnoreCase);
            if (expectedCultures is not null && !expectedCultures.SetEquals(cultures))
                Fail($"key '{key}' does not define the same culture set as the other keys");
            expectedCultures ??= cultures;
            foreach (var pair in translations)
                if (!expectedFormat!.SequenceEqual(pair.Value))
                    Fail($"key '{key}' does not preserve fallback format placeholders for culture '{pair.Key}'");
            if (CultureGenerator.IsValidKey(key)) keys.Add(new KeyOccurrence(key, keyOffset, keyLength));
            else issues.Add(new ResourceIssue(true, key, keyOffset, keyLength));
            Whitespace();
            if (Consume('}')) break;
            Expect(',');
        }
        Whitespace();
        if (position != input.Content.Length) Fail("contains unexpected content after the root object");
        if (keys.Count == 0 && issues.Count == 0) Fail("does not contain any valid keys");
    }

    private Dictionary<string, int[]> ReadTranslations(string key)
    {
        Expect('{');
        Whitespace();
        if (Consume('}')) Fail($"key '{key}' does not contain any translations");
        var translations = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            Whitespace();
            var rawCulture = ReadString();
            if (!cultureNames.TryGetValue(rawCulture, out var culture))
                cultureNames.Add(rawCulture, culture = NormalizeCulture(rawCulture));
            Whitespace();
            Expect(':');
            Whitespace();
            if (Current != '"') Fail($"key '{key}' contains a non-string translation for culture '{culture}'");
            var value = ReadString();
            if (string.IsNullOrWhiteSpace(value)) Fail($"key '{key}' contains an empty translation for culture '{culture}'");
            int[] format;
            try { format = FormatIndexes(value, cancellation); }
            catch (FormatException) { Fail($"key '{key}' contains an invalid composite format for culture '{culture}'"); throw; }
            if (translations.ContainsKey(culture)) Fail($"key '{key}' contains duplicate normalized culture '{culture}'");
            translations.Add(culture, format);
            Whitespace();
            if (Consume('}')) return translations;
            Expect(',');
        }
    }

    private string ReadString()
    {
        Expect('"');
        var value = new StringBuilder();
        while (position < input.Content.Length)
        {
            if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested();
            var character = input.Content[position++];
            if (character == '"')
            {
                var result = value.ToString();
                for (var index = 0; index < result.Length; index++)
                {
                    if ((index & 1023) == 0) cancellation.ThrowIfCancellationRequested();
                    if (char.IsHighSurrogate(result[index]))
                    {
                        if (index + 1 >= result.Length || !char.IsLowSurrogate(result[++index])) Fail("contains an invalid unicode surrogate");
                    }
                    else if (char.IsLowSurrogate(result[index])) Fail("contains an invalid unicode surrogate");
                }
                return result;
            }
            if (character < ' ') Fail("contains an unescaped control character");
            if (character != '\\') { value.Append(character); continue; }
            if (position >= input.Content.Length) Fail("contains an incomplete string escape");
            var escaped = input.Content[position++];
            switch (escaped)
            {
                case '"': case '\\': case '/': value.Append(escaped); break;
                case 'b': value.Append('\b'); break;
                case 'f': value.Append('\f'); break;
                case 'n': value.Append('\n'); break;
                case 'r': value.Append('\r'); break;
                case 't': value.Append('\t'); break;
                case 'u':
                    var code = 0;
                    for (var index = 0; index < 4; index++)
                    {
                        if (position >= input.Content.Length) Fail("contains an incomplete unicode escape");
                        var digit = input.Content[position++];
                        code = code * 16 + (digit is >= '0' and <= '9' ? digit - '0'
                            : digit is >= 'a' and <= 'f' ? digit - 'a' + 10
                            : digit is >= 'A' and <= 'F' ? digit - 'A' + 10
                            : throw new FormatException("contains an invalid unicode escape"));
                    }
                    value.Append((char)code);
                    break;
                default: Fail("contains an unsupported string escape"); break;
            }
        }
        Fail("contains an unterminated string");
        return string.Empty;
    }

    // Match composite-format syntax while retaining only the sorted placeholder indexes.
    private static int[] FormatIndexes(string value, CancellationToken cancellation)
    {
        var indexes = new List<int>();
        for (var position = 0; position < value.Length;)
        {
            if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested();
            var brace = value[position++];
            if (brace is not ('{' or '}')) continue;
            if (position < value.Length && value[position] == brace) { position++; continue; }
            if (brace == '}' || position >= value.Length || value[position] is not (>= '0' and <= '9')) throw new FormatException();
            var index = 0;
            while (position < value.Length && value[position] is >= '0' and <= '9')
            {
                var digit = value[position++] - '0';
                if (index > (int.MaxValue - digit) / 10) throw new FormatException();
                index = index * 10 + digit;
            }
            while (position < value.Length && value[position] == ' ') { if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested(); position++; }
            if (position < value.Length && value[position] == ',')
            {
                position++;
                while (position < value.Length && value[position] == ' ') { if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested(); position++; }
                if (position < value.Length && value[position] == '-') position++;
                if (position >= value.Length || value[position] is not (>= '0' and <= '9')) throw new FormatException();
                while (position < value.Length && value[position] is >= '0' and <= '9') { if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested(); position++; }
                while (position < value.Length && value[position] == ' ') { if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested(); position++; }
            }
            if (position < value.Length && value[position] == ':')
            {
                position++;
                while (position < value.Length && value[position] != '}')
                {
                    if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested();
                    if (value[position++] == '{') throw new FormatException();
                }
            }
            if (position >= value.Length || value[position++] != '}') throw new FormatException();
            indexes.Add(index);
        }
        indexes.Sort();
        return indexes.ToArray();
    }

    private void Whitespace() { while (Current is ' ' or '\t' or '\r' or '\n') { if ((position & 1023) == 0) cancellation.ThrowIfCancellationRequested(); position++; } }
    private bool Consume(char character) { if (Current != character) return false; position++; return true; }
    private void Expect(char character) { if (!Consume(character)) Fail($"expected '{character}'"); }
    private void Fail(string message) => throw new FormatException(message);
}
