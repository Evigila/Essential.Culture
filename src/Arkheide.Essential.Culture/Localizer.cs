namespace Arkheide.Essential.Culture;

/// <summary>
/// Provides the process-wide Arkheide Essential Culture entry point.
/// </summary>
public static class Localizer
{
    /// <summary>Provides the dynamic culture state.</summary>
    public static class Current
    {
        /// <summary>Gets the currently selected culture name.</summary>
        public static string Culture => LocalizationRuntime.Shared.Culture;

        /// <summary>Gets the cultures declared by Culture.json.</summary>
        public static IReadOnlyList<string> AvailableCultures =>
            LocalizationRuntime.Shared.AvailableCultures;

        /// <summary>Raised after the current culture changes.</summary>
        public static event EventHandler? Changed
        {
            add => LocalizationRuntime.Shared.Changed += value;
            remove => LocalizationRuntime.Shared.Changed -= value;
        }

        /// <summary>Changes the culture used by subsequent Parse calls.</summary>
        public static void SetCulture(string culture) =>
            LocalizationRuntime.Shared.SetCulture(culture);
    }

    /// <summary>Resolves a stable token using the current culture.</summary>
    public static string Parse(string token) => LocalizationRuntime.Shared.Parse(token);

    /// <summary>Resolves and formats a stable token using the current culture.</summary>
    public static string Parse(string token, params object?[] arguments) =>
        LocalizationRuntime.Shared.Parse(token, arguments);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    public static string Parse<TArg0>(string token, TArg0 argument0) =>
        LocalizationRuntime.Shared.Parse(token, argument0);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    public static string Parse<TArg0, TArg1>(string token, TArg0 argument0, TArg1 argument1) =>
        LocalizationRuntime.Shared.Parse(token, argument0, argument1);

    /// <summary>Resolves and formats a token without allocating an argument array.</summary>
    public static string Parse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2
    ) => LocalizationRuntime.Shared.Parse(token, argument0, argument1, argument2);

    /// <summary>Attempts to resolve a stable token using the current culture.</summary>
    public static bool TryParse(string token, out string value) =>
        LocalizationRuntime.Shared.TryParse(token, out value);

    /// <summary>Attempts to resolve and format a stable token using the current culture.</summary>
    public static bool TryParse(string token, object?[] arguments, out string value) =>
        LocalizationRuntime.Shared.TryParse(token, arguments, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    public static bool TryParse<TArg0>(string token, TArg0 argument0, out string value) =>
        LocalizationRuntime.Shared.TryParse(token, argument0, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    public static bool TryParse<TArg0, TArg1>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        out string value
    ) => LocalizationRuntime.Shared.TryParse(token, argument0, argument1, out value);

    /// <summary>Attempts to resolve and format a token without allocating an argument array.</summary>
    public static bool TryParse<TArg0, TArg1, TArg2>(
        string token,
        TArg0 argument0,
        TArg1 argument1,
        TArg2 argument2,
        out string value
    ) => LocalizationRuntime.Shared.TryParse(token, argument0, argument1, argument2, out value);

    /// <summary>Returns whether a raw key or prefixed token is declared.</summary>
    public static bool Contains(string token) => LocalizationRuntime.Shared.Contains(token);
}
