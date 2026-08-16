using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Arkheide.Essential.Culture.Generator.Test;

public sealed class CultureGeneratorTests
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
        Assert.Equal("AEC001", diagnostic.Id);
    }

    [Fact]
    public void Disabled_mode_ignores_documents()
    {
        var result = RunGenerator(
            enabled: "false",
            documents: [Document("C:\\project\\Culture.json", "not json")]
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
                    "C:\\project\\Culture.json",
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
        var generated = Assert.Single(result.Results.Single().GeneratedSources);
        Assert.Equal("Key.g.cs", generated.HintName);
        var source = generated.SourceText.ToString();
        Assert.Contains("namespace Demo.Generated;", source, StringComparison.Ordinal);
        Assert.Contains("public static class Key", source, StringComparison.Ordinal);
        Assert.Contains(
            "public static string Greeting => \"Key.Greeting\";",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Auto_mode_uses_product_namespace_by_default()
    {
        var result = RunGenerator(
            documents:
            [
                Document(
                    "C:\\project\\Culture.json",
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
        Assert.Contains("namespace Arkheide.Essential.Culture;", source, StringComparison.Ordinal);
        Assert.Contains("public static class Key", source, StringComparison.Ordinal);
        Assert.Contains(
            "public static string Greeting => \"Key.Greeting\";",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Auto_mode_rejects_multiple_documents()
    {
        var result = RunGenerator(
            documents:
            [
                Document("C:\\one\\Culture.json", "{}"),
                Document("C:\\two\\Culture.json", "{}"),
            ]
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("AEC001", diagnostic.Id);
    }

    [Fact]
    public void Invalid_mode_is_reported()
    {
        var result = RunGenerator(enabled: "sometimes");

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("AEC004", diagnostic.Id);
    }

    [Fact]
    public void Equal_document_content_does_not_invalidate_generation_inputs()
    {
        const string content = """
            {"Greeting":{"en-US":"Hello"}}
            """;
        var firstDocument = Document("C:\\project\\Culture.json", content);
        var replacement = Document("C:\\project\\Culture.json", content);
        var syntaxTree = CSharpSyntaxTree.ParseText("internal sealed class Input;");
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new CultureGenerator().AsSourceGenerator()],
            [firstDocument],
            (CSharpParseOptions)syntaxTree.Options,
            new TestAnalyzerConfigOptionsProvider(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            ),
            new GeneratorDriverOptions(
                IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true
            )
        );

        driver = driver.RunGenerators(compilation);
        driver = driver.ReplaceAdditionalText(firstDocument, replacement);
        driver = driver.RunGenerators(compilation);

        var steps = driver
            .GetRunResult()
            .Results.Single()
            .TrackedSteps["CultureGenerationInputs"];
        Assert.All(
            steps.SelectMany(step => step.Outputs),
            output =>
                Assert.Contains(
                    output.Reason,
                    new[]
                    {
                        IncrementalStepRunReason.Cached,
                        IncrementalStepRunReason.Unchanged,
                    }
                )
        );
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
            options["build_property.ArkheideEssentialCultureGeneratorEnabled"] = enabled;
        }

        if (targetNamespace is not null)
        {
            options["build_property.ArkheideEssentialCultureNamespace"] = targetNamespace;
        }

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new CultureGenerator().AsSourceGenerator()],
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

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values)
        : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) =>
            values.TryGetValue(key, out value!);
    }
}
