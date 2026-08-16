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
    public void StaticParse_ResolvesAndFormatsWithoutObtainingAService()
    {
        Localizer.Current.SetCulture("en-US");

        Assert.Equal("Hello World!", Localizer.Parse(GeneratedKey.Title_Hello));
        Assert.Equal("Count: 3", Localizer.Parse(GeneratedKey.Message_Count, 3));
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
    public void StaticSurface_ReportsCulturesAndUnknownTokens()
    {
        Assert.Contains("en-US", Localizer.Current.AvailableCultures);
        Assert.Contains("zh-CN", Localizer.Current.AvailableCultures);
        Assert.True(Localizer.Contains(GeneratedKey.Title_Hello));
        Assert.False(Localizer.Contains("Key.Does_Not_Exist"));
        Assert.Equal("Key.Does_Not_Exist", Localizer.Parse("Key.Does_Not_Exist"));
    }

    [Fact]
    public void RuntimeValidation_RejectsKeysContainingDots()
    {
        var path = Path.Combine(Path.GetTempPath(), $"Localizer-{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, "{\"Title.Hello\":{\"en-US\":\"Hello\"}}");

            var error = Assert.Throws<InvalidDataException>(() =>
                new LocalizationRuntime(path, "en-US")
            );

            Assert.Contains("replace dots with underscores", error.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
