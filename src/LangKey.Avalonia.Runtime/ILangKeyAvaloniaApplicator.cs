using global::Avalonia;

namespace ArkheideSystem.LangKey.Avalonia;

/// <summary>Applies LangKey tokens to supported Avalonia display properties.</summary>
public interface ILangKeyAvaloniaApplicator
{
    /// <summary>Starts automatic Loaded handling and culture-change refresh for an application.</summary>
    void Start(Application application);

    /// <summary>Stops automatic Loaded handling and culture-change refresh.</summary>
    void Stop();

    /// <summary>Applies localization to an Avalonia visual tree immediately.</summary>
    void Apply(Visual root);
}
