namespace Arkheide.Essential.Culture;

/// <summary>
/// Provides the process-wide Arkheide Essential Culture entry point.
/// </summary>
public static class Localizer
{
    /// <summary>Gets the current localization state, loading Culture.json on first use.</summary>
    public static ILocalizer Current => LocalizationRuntime.Shared;

    /// <summary>Resolves a stable token using the current culture.</summary>
    public static string Parse(string token) => Current.Parse(token);

    /// <summary>Resolves and formats a stable token using the current culture.</summary>
    public static string Parse(string token, params object?[] arguments) =>
        Current.Parse(token, arguments);

    /// <summary>Returns whether a raw key or prefixed token is declared.</summary>
    public static bool Contains(string token) => Current.Contains(token);
}
