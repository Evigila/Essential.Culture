using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace ArkheideSystem.Essential.Culture.Generator.Test;

public sealed class ModuleGenerationTests
{
    [Fact]
    public void Explicit_modules_preserve_tokens_and_generate_a_sorted_readonly_resource_list()
    {
        var driver = Driver(
            [File("Culture.json", """{"Greeting":{"en-US":"Hello","zh-CN":"浣犲ソ"}}"""),
             File("Culture.Feature.json", """{"Feature_Message":{"en-US":"Feature","fr":"Fonction"}}""")],
            metadata: Module("Culture.Feature.json", "Feature", "Features/Culture.Feature.json"));
        var result = Run(driver);

        Assert.Empty(result.Diagnostics);
        var generated = result.Results.Single().GeneratedSources;
        Assert.Equal(2, generated.Length);
        var keys = Source(result, "Key.g.cs");
        Assert.Contains("public static string Greeting => \"Key.Greeting\";", keys);
        Assert.Contains("public static string Feature_Message => \"Key.Feature_Message\";", keys);
        var resources = Source(result, "CultureResources.g.cs");
        Assert.Contains("internal static class CultureResources", resources);
        Assert.Contains("IReadOnlyList<string> Files", resources);
        Assert.Contains("Array.AsReadOnly", resources);
        Assert.Contains("internal const string FallbackCulture = \"en-US\";", resources);
        Assert.True(resources.IndexOf("\"Culture.json\"", StringComparison.Ordinal) < resources.IndexOf("\"Features/Culture.Feature.json\"", StringComparison.Ordinal));
    }

    [Fact]
    public void Discovery_is_opt_in_and_does_not_treat_the_options_filename_as_translations()
    {
        var files = new[]
        {
            File("Culture.json", """{"Greeting":{"en-US":"Hello"}}"""),
            File("Culture.Feature.json", """{"Feature":{"en-US":"Feature"}}"""),
            File("Culture.options.json", "not translation JSON")
        };
        var legacy = Run(Driver(files));
        Assert.Empty(legacy.Diagnostics);
        Assert.Single(legacy.Results.Single().GeneratedSources);
        Assert.DoesNotContain("Feature =>", Source(legacy, "Key.g.cs"));
        var modular = Run(Driver(files, modules: "true"));
        Assert.Empty(modular.Diagnostics);
        Assert.Contains("Feature =>", Source(modular, "Key.g.cs"));
        Assert.DoesNotContain("Culture.options.json", Source(modular, "CultureResources.g.cs"));
    }

