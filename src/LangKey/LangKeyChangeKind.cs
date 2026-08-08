namespace ArkheideSystem.LangKey;

/// <summary>Describes a change reported by an <see cref="ILangKeyParser"/>.</summary>
public enum LangKeyChangeKind
{
    /// <summary>The selected culture changed.</summary>
    CultureChanged,

    /// <summary>The localization document was reloaded from disk.</summary>
    DocumentReloaded,
}
