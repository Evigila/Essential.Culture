using GeneratedKey = Arkheide.Essential.Culture.Key;

namespace Arkheide.Essential.Culture.Test;

public sealed class LocalizerTests
{
    [Fact]
    public void GeneratedFacade_ExposesStableTokens()
    {
        Assert.Equal("Key.Title_Hello", GeneratedKey.Title_Hello);
    }

    [Fact]
    public void StaticParse_ResolvesFormatsAndTriesWithoutObtainingAService()
    {
        Localizer.Current.SetCulture("en-US");

        Assert.Equal("Hello World!", Localizer.Parse(GeneratedKey.Title_Hello));
        Assert.Equal("Count: 3", Localizer.Parse(GeneratedKey.Message_Count, 3));
        Assert.True(Localizer.TryParse(GeneratedKey.Title_Hello, out var value));
        Assert.Equal("Hello World!", value);
        Assert.False(Localizer.TryParse("Key.Does_Not_Exist", out value));
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void Current_ChangesCultureAndRaisesOneEventAfterStateIsCommitted()
    {
        Localizer.Current.SetCulture("en-US");
        var observedCultures = new List<string>();
        EventHandler handler = (_, _) => observedCultures.Add(Localizer.Current.Culture);
        Localizer.Current.Changed += handler;
        try
        {
            Localizer.Current.SetCulture("zh_CN");
            Localizer.Current.SetCulture("zh-CN");

            Assert.Equal("zh-CN", Localizer.Current.Culture);
            Assert.Equal("你好 世界！", Localizer.Parse(GeneratedKey.Title_Hello));
            Assert.Equal("数量：3", Localizer.Parse(GeneratedKey.Message_Count, 3));
            Assert.Equal(["zh-CN"], observedCultures);
        }
        finally
        {
            Localizer.Current.Changed -= handler;
            Localizer.Current.SetCulture("en-US");
        }
    }

    [Fact]
    public void StaticSurface_ReportsCulturesAndPreservesUnknownTokens()
    {
        Assert.Contains("en-US", Localizer.Current.AvailableCultures);
        Assert.Contains("zh-CN", Localizer.Current.AvailableCultures);
        Assert.True(Localizer.Contains(GeneratedKey.Title_Hello));
        Assert.False(Localizer.Contains("Key.Does_Not_Exist"));
        Assert.Equal("Key.Does_Not_Exist", Localizer.Parse("Key.Does_Not_Exist"));
    }

    [Fact]
    public void RuntimeValidation_RejectsIllegalKeys()
    {
        WithDocument(
            """
            {"Title.Hello":{"en-US":"Hello"}}
            """,
            path =>
            {
                var error = Assert.Throws<InvalidDataException>(() =>
                    new LocalizationRuntime(path, "en-US")
                );
                Assert.Contains("contains illegal key", error.Message);
            }
        );
    }

    [Fact]
    public void CompositeFormats_AcceptEscapedBracesAndRejectInvalidSyntax()
    {
        WithDocument(
            """
            {"Value":{"en-US":"{{value}} = {0:0.00}"}}
            """,
            path =>
            {
                var runtime = new LocalizationRuntime(path, "en-US");
                Assert.Equal("{value} = 1.50", runtime.Parse("Key.Value", 1.5));
            }
        );

        WithDocument(
            """
            {"Value":{"en-US":"broken {0"}}
            """,
            path =>
            {
                var error = Assert.Throws<InvalidDataException>(() =>
                    new LocalizationRuntime(path, "en-US")
                );
                Assert.Contains("invalid composite format", error.Message);
            }
        );
    }

    [Fact]
    public void CustomCulture_UsesFallbackTranslationAndInvariantFormatting()
    {
        WithDocument(
            """
            {"Value":{"en-US":"Value: {0:0.0}"}}
            """,
            path =>
            {
                var runtime = new LocalizationRuntime(path, "x-demo");
                Assert.Equal("x-Demo", runtime.Culture);
                Assert.Equal("Value: 1.5", runtime.Parse("Key.Value", 1.5));
            }
        );
    }

    [Fact]
    public void ReadsRemainConsistentWhileCultureChanges()
    {
        WithDocument(
            """
            {"Value":{"en-US":"Hello","zh-CN":"你好"}}
            """,
            path =>
            {
                var runtime = new LocalizationRuntime(path, "en-US");
                var values = new System.Collections.Concurrent.ConcurrentBag<string>();
                var reads = Task.Run(() =>
                {
                    for (var index = 0; index < 2_000; index++)
                    {
                        values.Add(runtime.Parse("Key.Value"));
                    }
                });
                var writes = Task.Run(() =>
                {
                    for (var index = 0; index < 100; index++)
                    {
                        runtime.SetCulture(index % 2 == 0 ? "zh-CN" : "en-US");
                    }
                });

                Task.WaitAll(reads, writes);
                Assert.All(values, value => Assert.Contains(value, new[] { "Hello", "你好" }));
            }
        );
    }

    private static void WithDocument(string content, Action<string> action)
    {
        var path = Path.Combine(Path.GetTempPath(), $"Culture-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, content);
            action(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
