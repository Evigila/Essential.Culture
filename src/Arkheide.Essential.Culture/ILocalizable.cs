namespace Arkheide.Essential.Culture;

/// <summary>Allows a framework data item to refresh values derived from generated keys.</summary>
public interface ILocalizable
{
    /// <summary>Refreshes the item's localized values through the global localizer.</summary>
    void ApplyLocalization();
}
