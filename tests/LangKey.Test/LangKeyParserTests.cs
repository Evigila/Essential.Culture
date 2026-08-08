using GeneratedLangKey = ArkheideSystem.LangKey.Test.Generated.LangKey;

namespace ArkheideSystem.LangKey.Test;

public sealed class LangKeyParserTests
{
    [Fact]
    public void GeneratedFacade_ExposesStableTokens()
    {
        Assert.Equal("LangKey.Title_Hello", GeneratedLangKey.Title_Hello);
    }

    [Fact]
    public void Resolver_SurfaceCannotMutateGlobalParserState()
    {
        var resolverCurrent = typeof(ILangKeyResolver).GetProperty(
            nameof(ILangKeyResolver.Current)
        );
        var parserCurrent = typeof(ILangKeyParser).GetProperty(nameof(ILangKeyParser.Current));

        Assert.NotNull(resolverCurrent);
        Assert.NotNull(parserCurrent);
        Assert.False(resolverCurrent.CanWrite);
        Assert.True(parserCurrent.CanWrite);
        Assert.DoesNotContain(
            typeof(ILangKeyResolver).GetMethods(),
            method => method.Name == nameof(ILangKeyParser.Reload)
        );
    }

    [Fact]
    public void Parser_ResolvesAndSwitchesCultures()
    {
        var parser = new LangKeyParser(GetDocumentPath(), "en-US");
        var changes = new List<LangKeyChangedEventArgs>();
        parser.Changed += (_, change) => changes.Add(change);

        Assert.Equal("Hello World!", parser.Get(GeneratedLangKey.Title_Hello));
        parser.Current = "zh_CN";

        Assert.Equal("zh-CN", parser.Current);
        Assert.Equal("你好 世界！", parser.Get(GeneratedLangKey.Title_Hello));
        Assert.Equal("数量：3", parser.Format(GeneratedLangKey.Message_Count, 3));
        Assert.Equal(LangKeyChangeKind.CultureChanged, Assert.Single(changes).Kind);
    }

    [Fact]
    public void Parser_FollowsFrameworkIndependentCultureSourceUntilDisposed()
    {
        var source = new TestCultureSource("en-US");
        var parser = new LangKeyParser(GetDocumentPath(), source);

        source.ChangeTo("zh-CN");
        Assert.Equal("zh-CN", parser.Current);

        parser.Dispose();
        source.ChangeTo("en-US");
        Assert.Equal("zh-CN", parser.Current);
    }

    [Fact]
    public void Parser_RejectsKeysContainingDots()
    {
        var path = Path.Combine(Path.GetTempPath(), $"LangKey-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, "{\"Title.Hello\":{\"en-US\":\"Hello\"}}");
            var error = Assert.Throws<InvalidDataException>(() =>
                new LangKeyParser(path, "en-US")
            );

            Assert.Contains("replace dots with underscores", error.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string GetDocumentPath() =>
        Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "LangKey.json")
        );

    private sealed class TestCultureSource(string currentCulture) : ILangKeyCultureSource
    {
        public string CurrentCulture { get; private set; } = currentCulture;

        public event EventHandler<LangKeyCultureChangedEventArgs>? Changed;

        public void ChangeTo(string culture)
        {
            CurrentCulture = culture;
            Changed?.Invoke(this, new LangKeyCultureChangedEventArgs(culture));
        }
    }
}
