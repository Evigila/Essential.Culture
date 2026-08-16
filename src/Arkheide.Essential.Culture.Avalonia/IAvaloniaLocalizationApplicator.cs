using global::Avalonia;

namespace Arkheide.Essential.Culture.Avalonia;

/// <summary>Applies Key tokens to supported Avalonia display properties.</summary>
public interface IAvaloniaLocalizationApplicator : IDisposable
{
    /// <summary>Starts automatic Loaded handling and culture-change refresh for an application.</summary>
    void Start(Application application);

    /// <summary>Stops automatic Loaded handling and culture-change refresh.</summary>
    void Stop();

    /// <summary>
    /// Applies localization to an Avalonia visual tree immediately. This method must be called
    /// on the UI thread.
    /// </summary>
    void Apply(Visual root);
}
