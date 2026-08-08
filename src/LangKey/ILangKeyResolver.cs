namespace ArkheideSystem.LangKey;

/// <summary>Resolves stable LangKey tokens without exposing catalog mutation operations.</summary>
public interface ILangKeyResolver
{
    /// <summary>Gets the current culture name.</summary>
    string Current { get; }

    /// <summary>Raised after the current culture changes or the document is reloaded.</summary>
    event EventHandler<LangKeyChangedEventArgs>? Changed;

    /// <summary>Returns whether a raw key or prefixed LangKey token is declared.</summary>
    bool Contains(string key);

    /// <summary>Resolves a raw key or prefixed LangKey token.</summary>
    string Get(string key);

    /// <summary>Resolves and formats a value using the current culture.</summary>
    string Format(string key, params object?[] arguments);
}
