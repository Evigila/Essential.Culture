namespace ArkheideSystem.LangKey;

/// <summary>Provides the current culture and reports culture changes from an external system.</summary>
public interface ILangKeyCultureSource
{
    /// <summary>Gets the culture currently selected by the external system.</summary>
    string CurrentCulture { get; }

    /// <summary>Raised when <see cref="CurrentCulture" /> changes.</summary>
    event EventHandler<LangKeyCultureChangedEventArgs>? Changed;
}
