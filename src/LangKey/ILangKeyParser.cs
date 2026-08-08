namespace ArkheideSystem.LangKey;

/// <summary>
/// Resolves stable <c>LangKey.&lt;name&gt;</c> tokens from a single localization document.
/// </summary>
public interface ILangKeyParser : ILangKeyResolver, IDisposable
{
    /// <summary>Gets or sets the current culture name.</summary>
    new string Current { get; set; }

    /// <summary>Gets the fallback culture used when the current culture has no value.</summary>
    string Fallback { get; }

    /// <summary>Gets the cultures declared by the localization document.</summary>
    IReadOnlyList<string> AvailableCultures { get; }

    /// <summary>Gets the raw keys declared by the localization document.</summary>
    IReadOnlySet<string> Keys { get; }

    /// <summary>Reloads and validates the localization document from disk.</summary>
    void Reload();
}
