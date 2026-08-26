namespace ArkheideSystem.Essential.Culture.WinUI;

internal static class WinUILocalizationMarker
{
    private const string Prefix = "\uE000ArkheideSystem.Essential.Culture.WinUI.Localize\uE001";
    private const char Suffix = '\uE002';

    public static string Create(string token)
    {
        if (!token.StartsWith(KeyToken.Prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"A localization token must start with '{KeyToken.Prefix}'.",
                nameof(token)
            );
        }

        return string.Concat(Prefix, token, Suffix);
    }

    public static bool TryExtract(string value, out string token)
    {
        if (
            value.Length <= Prefix.Length + 1
            || !value.StartsWith(Prefix, StringComparison.Ordinal)
            || value[^1] != Suffix
        )
        {
            token = string.Empty;
            return false;
        }

        token = value.Substring(Prefix.Length, value.Length - Prefix.Length - 1);
        if (token.StartsWith(KeyToken.Prefix, StringComparison.Ordinal))
        {
            return true;
        }

        token = string.Empty;
        return false;
    }
}
