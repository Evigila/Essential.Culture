namespace Arkheide.Essential.Culture;

/// <summary>
/// Resolves stable <c>Key.*</c> tokens and owns one localization culture state.
/// </summary>
public interface ILocalizer
{
    /// <summary>Resolves a stable token using the current culture.</summary>
    string Parse(string token);

    /// <summary>Resolves and formats a stable token using the current culture.</summary>
    string Parse(string token, params object?[] arguments);

    /// <summary>Returns whether a raw key or prefixed token is declared.</summary>
    bool Contains(string token);

    /// <summary>Gets the currently selected culture name.</summary>
    string Culture { get; }

    /// <summary>Gets the cultures declared by Culture.json.</summary>
    IReadOnlyList<string> AvailableCultures { get; }

    /// <summary>Raised after the current culture changes.</summary>
    event EventHandler? Changed;

    /// <summary>Changes the process-wide culture used by subsequent Parse calls.</summary>
    void SetCulture(string culture);
}
