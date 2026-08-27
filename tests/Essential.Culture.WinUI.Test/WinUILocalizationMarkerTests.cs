using ArkheideSystem.Essential.Culture.WinUI;
using Xunit;

namespace ArkheideSystem.Essential.Culture.WinUI.Test;

public sealed class WinUILocalizationMarkerTests
{
    [Fact]
    public void Static_marker_preserves_token()
    {
        var marker = WinUILocalizationMarker.Create("Key.Greeting");

        Assert.True(WinUILocalizationMarker.TryExtract(marker, out var token));
        Assert.Equal("Key.Greeting", token);
    }

    [Fact]
    public void Dynamic_marker_has_no_embedded_token()
    {
        var marker = WinUILocalizationMarker.CreateDynamic();

        Assert.True(WinUILocalizationMarker.TryExtract(marker, out var token));
        Assert.Null(token);
    }

    [Fact]
    public void Ordinary_text_is_not_a_marker()
    {
        Assert.False(WinUILocalizationMarker.TryExtract("Hello", out var token));
        Assert.Null(token);
    }

    [Fact]
    public void Static_marker_rejects_non_token_value()
    {
        Assert.Throws<ArgumentException>(() => WinUILocalizationMarker.Create("Greeting"));
    }
}
