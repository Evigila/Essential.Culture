namespace ArkheideSystem.LangKey;

/// <summary>Allows a framework data item to refresh values that originate from LangKey tokens.</summary>
public interface ILangKeyLocalizable
{
    /// <summary>Refreshes the item's localized values through the supplied resolver.</summary>
    void ApplyLocalization(ILangKeyResolver resolver);
}
