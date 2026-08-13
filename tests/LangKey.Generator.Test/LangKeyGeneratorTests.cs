using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace ArkheideSystem.LangKey.Generator.Test;

public sealed class LangKeyGeneratorTests
{
    [Fact]
    public void Auto_mode_without_document_is_a_no_op()
    {
        var result = RunGenerator();

        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Fact]
    public void Enabled_mode_requires_a_document()
    {
        var result = RunGenerator(enabled: "true");

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("LANGKEY001", diagnostic.Id);
    }

    [Fact]
    public void Disabled_mode_ignores_documents()
    {
        var result = RunGenerator(
            enabled: "false",
            documents: [Document("C:\\project\\LangKey.json", "not json")]
        );

        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Fact]
    public void Auto_mode_generates_tokens_when_document_exists()
    {
        var result = RunGenerator(
            targetNamespace: "Demo.Generated",
            documents:
            [
                Document(
                    "C:\\project\\LangKey.json",
                    """
                    {
                      "Greeting": {
                        "en-US": "Hello"
                      }
                    }
                    """
                ),
            ]
        );

        Assert.Empty(result.Diagnostics);
        var source = Assert.Single(result.Results.Single().GeneratedSources).SourceText.ToString();
        Assert.Contains("namespace Demo.Generated;", source, StringComparison.Ordinal);
        Assert.Contains("public static string Greeting => \"LangKey.Greeting\";", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Auto_mode_rejects_multiple_documents()
    {
        var result = RunGenerator(
            documents:
            [
                Document("C:\\one\\LangKey.json", "{}"),
                Document("C:\\two\\LangKey.json", "{}"),
            ]
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("LANGKEY001", diagnostic.Id);
    }

    [Fact]
    public void Invalid_mode_is_reported()
    {
        var result = RunGenerator(enabled: "sometimes");

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("LANGKEY004", diagnostic.Id);
    }

    private static GeneratorDriverRunResult RunGenerator(
        string? enabled = null,
        string? targetNamespace = null,
        AdditionalText[]? documents = null
    )
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("internal sealed class Input;");
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (enabled is not null)
        {
            options["build_property.LangKeyGeneratorEnabled"] = enabled;
        }

        if (targetNamespace is not null)
        {
            options["build_property.LangKeyNamespace"] = targetNamespace;
        }

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new LangKeyGenerator().AsSourceGenerator()],
            documents ?? [],
            (CSharpParseOptions)syntaxTree.Options,
            new TestAnalyzerConfigOptionsProvider(options)
        );

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }

    private static AdditionalText Document(string path, string content) =>
        new TestAdditionalText(path, content);

    private sealed class TestAdditionalText(string path, string content) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            SourceText.From(content);
    }

    private sealed class TestAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalValues
    ) : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty = new TestAnalyzerConfigOptions(
            new Dictionary<string, string>()
        );

        public override AnalyzerConfigOptions GlobalOptions { get; } =
            new TestAnalyzerConfigOptions(globalValues);

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
    }

    private sealed class TestAnalyzerConfigOptions(
        IReadOnlyDictionary<string, string> values
    ) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) =>
            values.TryGetValue(key, out value!);
    }
}
