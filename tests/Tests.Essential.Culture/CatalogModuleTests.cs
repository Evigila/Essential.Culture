namespace ArkheideSystem.Essential.Culture.Test;

public sealed class CatalogModuleTests
{
    [Fact]
    public void ModulesComposeGlobalKeysAndResolveTheirDifferentOptionalLanguages()
    {
        using var files = new ModuleFiles(
            """{"Common":{"en-US":"Common","fr":"Commun"}}""",
            """{"Feature":{"en-US":"Feature","de-DE":"Funktion"}}"""
        );
        var catalog = LocalizationCatalog.FromFiles(files.Paths);
        Assert.Equal(["de-DE", "en-US", "fr"], catalog.DeclaredCultures);
        var context = new LocalizationContext(catalog, "fr-CA");
        Assert.Equal("Commun", context.Parse("Key.Common"));
        Assert.Equal("Feature", context.Parse("Feature"));
        context.SetCulture("de-DE");
        Assert.Equal("Common", context.Parse("Common"));
        Assert.Equal("Funktion", context.Parse("Key.Feature"));
        var reversed = new LocalizationContext(LocalizationCatalog.FromFiles(files.Paths.Reverse()), "de-DE");
        Assert.Equal(context.Parse("Common"), reversed.Parse("Common"));
        Assert.Equal(context.Parse("Feature"), reversed.Parse("Feature"));
    }

    [Fact]
    public void ModulePoliciesKeepOnlyNecessaryPermittedParentAndFallbackTranslations()
    {
        using var files = new ModuleFiles(
            """{"Common":{"en-US":"Common","fr":"Commun","ja-JP":"Unused"}}""",
            """{"Feature":{"en-US":"Feature","fr-CA":"Fonction","de-DE":"Unused"}}"""
        );
        var options = new CatalogLoadOptions(["fr-CA"], ["ja-JP"]);
        var catalog = LocalizationCatalog.FromFiles(files.Paths, "en-US", options);
        Assert.Equal(["fr-CA"], catalog.AvailableCultures);
        Assert.Equal(["de-DE", "en-US", "fr", "fr-CA", "ja-JP"], catalog.DeclaredCultures);
        Assert.Equal(["en-US", "fr", "fr-CA"], CatalogPolicyTests.RetainedCultures(catalog));
        var context = new LocalizationContext(catalog, "fr-CA");
        Assert.Equal("Commun", context.Parse("Common"));
        Assert.Equal("Fonction", context.Parse("Feature"));
        Assert.Throws<ArgumentException>(() => context.SetCulture("de-DE"));
    }

    [Fact]
    public void DuplicateGlobalKeysReportBothSourcePaths()
    {
        using var files = new ModuleFiles(
            """{"Shared":{"en-US":"First"}}""",
            """{"Shared":{"en-US":"Second"}}"""
        );
        var error = Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromFiles(files.Paths));
        Assert.Contains("Shared", error.Message, StringComparison.Ordinal);
        Assert.All(files.Paths, path => Assert.Contains(path, error.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void EquivalentDuplicatePathsAndEmptyModuleListsAreRejected()
    {
        using var files = new ModuleFiles("""{"Common":{"en-US":"Common"}}""");
        var path = files.Paths[0];
        var equivalent = Path.Combine(Path.GetDirectoryName(path)!, ".", Path.GetFileName(path));
        Assert.Throws<ArgumentException>(() => LocalizationCatalog.FromFiles([path, equivalent]));
        Assert.Throws<ArgumentException>(() => LocalizationCatalog.FromFiles([]));
        Assert.Throws<ArgumentNullException>(() => LocalizationCatalog.FromFiles(null!));
        Assert.Throws<FileNotFoundException>(() => LocalizationCatalog.FromFiles([Path.Combine(Path.GetDirectoryName(path)!, "missing.json")]));
    }

    [Fact]
    public void EachModuleMustRemainUniformAndContainTheConfiguredFallback()
    {
        using var nonuniform = new ModuleFiles("""{"A":{"en-US":"A","fr":"A"},"B":{"en-US":"B"}}""");
        Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromFiles(nonuniform.Paths));
        using var missingFallback = new ModuleFiles("""{"A":{"en-US":"A"}}""", """{"B":{"fr":"B"}}""");
        var error = Assert.Throws<InvalidDataException>(() => LocalizationCatalog.FromFiles(missingFallback.Paths));
        Assert.Contains(missingFallback.Paths[1], error.Message, StringComparison.Ordinal);
        Assert.Contains("fallback", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFilteredExplicitFileUsesTheSamePolicyAsOtherFactories()
    {
        using var files = new ModuleFiles("""{"Common":{"en-US":"Common","fr":"Commun"}}""");
        var catalog = LocalizationCatalog.FromFile(files.Paths[0], "en-US", new CatalogLoadOptions(["fr-CA"]));
        Assert.Equal("Commun", new LocalizationContext(catalog, "fr-CA").Parse("Common"));
        Assert.Equal(["fr-CA"], catalog.AvailableCultures);
    }

    private sealed class ModuleFiles : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "culture-module-tests", Guid.NewGuid().ToString("N"));

        internal ModuleFiles(params string[] documents)
        {
            Directory.CreateDirectory(directory);
            Paths = documents.Select((json, index) =>
            {
                var path = Path.Combine(directory, $"Culture.Module{index}.json");
                File.WriteAllText(path, json);
                return path;
            }).ToArray();
        }

        internal string[] Paths { get; }

        public void Dispose() => Directory.Delete(directory, recursive: true);
    }
}
