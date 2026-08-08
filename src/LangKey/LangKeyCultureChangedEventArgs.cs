namespace ArkheideSystem.LangKey;

/// <summary>Provides the culture selected by an <see cref="ILangKeyCultureSource" />.</summary>
public sealed class LangKeyCultureChangedEventArgs(string currentCulture) : EventArgs
{
    /// <summary>Gets the newly selected culture.</summary>
    public string CurrentCulture { get; } = currentCulture;
}