    [Fact]
    public void Cross_file_duplicate_keys_report_both_source_locations()
    {
        var result = Run(Driver(
            [File("Culture.json", "{\n  \"Greeting\":{\"en-US\":\"Hello\"}\n}"),
             File("Culture.Feature.json", "{\n  \"Greeting\":{\"en-US\":\"Again\"}\n}")], modules: "true"));
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("AEC007", diagnostic.Id);
        Assert.EndsWith("Culture.json", diagnostic.Location.GetLineSpan().Path);
        Assert.EndsWith("Culture.Feature.json", Assert.Single(diagnostic.AdditionalLocations).GetLineSpan().Path);
        Assert.Equal(1, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        Assert.Contains("Culture.Feature.json", diagnostic.GetMessage());
        Assert.Contains("Culture.json", diagnostic.GetMessage());
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Theory]
    [InlineData("Same", "Same", "Culture.One.json", "Culture.Two.json")]
    [InlineData("One", "Two", "Features/Text.json", "features/text.json")]
    public void Duplicate_module_ids_and_deployment_paths_are_rejected(string firstId, string secondId, string firstPath, string secondPath)
    {
        var metadata = Module("One.json", firstId, firstPath);
        metadata.Add("Two.json", ModuleValues(secondId, secondPath));
        var result = Run(Driver(
            [File("One.json", """{"One":{"en-US":"One"}}"""), File("Two.json", """{"Two":{"en-US":"Two"}}""")], metadata: metadata));
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("AEC006", diagnostic.Id);
        Assert.Single(diagnostic.AdditionalLocations);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Theory]
    [InlineData("../Outside.json")]
    [InlineData("/Outside.json")]
    [InlineData("C:\\Outside.json")]
    [InlineData("Features//Text.json")]
    [InlineData("Features/./Text.json")]
    [InlineData("Features/CON.json")]
    [InlineData("Program.dll")]
    [InlineData(" ")]
    public void Unsafe_deployment_paths_are_rejected(string path)
    {
        var result = Run(Driver([File("Feature.json", """{"Feature":{"en-US":"Text"}}""")],
            metadata: Module("Feature.json", "Feature", path)));
        Assert.Equal("AEC006", Assert.Single(result.Diagnostics).Id);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Fact]
    public void Whitespace_module_identity_is_rejected_for_host_managed_metadata()
    {
        var result = Run(Driver([File("Feature.json", """{"Feature":{"en-US":"Text"}}""")],
            metadata: Module("Feature.json", " ", "Feature.json")));
        Assert.Equal("AEC006", Assert.Single(result.Diagnostics).Id);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Fact]
    public void Configured_fallback_is_normalized_and_required_by_every_module()
    {
        var file = File("Feature.json", """{"Feature":{"pt-BR":"Texto"}}""");
        var metadata = Module("Feature.json", "Feature", "Feature.json");
        var valid = Run(Driver([file], fallback: "pt_br", metadata: metadata));
        Assert.Empty(valid.Diagnostics);
        Assert.Contains("FallbackCulture = \"pt-BR\"", Source(valid, "CultureResources.g.cs"));
        var invalid = Run(Driver([file], metadata: metadata));
        Assert.Equal("AEC002", Assert.Single(invalid.Diagnostics).Id);
        Assert.Contains("fallback culture 'en-US'", invalid.Diagnostics.Single().GetMessage());
    }

    [Theory]
    [InlineData("sometimes", "en-US")]
    [InlineData("true", "en--US")]
    public void Invalid_module_configuration_is_diagnosed(string modules, string fallback)
    {
        var result = Run(Driver([File("Culture.json", """{"Text":{"en-US":"Text"}}""")], modules: modules, fallback: fallback));
        Assert.Equal("AEC006", Assert.Single(result.Diagnostics).Id);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Text\":{}}")]
    [InlineData("{\"Text\":{\"fr\":\"Texte\"}}")]
    [InlineData("{\"Text\":{\"en-US\":null}}")]
    [InlineData("{\"Text\":{\"en-US\":1}}")]
    [InlineData("{\"Text\":{\"en-US\":[]}}")]
    [InlineData("{\"Text\":{\"en-US\":{}}}")]
    [InlineData("{\"Text\":{\"en-US\":\" \"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"Text\",\"en_us\":\"Again\"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"Text\",\"en--US\":\"Again\"}}")]
    [InlineData("{\"One\":{\"en-US\":\"One\",\"fr\":\"Un\"},\"Two\":{\"en-US\":\"Two\"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"Value {0}\",\"fr\":\"Valeur {1}\"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"Value {0} {0}\",\"fr\":\"Valeur {0}\"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"broken {0\"}}")]
    [InlineData("{\"Text\":{\"en-US\":\"Text\"},}")]
    [InlineData("{\"Text\":{\"en-US\":\"Text\"}} trailing")]
    public void Full_authored_semantic_validation_runs_before_generation(string json)
    {
        var result = Run(Driver([File("Culture.json", json)]));
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("AEC002", diagnostic.Id);
        Assert.NotEqual(Location.None, diagnostic.Location);
        Assert.Empty(result.Results.Single().GeneratedSources);
    }

    [Theory]
    [InlineData("{{value}} = {0:0.00}")]
    [InlineData("{0, -10:N2}")]
    [InlineData("{0:}")]
    [InlineData("{{{0}}}")]
    [InlineData("{01} {0} {1}")]
    [InlineData("{0:}}}")]
    [InlineData("{ 0}")]
    [InlineData("{0,+10}")]
    [InlineData("{0,- 10}")]
    [InlineData("{0:{x}}")]
    [InlineData("Text }")]
    [InlineData("Text {")]
    public void Composite_format_validation_matches_the_target_runtime(string format)
    {
        var runtimeValid = true;
        try { _ = CompositeFormat.Parse(format); }
        catch (FormatException) { runtimeValid = false; }
        var json = JsonSerializer.Serialize(new Dictionary<string, Dictionary<string, string>>
        {
            ["Text"] = new() { ["en-US"] = format }
        });
        var result = Run(Driver([File("Culture.json", json)]));
        Assert.Equal(runtimeValid, result.Diagnostics.IsEmpty);
    }

    [Fact]
    public void Translation_only_edits_reparse_one_module_and_cache_the_key_output()
    {
        var first = File("Culture.One.json", """{"One":{"en-US":"One"}}""");
        var second = File("Culture.Two.json", """{"Two":{"en-US":"Two"}}""");
        var replacement = File("Culture.One.json", """{"One":{"en-US":"Changed translation"}}""");
        var driver = Driver([first, second], modules: "true", tracked: true);
        driver = driver.RunGenerators(Compilation());
        var original = driver.GetRunResult();
        driver = driver.ReplaceAdditionalText(first, replacement).RunGenerators(Compilation());
        var result = driver.GetRunResult();
        Assert.Equal(Source(original, "Key.g.cs"), Source(result, "Key.g.cs"));
        Assert.Equal(Source(original, "CultureResources.g.cs"), Source(result, "CultureResources.g.cs"));
        var steps = result.Results.Single().TrackedSteps;
        Assert.All(steps["CultureGenerationInputs"].SelectMany(step => step.Outputs), output =>
            Assert.Contains(output.Reason, new[] { IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged }));
        var parseReasons = steps["CultureParsedResources"].SelectMany(step => step.Outputs).Select(output => output.Reason).ToArray();
        Assert.Contains(IncrementalStepRunReason.Modified, parseReasons);
        Assert.Contains(IncrementalStepRunReason.Cached, parseReasons);
    }

    [Fact]
    public void Moving_a_key_changes_ownership_but_keeps_the_global_tokens_stable()
    {
        var first = File("Culture.One.json", """{"One":{"en-US":"One"},"Moved":{"en-US":"Move"}}""");
        var second = File("Culture.Two.json", """{"Two":{"en-US":"Two"}}""");
        var driver = Driver([first, second], modules: "true", tracked: true).RunGenerators(Compilation());
        var keys = Source(driver.GetRunResult(), "Key.g.cs");
        driver = driver.ReplaceAdditionalText(first, File("Culture.One.json", """{"One":{"en-US":"One"}}"""))
            .ReplaceAdditionalText(second, File("Culture.Two.json", """{"Two":{"en-US":"Two"},"Moved":{"en-US":"Move"}}"""))
            .RunGenerators(Compilation());
        Assert.Empty(driver.GetRunResult().Diagnostics);
        Assert.Equal(keys, Source(driver.GetRunResult(), "Key.g.cs"));
        Assert.Contains(driver.GetRunResult().Results.Single().TrackedSteps["CultureGenerationInputs"].SelectMany(step => step.Outputs),
            output => output.Reason == IncrementalStepRunReason.Modified);
    }

    [Fact]
    public void Resource_order_does_not_change_generated_source()
    {
        var files = new[] { File("Culture.Two.json", """{"Two":{"en-US":"Two"}}"""), File("Culture.One.json", """{"One":{"en-US":"One"}}""") };
        var first = Run(Driver(files, modules: "true"));
        var second = Run(Driver(files.Reverse().ToArray(), modules: "true"));
        Assert.Equal(Source(first, "Key.g.cs"), Source(second, "Key.g.cs"));
        Assert.Equal(Source(first, "CultureResources.g.cs"), Source(second, "CultureResources.g.cs"));
    }

    [Theory]
    [InlineData("EssentialCultureGeneratorEnabled")]
    [InlineData("ArkheideEssentialCultureGeneratorEnabled")]
    public void Disabled_generation_does_not_read_or_parse_resource_text(string property)
    {
        var options = new Options(new() { ["build_property." + property] = " false " }, []);
        var driver = CSharpGeneratorDriver.Create([new CultureGenerator().AsSourceGenerator()],
            [new UnreadableResourceText()], optionsProvider: options,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true));
        var result = driver.RunGenerators(Compilation()).GetRunResult();
        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.Results.Single().GeneratedSources);
        Assert.False(result.Results.Single().TrackedSteps.ContainsKey("CultureParsedResources"));
    }

    private static CSharpCompilation Compilation() => CSharpCompilation.Create("ModuleTests",
        [CSharpSyntaxTree.ParseText("internal sealed class Input;")],
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)),
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static GeneratorDriver Driver(AdditionalText[] files, string? modules = null, string? fallback = null,
        Dictionary<string, Dictionary<string, string>>? metadata = null, bool tracked = false)
    {
        var values = new Dictionary<string, string>();
        if (modules is not null) values["build_property.EssentialCultureModulesEnabled"] = modules;
        if (fallback is not null) values["build_property.EssentialCultureFallbackCulture"] = fallback;
        return CSharpGeneratorDriver.Create([new CultureGenerator().AsSourceGenerator()], files,
            optionsProvider: new Options(values, metadata ?? []),
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, tracked));
    }

    private static GeneratorDriverRunResult Run(GeneratorDriver driver) => driver.RunGenerators(Compilation()).GetRunResult();
    private static string Source(GeneratorDriverRunResult result, string hint) =>
        Assert.Single(result.Results.Single().GeneratedSources, source => source.HintName == hint).SourceText.ToString();
    private static AdditionalText File(string name, string content) => new ResourceText(Path.Combine("C:\\project", name), content);
    private static Dictionary<string, Dictionary<string, string>> Module(string name, string id, string deployment) => new()
        { [name] = ModuleValues(id, deployment) };
    private static Dictionary<string, string> ModuleValues(string id, string deployment) => new()
    {
        ["build_metadata.AdditionalFiles.CultureModule"] = "true",
        ["build_metadata.AdditionalFiles.CultureModuleId"] = id,
        ["build_metadata.AdditionalFiles.CultureDeploymentPath"] = deployment
    };

    private sealed class ResourceText(string path, string text) : AdditionalText
    {
        public override string Path => path;
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
    }

    private sealed class UnreadableResourceText : AdditionalText
    {
        public override string Path => "C:\\project\\Culture.json";
        public override SourceText GetText(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Disabled generation must not read this text.");
    }

    private sealed class Options(Dictionary<string, string> values, Dictionary<string, Dictionary<string, string>> metadata) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions => new Values(values);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Values([]);
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new Values(metadata.GetValueOrDefault(Path.GetFileName(textFile.Path)) ?? []);
    }

    private sealed class Values(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }
}
